// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649
namespace QualisysRealTime.Unity.Skeleton
{
    class RTCharacterStream : MonoBehaviour
    {
        public string ActorMarkerPrefix = "";

        // Actors body sizes
        public int actorHeight = 0; // cm
        public int actorMass = 0; // kg

        public bool ScaleMovementToSize = false;
        public bool UseIK = true;
        public bool UseTrackingMarkers = true;
        public bool UseFingers = false;
        public CharacterModels model = CharacterModels.Model1;
        public BoneRotations boneRotation;
        public HeadCam headCam;

        internal Camera headCamera = new Camera();
        internal bool jointsFound = true;

        protected RTClient rtClient;
        protected Vector3 pos;
        protected List<Marker> markerData;
        protected BipedSkeleton skeleton;

        private SkeletonBuilder skeletonBuilder;
        private CharacterGameObjects charactersJoints = new CharacterGameObjects();
        public CharacterGameObjects Joints
        {
            get { return charactersJoints; }
        }
        private float scale = 0;

        private Vector3 defaultPelvisPosition = Vector3.zero;
        private Vector3 footOffset = Vector3.zero;

        /// <summary>
        /// Locating the joints of the character and find the scale by which the position should be applied to
        /// </summary>
        public void Start()
        {
            rtClient = RTClient.GetInstance();

            //Find all joints of the characters
            jointsFound = charactersJoints.SetLimbs(this.transform, UseFingers);
            //charactersJoints.PrintAll();

            // disable the animation
            var animation = this.GetComponent<Animation>();
            if (animation) animation.enabled = false;

            // E.Wolf, 2015-12-15
            this.SetModelRotation();
            //
        }
        /// <summary>
        /// Updates the rotation and position of the Character
        /// </summary>
        public void Update()
        {
            if (rtClient == null)
                rtClient = RTClient.GetInstance();
            if (!rtClient.GetStreamingStatus())
                return;
            markerData = rtClient.Markers.Convert();
            if ((markerData == null || markerData.Count == 0))
            {
                Debug.LogError("The stream does not contain any markers");
                return;
            }
            if (skeletonBuilder != null)
                skeleton = skeletonBuilder.SolveSkeleton(markerData);
            else
                ResetSkeleton();
            SetAll();
            if (!headCam.UseHeadCamera && headCamera)
                DestroyCamera();
        }

        /// <summary>
        /// ...
        /// </summary>
        public void ResetSkeleton()
        {
            if (DEBUG.enabled) Debug.Log("Reset Skeleton...");

            charactersJoints.SetLimbs(this.transform, UseFingers);
            //charactersJoints.PrintAll();

            skeletonBuilder = new SkeletonBuilder();
            skeletonBuilder.MarkerPrefix = ActorMarkerPrefix;
            skeletonBuilder.SetBodyData(actorHeight, actorMass);
            skeletonBuilder.SolveWithIK = UseIK;
            skeletonBuilder.UseTrackingMarkers = UseTrackingMarkers;

            if (markerData != null)
                skeleton = skeletonBuilder.SolveSkeleton(markerData);
            else
                skeleton = new BipedSkeleton();

            if (ScaleMovementToSize)
                scale = FindScale();
            else
                scale = 0;
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
                        SetJointRotation(charactersJoints.pelvis, b.Data, boneRotation.hip);
                        if (charactersJoints.pelvis && !b.Data.Pos.IsNaN())
                        {
                            if (defaultPelvisPosition == Vector3.zero)
                                defaultPelvisPosition = charactersJoints.pelvis.position;

                            charactersJoints.pelvis.position = transform.position + footOffset + transform.rotation *
                                ((ScaleMovementToSize && scale > 0) ?
                                (b.Data.Pos * scale * transform.localScale.magnitude).Convert()
                                : b.Data.Pos.Convert());
                        }
                        break;
                    case Joint.SPINE0:
                        if (charactersJoints.spine.Length > 0)
                            SetJointRotation(charactersJoints.spine[0], b.Data, boneRotation.spine);
                        break;
                    case Joint.SPINE1:
                        if (charactersJoints.spine.Length > 1)
                            SetJointRotation(charactersJoints.spine[1], b.Data, boneRotation.spine);
                        break;
                    case Joint.SPINE3:
                        if (charactersJoints.spine.Length > 2)
                            SetJointRotation(charactersJoints.spine[2], b.Data, boneRotation.spine);
                        break;
                    case Joint.NECK:
                        SetJointRotation(charactersJoints.neck, b.Data, boneRotation.neck);
                        break;
                    case Joint.HEAD:
                        if (headCam.UseHeadCamera)
                            SetCameraPosition(b.Data);
                        if (headCam.UseHeadCamera && !headCam.UseVRHeadSetRotation && headCamera)
                        {
                            SetJointRotation(charactersJoints.head, b.Data, boneRotation.head);
                        }
                        else if (headCamera)
                        {
                            charactersJoints.head.rotation =
                                headCamera.transform.rotation * Quaternion.Euler(boneRotation.headCamera);
                        }
                        else SetJointRotation(charactersJoints.head, b.Data, boneRotation.head);
                        break;
                    case Joint.HIP_L:
                        SetJointRotation(charactersJoints.leftThigh, b.Data, boneRotation.legUpperLeft);
                        break;
                    case Joint.HIP_R:
                        SetJointRotation(charactersJoints.rightThigh, b.Data, boneRotation.legUpperRight);
                        break;
                    case Joint.KNEE_L:
                        SetJointRotation(charactersJoints.leftCalf, b.Data, boneRotation.legLowerLeft);
                        break;
                    case Joint.KNEE_R:
                        SetJointRotation(charactersJoints.rightCalf, b.Data, boneRotation.legLowerRight);
                        break;
                    case Joint.FOOTBASE_L:
                        SetJointRotation(charactersJoints.leftFoot, b.Data, boneRotation.footLeft);
                        break;
                    case Joint.FOOTBASE_R:
                        SetJointRotation(charactersJoints.rightFoot, b.Data, boneRotation.footRight);
                        break;
                    case Joint.CLAVICLE_L:
                        SetJointRotation(charactersJoints.leftClavicle, b.Data, boneRotation.clavicleLeft);
                        break;
                    case Joint.CLAVICLE_R:
                        SetJointRotation(charactersJoints.rightClavicle, b.Data, boneRotation.clavicleRight);
                        break;
                    case Joint.SHOULDER_L:
                        SetJointRotation(charactersJoints.leftUpperArm, b.Data, boneRotation.armUpperLeft);
                        break;
                    case Joint.SHOULDER_R:
                        SetJointRotation(charactersJoints.rightUpperArm, b.Data, boneRotation.armUpperRight);
                        break;
                    case Joint.ELBOW_L:
                        SetJointRotation(charactersJoints.leftForearm, b.Data, boneRotation.armLowerLeft);
                        break;
                    case Joint.ELBOW_R:
                        SetJointRotation(charactersJoints.rightForearm, b.Data, boneRotation.armLowerRight);
                        break;
                    case Joint.WRIST_L:
                        SetJointRotation(charactersJoints.leftHand, b.Data, boneRotation.handLeft);
                        break;
                    case Joint.WRIST_R:
                        SetJointRotation(charactersJoints.rightHand, b.Data, boneRotation.handRight);
                        break;
                    case Joint.HAND_L:
                        if (UseFingers && charactersJoints.fingersLeft != null)
                            foreach (var fing in charactersJoints.fingersLeft)
                                SetJointRotation(fing, b.Data, boneRotation.fingersLeft);
                        break;
                    case Joint.HAND_R:
                        if (UseFingers && charactersJoints.fingersRight != null)
                            foreach (var fing in charactersJoints.fingersRight)
                                SetJointRotation(fing, b.Data, boneRotation.fingersRight);
                        break;
                    case Joint.TRAP_L:
                        if (UseFingers)
                            SetJointRotation(charactersJoints.thumbLeft, b.Data, boneRotation.thumbLeft);
                        break;
                    case Joint.TRAP_R:
                        if (UseFingers)
                            SetJointRotation(charactersJoints.thumbRight, b.Data, boneRotation.thumbRight);
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
                    * Quaternion.Euler(boneRotation.root);
            }
        }

