    #region --- LICENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015 Qualisys AB

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649
namespace QualisysRealTime.Unity.Skeleton
{
    class RTCharacterStream : MonoBehaviour
    {
        public string ActorMarkersPrefix = "";
        public bool ScaleMovementToSize = false;
        public bool UseIK = true;
        public bool UseFingers = false;
        public CharactersModel model = CharactersModel.Model1;
        public BoneRotations boneRotatation;
        public HeadCam headCam;

        internal Camera headCamera = new Camera();
        internal bool jointsFound = true;

        protected RTClient rtClient;
        protected Vector3 pos;
        protected List<LabeledMarker> markerData;
        protected BipedSkeleton skeleton;

        private SkeletonBuilder skeletonBuilder;
        private CharacterGameObjects charactersJoints = new CharacterGameObjects();
        private float scale = 0;

        /// <summary>
        /// Locating the joints of the character and find the scale by which the position should be applied to
        /// </summary>
        public void Start()
        {
            rtClient = RTClient.GetInstance();
            //Find all joints of the characters
            jointsFound = charactersJoints.SetLimbs(this.transform, UseFingers);
            // disable the animation
            var animation = this.GetComponent<Animation>();
            if (animation) animation.enabled = false;
        }
        /// <summary>
        /// Updates the rotation and position of the Character
        /// </summary>
        public void Update()
        {
            if (rtClient == null) rtClient = RTClient.GetInstance();
            if (!rtClient.GetStreamingStatus()) return;
            markerData = rtClient.Markers;
            if ((markerData == null || markerData.Count == 0))
            {
                Debug.LogError("The stream does not contain any markers");
                return;
            }
            if (skeletonBuilder != null) skeleton = skeletonBuilder.SolveSkeleton(markerData);
            else ResetSkeleton();
            SetAll();
            if (!headCam.UseHeadCamera && headCamera) DestroyCamera();
        }
        public void ResetSkeleton()
        {
            charactersJoints.SetLimbs(this.transform, UseFingers);
            skeletonBuilder = new SkeletonBuilder();
            skeletonBuilder.MarkerPrefix = ActorMarkersPrefix;
            skeletonBuilder.SolveWithIK = UseIK;
            if (markerData != null) skeleton = skeletonBuilder.SolveSkeleton(markerData);
            else skeleton = new BipedSkeleton();
            if (ScaleMovementToSize) scale = FindScale();
            else scale = 0;
        }

