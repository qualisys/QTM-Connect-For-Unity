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
        public SegmentRotations segmentRotation;
        public HeadCam headCam;

        internal Camera headCamera = new Camera();
        internal bool segmentsFound = true;

        protected RTClient rtClient;
        protected Vector3 pos;
        protected List<Marker> markerData;
        protected BipedSkeleton skeleton;

        private SkeletonBuilder skeletonBuilder;
        private CharacterGameObjects characterTransforms = new CharacterGameObjects();
        public CharacterGameObjects Transforms
        {
            get { return characterTransforms; }
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
            segmentsFound = characterTransforms.SetLimbs(this.transform, UseFingers);
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

            characterTransforms.SetLimbs(this.transform, UseFingers);
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
                    case SegmentName.PELVIS:
                        SetTransformRotation(characterTransforms.pelvis, b.Data, segmentRotation.hip);
                        if (characterTransforms.pelvis && !b.Data.Pos.IsNaN())
                        {
                            if (defaultPelvisPosition == Vector3.zero)
                                defaultPelvisPosition = characterTransforms.pelvis.position;

                            characterTransforms.pelvis.position = transform.position + footOffset + transform.rotation *
                                ((ScaleMovementToSize && scale > 0) ?
                                (b.Data.Pos * scale * transform.localScale.magnitude).Convert()
                                : b.Data.Pos.Convert());
                        }
                        break;
                    case SegmentName.SPINE0:
                        if (characterTransforms.spine.Length > 0)
                            SetTransformRotation(characterTransforms.spine[0], b.Data, segmentRotation.spine);
                        break;
                    case SegmentName.SPINE1:
                        if (characterTransforms.spine.Length > 1)
                            SetTransformRotation(characterTransforms.spine[1], b.Data, segmentRotation.spine);
                        break;
                    case SegmentName.SPINE3:
                        if (characterTransforms.spine.Length > 2)
                            SetTransformRotation(characterTransforms.spine[2], b.Data, segmentRotation.spine);
                        break;
                    case SegmentName.NECK:
                        SetTransformRotation(characterTransforms.neck, b.Data, segmentRotation.neck);
                        break;
                    case SegmentName.HEAD:
                        if (headCam.UseHeadCamera)
                            SetCameraPosition(b.Data);
                        if (headCam.UseHeadCamera && !headCam.UseVRHeadSetRotation && headCamera)
                        {
                            SetTransformRotation(characterTransforms.head, b.Data, segmentRotation.head);
                        }
                        else if (headCamera)
                        {
                            characterTransforms.head.rotation =
                                headCamera.transform.rotation * Quaternion.Euler(segmentRotation.headCamera);
                        }
                        else SetTransformRotation(characterTransforms.head, b.Data, segmentRotation.head);
                        break;
                    case SegmentName.HIP_L:
                        SetTransformRotation(characterTransforms.leftThigh, b.Data, segmentRotation.legUpperLeft);
                        break;
                    case SegmentName.HIP_R:
                        SetTransformRotation(characterTransforms.rightThigh, b.Data, segmentRotation.legUpperRight);
                        break;
                    case SegmentName.KNEE_L:
                        SetTransformRotation(characterTransforms.leftCalf, b.Data, segmentRotation.legLowerLeft);
                        break;
                    case SegmentName.KNEE_R:
                        SetTransformRotation(characterTransforms.rightCalf, b.Data, segmentRotation.legLowerRight);
                        break;
                    case SegmentName.FOOTBASE_L:
                        SetTransformRotation(characterTransforms.leftFoot, b.Data, segmentRotation.footLeft);
                        break;
                    case SegmentName.FOOTBASE_R:
                        SetTransformRotation(characterTransforms.rightFoot, b.Data, segmentRotation.footRight);
                        break;
                    case SegmentName.CLAVICLE_L:
                        SetTransformRotation(characterTransforms.leftClavicle, b.Data, segmentRotation.clavicleLeft);
                        break;
                    case SegmentName.CLAVICLE_R:
                        SetTransformRotation(characterTransforms.rightClavicle, b.Data, segmentRotation.clavicleRight);
                        break;
                    case SegmentName.SHOULDER_L:
                        SetTransformRotation(characterTransforms.leftUpperArm, b.Data, segmentRotation.armUpperLeft);
                        break;
                    case SegmentName.SHOULDER_R:
                        SetTransformRotation(characterTransforms.rightUpperArm, b.Data, segmentRotation.armUpperRight);
                        break;
                    case SegmentName.ELBOW_L:
                        SetTransformRotation(characterTransforms.leftForearm, b.Data, segmentRotation.armLowerLeft);
                        break;
                    case SegmentName.ELBOW_R:
                        SetTransformRotation(characterTransforms.rightForearm, b.Data, segmentRotation.armLowerRight);
                        break;
                    case SegmentName.WRIST_L:
                        SetTransformRotation(characterTransforms.leftHand, b.Data, segmentRotation.handLeft);
                        break;
                    case SegmentName.WRIST_R:
                        SetTransformRotation(characterTransforms.rightHand, b.Data, segmentRotation.handRight);
                        break;
                    case SegmentName.HAND_L:
                        if (UseFingers && characterTransforms.fingersLeft != null)
                            foreach (var fing in characterTransforms.fingersLeft)
                                SetTransformRotation(fing, b.Data, segmentRotation.fingersLeft);
                        break;
                    case SegmentName.HAND_R:
                        if (UseFingers && characterTransforms.fingersRight != null)
                            foreach (var fing in characterTransforms.fingersRight)
                                SetTransformRotation(fing, b.Data, segmentRotation.fingersRight);
                        break;
                    case SegmentName.TRAP_L:
                        if (UseFingers)
                            SetTransformRotation(characterTransforms.thumbLeft, b.Data, segmentRotation.thumbLeft);
                        break;
                    case SegmentName.TRAP_R:
                        if (UseFingers)
                            SetTransformRotation(characterTransforms.thumbRight, b.Data, segmentRotation.thumbRight);
                        break;
                    case SegmentName.ANKLE_L:
                    case SegmentName.ANKLE_R:
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Sets the rotation of the Transform from Segment object to 
        /// </summary>
        /// <param name="transform">Transform</param>
        /// <param name="b">Segment</param>
        /// <param name="euler">Euler angels</param>
        private void SetTransformRotation(Transform transform, Segment b, Vector3 euler)
        {
            if (transform && !b.HasNaN)
            {
                transform.rotation =
                    base.transform.rotation
                    * b.Orientation.Convert()
                    * Quaternion.Euler(euler)
                    * Quaternion.Euler(segmentRotation.root);
            }
        }

        /// <summary>
        /// Find the scale by which the characters positions should change
        /// </summary>
        /// <returns>A scaling factor to be applied to the positional vector</returns>
        private float FindScale()
        {
            var calf = characterTransforms.leftCalf.position;
            var thigh = characterTransforms.leftThigh.position;
            var foot = characterTransforms.leftFoot.position;
            float pelvisHeight = (calf - thigh).magnitude + (thigh - foot).magnitude;
            var hip = skeleton[SegmentName.HIP_L].Pos;
            var knee = skeleton[SegmentName.KNEE_L].Pos;
            var footbase = skeleton[SegmentName.FOOTBASE_L].Pos;
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
            characterTransforms.pelvis.position -= footOffset;
            footOffset = (defaultPelvisPosition - characterTransforms.pelvis.position).y * Vector3.up;

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
                    segmentRotation = new Model1();
                    break;
                case CharacterModels.Model2:
                    segmentRotation = new Model2();
                    break;
                case CharacterModels.Model3:
                    segmentRotation = new Model3();
                    break;
                case CharacterModels.Model4:
                    segmentRotation = new Model4();
                    break;
                case CharacterModels.Model5:
                    segmentRotation = new Model5();
                    break;
                case CharacterModels.Model6:
                    segmentRotation = new Model6();
                    break;
                case CharacterModels.Model7:
                    segmentRotation = new Model7();
                    break;
                case CharacterModels.Model8:
                    segmentRotation = new Model8();
                    break;
                case CharacterModels.Model9:
                    segmentRotation = new Model9();
                    break;
                default:
                case CharacterModels.EmptyModel:
                    segmentRotation = new Empty();
                    break;
            }
        }

        /// <summary>
        /// If using head rotation from oculus instead of from markers
        /// </summary>
        /// <param name="b">The head segment as defiention what rotation is forward</param>
        void SetCameraPosition(Segment b)
        {
            if (!headCamera) GetCamera();
            if (headCamera)
            {
                var cameraAnchor = headCamera.transform.parent;
                cameraAnchor.position =
                    characterTransforms.head.position
                    + (headCamera.transform.rotation * headCam.CameraOffset);
                if (headCam.UseHeadCamera && !headCam.UseVRHeadSetRotation && headCamera)
                {
                    cameraAnchor = headCamera.transform.parent;
                    cameraAnchor.rotation = skeleton.Find(SegmentName.HEAD).Orientation.Convert();
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
                var b = skeleton[SegmentName.HEAD];
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