        /// <summary>
        /// Find the scale by which the characters positions should change
        /// </summary>
        /// <returns>A scaling factor to be applied to the positional vector</returns>
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
        /// Calibrate size
        /// </summary>
        public void Calibrate()
        {
            if (DEBUG.enabled) Debug.Log("Calibrate character...");

            // Calculate offset between characters hip height and actors hip height
            charactersJoints.pelvis.position -= footOffset;
            footOffset = (defaultPelvisPosition - charactersJoints.pelvis.position).y * Vector3.up;

            if (DEBUG.enabled) Debug.Log("Set Foot Offset to " + footOffset);

            // Calculate Body Data
            if (skeletonBuilder != null)
                skeletonBuilder.SetBodyData(actorHeight, actorMass);

            if (DEBUG.enabled) Debug.Log("Set Body Height to " + actorHeight + "cm and Mass to " + actorMass + "kg");
        }

        /// <summary>
        /// Set model rotation to use
        /// </summary>
        public void SetModelRotation()
        {
            switch (model)
            {
                case CharacterModels.Model1:
                    boneRotation = new Model1();
                    break;
                case CharacterModels.Model2:
                    boneRotation = new Model2();
                    break;
                case CharacterModels.Model3:
                    boneRotation = new Model3();
                    break;
                case CharacterModels.Model4:
                    boneRotation = new Model4();
                    break;
                case CharacterModels.Model5:
                    boneRotation = new Model5();
                    break;
                case CharacterModels.Model6:
                    boneRotation = new Model6();
                    break;
                case CharacterModels.Model7:
                    boneRotation = new Model7();
                    break;
                case CharacterModels.Model8:
                    boneRotation = new Model8();
                    break;
                case CharacterModels.Model9:
                    boneRotation = new Model9();
                    break;
                default:
                case CharacterModels.EmptyModel:
                    boneRotation = new Empty();
                    break;
            }
        }

        /// <summary>
        /// If using head rotation from oculus instead of from markers
        /// </summary>
        /// <param name="b">The head bone as defiention what rotation is forward</param>
        void SetCameraPosition(Bone b)
        {
            if (!headCamera) GetCamera();
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
            var searchRes = this.transform.Find("FPS_Camera");
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
#if UNITY_2017_2_OR_NEWER
            UnityEngine.XR.InputTracking.Recenter();
#else
            UnityEngine.VR.InputTracking.Recenter();
#endif

        }
    }
}