        /// <summary>
        /// Maps the built skeleton joint rotation to the characeters joints
        /// </summary>
        private void SetAll()
        {
            foreach (var b in skeleton.Root)
            {
                switch (b.Data.Name)
                {
                    case Joint.PELVIS:
                        SetJointRotation(charactersJoints.pelvis, b.Data, boneRotatation.hip);
                        if (charactersJoints.pelvis && !b.Data.Pos.IsNaN())
                        {
                            charactersJoints.pelvis.position = transform.position 
                                        + ((ScaleMovementToSize && scale > 0)?
                                            (b.Data.Pos * scale * transform.localScale.magnitude).Convert()
                                            : b.Data.Pos.Convert());
                        }
                        break;
                    case Joint.SPINE0:
                        if (charactersJoints.spine.Length > 0)
                            SetJointRotation(charactersJoints.spine[0], b.Data, boneRotatation.spine);
                        break;
                    case Joint.SPINE1:
                        if (charactersJoints.spine.Length > 1)
                            SetJointRotation(charactersJoints.spine[1], b.Data, boneRotatation.spine);
                        break;
                    case Joint.SPINE3:
                        if (charactersJoints.spine.Length > 2)
                            SetJointRotation(charactersJoints.spine[2], b.Data, boneRotatation.spine);
                        break;
                    case Joint.NECK:
                        SetJointRotation(charactersJoints.neck, b.Data, boneRotatation.neck);
                        break;
                    case Joint.HEAD:
                        if (headCam.UseHeadCamera)
                            SetCameraPosition(b.Data);
                        if (headCam.UseHeadCamera && !headCam.UseVRHeadSetRotation && headCamera)
                        {
                            SetJointRotation(charactersJoints.head, b.Data, boneRotatation.head);
                        }
                        else if (headCamera)
                        {
                            charactersJoints.head.rotation =
                                headCamera.transform.rotation * Quaternion.Euler(boneRotatation.headCamera);
                        }
                        else SetJointRotation(charactersJoints.head, b.Data, boneRotatation.head);
                        break;
                    case Joint.HIP_L:
                        SetJointRotation(charactersJoints.leftThigh, b.Data, boneRotatation.legUpperLeft);
                        break;
                    case Joint.HIP_R:
                        SetJointRotation(charactersJoints.rightThigh, b.Data, boneRotatation.legUpperRight);
                        break;
                    case Joint.KNEE_L:
                        SetJointRotation(charactersJoints.leftCalf, b.Data, boneRotatation.legLowerLeft);
                        break;
                    case Joint.KNEE_R:
                        SetJointRotation(charactersJoints.rightCalf, b.Data, boneRotatation.legLowerRight);
                        break;
                    case Joint.FOOTBASE_L:
                        SetJointRotation(charactersJoints.leftFoot, b.Data, boneRotatation.footLeft);
                        break;
                    case Joint.FOOTBASE_R:
                        SetJointRotation(charactersJoints.rightFoot, b.Data, boneRotatation.footRight);
                        break;
                    case Joint.CLAVICLE_L:
                        SetJointRotation(charactersJoints.leftClavicle, b.Data, boneRotatation.clavicleLeft);
                        break;
                    case Joint.CLAVICLE_R:
                        SetJointRotation(charactersJoints.rightClavicle, b.Data, boneRotatation.clavicleRight);
                        break;
                    case Joint.SHOULDER_L:
                        SetJointRotation(charactersJoints.leftUpperArm, b.Data, boneRotatation.armUpperLeft);
                        break;
                    case Joint.SHOULDER_R:
                        SetJointRotation(charactersJoints.rightUpperArm, b.Data, boneRotatation.armUpperRight);
                        break;
                    case Joint.ELBOW_L:
                        SetJointRotation(charactersJoints.leftForearm, b.Data, boneRotatation.armLowerLeft);
                        break;
                    case Joint.ELBOW_R:
                        SetJointRotation(charactersJoints.rightForearm, b.Data, boneRotatation.armLowerRight);
                        break;
                    case Joint.WRIST_L:
                        SetJointRotation(charactersJoints.leftHand, b.Data, boneRotatation.handLeft);
                        break;
                    case Joint.WRIST_R:
                        SetJointRotation(charactersJoints.rightHand, b.Data, boneRotatation.handRight);
                        break;
                    case Joint.HAND_L:
                        if (UseFingers && charactersJoints.fingersLeft != null)
                            foreach (var fing in charactersJoints.fingersLeft)
                                SetJointRotation(fing, b.Data, boneRotatation.fingersLeft);
                        break;
                    case Joint.HAND_R:
                        if (UseFingers && charactersJoints.fingersRight != null)
                            foreach (var fing in charactersJoints.fingersRight)
                                SetJointRotation(fing, b.Data, boneRotatation.fingersRight);
                        break;
                    case Joint.TRAP_L:
                        if (UseFingers)
                            SetJointRotation(charactersJoints.thumbLeft, b.Data, boneRotatation.thumbLeft);
                        break;
                    case Joint.TRAP_R:
                        if (UseFingers)
                            SetJointRotation(charactersJoints.thumbRight, b.Data, boneRotatation.thumbRight);
                        break;
                    case Joint.ANKLE_L:
                    case Joint.ANKLE_R:
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// Sets the rotation of the Transform from Bone object to 
        /// </summary>
        /// <param name="go">Joint Transform</param>
        /// <param name="b">Bone</param>
        /// <param name="euler">Euler angels</param>
        private void SetJointRotation(Transform go, Bone b, Vector3 euler)
        {
            if (go && !b.HasNaN)
            {
                go.rotation =
                    transform.rotation
                    * b.Orientation.Convert()
                    * Quaternion.Euler(euler)
                    * Quaternion.Euler(boneRotatation.root);
            }
        }
        /// <summary>
        /// Find the scale by which the characters positions should change
        /// </summary>
        /// <returns>A scaling factor to be applied to the poistional vector</returns>
        private float FindScale()
        {
            var calf = charactersJoints.leftCalf.position;
            var thigh = charactersJoints.leftThigh.position;
            var foot = charactersJoints.leftFoot.position;
            float pelvisHeight = (calf - thigh).magnitude + (thigh - foot).magnitude;
            var hip = skeleton[Joint.HIP_L].Pos;
            var knee = skeleton[Joint.KNEE_L].Pos;
            var footbase = skeleton[Joint.FOOTBASE_L].Pos;
            float actorPelvisHeight = (hip - knee).Length + (knee - footbase).Length;
            float s = pelvisHeight / actorPelvisHeight;
            s /= transform.localScale.magnitude;
            return s;
        }
        /// <summary>
        /// Checks whether the model has been changed since last and change the model
        /// </summary>
        public void SetModelRotation()
        {
            switch (model)
            {
                case CharactersModel.Model1:
                    boneRotatation = new Model1();
                    break;
                case CharactersModel.Model2:
                    boneRotatation = new Model2();
                    break;
                case CharactersModel.Model3:
                    boneRotatation = new Model3();
                    break;
                case CharactersModel.Model4:
                    boneRotatation = new Model4();
                    break;
                case CharactersModel.Model5:
                    boneRotatation = new Model5();
                    break;
                case CharactersModel.Model6:
                    boneRotatation = new Model6();
                    break;
                case CharactersModel.Model7:
                    boneRotatation = new Model7();
                    break;
                case CharactersModel.EmptyModel:
                    boneRotatation = new Empty();
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// If using head rotation from oculus instead of from markers
        /// </summary>
        /// <param name="b">The head bone as defiention what rotation is forward</param>
        void SetCameraPosition(Bone b)
        {
            if(!headCamera) GetCamera();
            if (headCamera)
            {
                var cameraAnchor = headCamera.transform.parent;
                cameraAnchor.position = 
                    charactersJoints.head.position 
                    + (headCamera.transform.rotation * headCam.CameraOffset);
                if (headCam.UseHeadCamera && !headCam.UseVRHeadSetRotation && headCamera)
                {
                    cameraAnchor = headCamera.transform.parent;
                    cameraAnchor.rotation = skeleton.Find(Joint.HEAD).Orientation.Convert();

                }
            }
        }
        /// <summary>
        /// Finds the camera and sets the reference
        /// </summary>
        public void GetCamera()
        {
            var searchRes = this.transform.Find("Camera");
            if (searchRes) headCamera = searchRes.GetComponent<Camera>();
            else
            {
                var cameraGO = new GameObject("Camera");
                cameraGO.transform.parent = transform;
                headCamera = cameraGO.AddComponent<Camera>();
            }
            if (headCamera)
            {
                headCamera.nearClipPlane = 0.03f; 
                var go = new GameObject("CameraAnchor");
                headCamera.transform.position = Vector3.zero;
                headCamera.transform.SetParent(go.transform);
                go.transform.SetParent(transform);
                Recenter();
            }
        }
        public void DestroyCamera()
        {
            if (headCamera.transform.parent.gameObject) Destroy(headCamera.transform.parent.gameObject);
        }
        public void Recenter()
        {
            if (skeleton != null)
            {
                var b = skeleton[Joint.HEAD];
                if (!b.HasNaN) headCamera.transform.parent.rotation = b.Orientation.Convert();
            }
            UnityEngine.VR.InputTracking.Recenter();
        }
    }
}



