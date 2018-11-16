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
                    case SegmentName.PELVIS:
                        SetJointRotation(charactersJoints.pelvis, b.Data, segmentRotation.hip);
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
                    case SegmentName.SPINE0:
                        if (charactersJoints.spine.Length > 0)
                            SetJointRotation(charactersJoints.spine[0], b.Data, segmentRotation.spine);
                        break;
                    case SegmentName.SPINE1:
                        if (charactersJoints.spine.Length > 1)
                            SetJointRotation(charactersJoints.spine[1], b.Data, segmentRotation.spine);
                        break;
                    case SegmentName.SPINE3:
                        if (charactersJoints.spine.Length > 2)
                            SetJointRotation(charactersJoints.spine[2], b.Data, segmentRotation.spine);
                        break;
                    case SegmentName.NECK:
                        SetJointRotation(charactersJoints.neck, b.Data, segmentRotation.neck);
                        break;
                    case SegmentName.HEAD:
                        if (headCam.UseHeadCamera)
                            SetCameraPosition(b.Data);
                        if (headCam.UseHeadCamera && !headCam.UseVRHeadSetRotation && headCamera)
                        {
                            SetJointRotation(charactersJoints.head, b.Data, segmentRotation.head);
                        }
                        else if (headCamera)
                        {
                            charactersJoints.head.rotation =
                                headCamera.transform.rotation * Quaternion.Euler(segmentRotation.headCamera);
                        }
                        else SetJointRotation(charactersJoints.head, b.Data, segmentRotation.head);
                        break;
                    case SegmentName.HIP_L:
                        SetJointRotation(charactersJoints.leftThigh, b.Data, segmentRotation.legUpperLeft);
                        break;
                    case SegmentName.HIP_R:
                        SetJointRotation(charactersJoints.rightThigh, b.Data, segmentRotation.legUpperRight);
                        break;
                    case SegmentName.KNEE_L:
                        SetJointRotation(charactersJoints.leftCalf, b.Data, segmentRotation.legLowerLeft);
                        break;
                    case SegmentName.KNEE_R:
                        SetJointRotation(charactersJoints.rightCalf, b.Data, segmentRotation.legLowerRight);
                        break;
                    case SegmentName.FOOTBASE_L:
                        SetJointRotation(charactersJoints.leftFoot, b.Data, segmentRotation.footLeft);
                        break;
                    case SegmentName.FOOTBASE_R:
                        SetJointRotation(charactersJoints.rightFoot, b.Data, segmentRotation.footRight);
                        break;
                    case SegmentName.CLAVICLE_L:
                        SetJointRotation(charactersJoints.leftClavicle, b.Data, segmentRotation.clavicleLeft);
                        break;
                    case SegmentName.CLAVICLE_R:
                        SetJointRotation(charactersJoints.rightClavicle, b.Data, segmentRotation.clavicleRight);
                        break;
                    case SegmentName.SHOULDER_L:
                        SetJointRotation(charactersJoints.leftUpperArm, b.Data, segmentRotation.armUpperLeft);
                        break;
                    case SegmentName.SHOULDER_R:
                        SetJointRotation(charactersJoints.rightUpperArm, b.Data, segmentRotation.armUpperRight);
                        break;
                    case SegmentName.ELBOW_L:
                        SetJointRotation(charactersJoints.leftForearm, b.Data, segmentRotation.armLowerLeft);
                        break;
                    case SegmentName.ELBOW_R:
                        SetJointRotation(charactersJoints.rightForearm, b.Data, segmentRotation.armLowerRight);
                        break;
                    case SegmentName.WRIST_L:
                        SetJointRotation(charactersJoints.leftHand, b.Data, segmentRotation.handLeft);
                        break;
                    case SegmentName.WRIST_R:
                        SetJointRotation(charactersJoints.rightHand, b.Data, segmentRotation.handRight);
                        break;
                    case SegmentName.HAND_L:
                        if (UseFingers && charactersJoints.fingersLeft != null)
                            foreach (var fing in charactersJoints.fingersLeft)
                                SetJointRotation(fing, b.Data, segmentRotation.fingersLeft);
                        break;
                    case SegmentName.HAND_R:
                        if (UseFingers && charactersJoints.fingersRight != null)
                            foreach (var fing in charactersJoints.fingersRight)
                                SetJointRotation(fing, b.Data, segmentRotation.fingersRight);
                        break;
                    case SegmentName.TRAP_L:
                        if (UseFingers)
                            SetJointRotation(charactersJoints.thumbLeft, b.Data, segmentRotation.thumbLeft);
                        break;
                    case SegmentName.TRAP_R:
                        if (UseFingers)
                            SetJointRotation(charactersJoints.thumbRight, b.Data, segmentRotation.thumbRight);
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
        /// <param name="go">Segment Transform</param>
        /// <param name="b">Segment</param>
        /// <param name="euler">Euler angels</param>
        private void SetJointRotation(Transform go, Segment b, Vector3 euler)
        {
            if (go && !b.HasNaN)
            {
                go.rotation =
                    transform.rotation
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
            var calf = charactersJoints.leftCalf.position;
            var thigh = charactersJoints.leftThigh.position;
            var foot = charactersJoints.leftFoot.position;
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
                    charactersJoints.head.position
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



