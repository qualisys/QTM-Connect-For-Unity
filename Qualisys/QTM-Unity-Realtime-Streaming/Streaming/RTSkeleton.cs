// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QTMRealTimeSDK;

namespace QualisysRealTime.Unity
{
    public class RTSkeleton : MonoBehaviour
    {
        public string SkeletonName = "Put QTM skeleton name here";

        private Avatar mSourceAvatar;
        public Avatar DestinationAvatar;

        private HumanPose mHumanPose = new HumanPose();
        private GameObject mStreamedRootObject;
        private Dictionary<uint, GameObject> mQTmJointIdToGameObject;
        private Dictionary<string, string> mMecanimToQtmJointNames = new Dictionary<string, string>();

        private HumanPoseHandler mSourcePoseHandler;
        private HumanPoseHandler mDestiationPoseHandler;

        protected RTClient rtClient;
        private QssSkeleton mQtmSkeleton;

        void Start()
        {
            CreateQtmToMecanimJointNameMap(SkeletonName);
        }

        bool buildingSkeleton = false;

        void Update()
        {
            if (rtClient == null) rtClient = RTClient.GetInstance();

            if (mQtmSkeleton == null)
            {
                // setup mecanim and qtm joint data
                foreach (var skeleton in rtClient.Skeletons)
                {
                    if (skeleton.Name != SkeletonName)
                        continue;

                    this.mQtmSkeleton = skeleton;
                    break;
                }

                if (mQtmSkeleton == null)
                    return;

                if (buildingSkeleton)
                    return;

                buildingSkeleton = true;

                mStreamedRootObject = new GameObject(this.SkeletonName);
                mQTmJointIdToGameObject = new Dictionary<uint, GameObject>(mQtmSkeleton.QssJoints.Count);
                foreach (var joint in mQtmSkeleton.QssJoints.ToList())
                {
                    var jointGameObject = new GameObject(this.SkeletonName + "_" + joint.Value.Name);
                    jointGameObject.transform.parent = joint.Value.ParentId == 0 ? mStreamedRootObject.transform : mQTmJointIdToGameObject[joint.Value.ParentId].transform;
                    jointGameObject.transform.localPosition = joint.Value.TPosition;
                    mQTmJointIdToGameObject[joint.Value.Id] = jointGameObject;
                }

                BuildMecanimAvatarFromQtmTPose();

                buildingSkeleton = false;
                return;
            }

            if (mQtmSkeleton == null)
                return;

            // Update all the joint transforms
            foreach (var joint in mQtmSkeleton.QssJoints.ToList())
            {
                GameObject jointGameObject;
                if (mQTmJointIdToGameObject.TryGetValue(joint.Key, out jointGameObject))
                {
                    jointGameObject.transform.localPosition = joint.Value.Position;
                    jointGameObject.transform.localRotation = joint.Value.Rotation;
                }
            }
            if (mSourcePoseHandler != null && mDestiationPoseHandler != null)
            {
                mSourcePoseHandler.GetHumanPose(ref mHumanPose);
                mDestiationPoseHandler.SetHumanPose(ref mHumanPose);
            }
        }

        private void BuildMecanimAvatarFromQtmTPose()
        {
            var humanBones = new List<HumanBone>(mQtmSkeleton.QssJoints.Count);
            for (int index = 0; index < HumanTrait.BoneName.Length; index++)
            {
                var humanBoneName = HumanTrait.BoneName[index];
                if (mMecanimToQtmJointNames.ContainsKey(humanBoneName))
                {
                    var bone = new HumanBone()
                    {
                        humanName = humanBoneName,
                        boneName = mMecanimToQtmJointNames[humanBoneName],
                    };
                    bone.limit.useDefaultValues = true;
                    humanBones.Add(bone);
                }
            }

            // Set up the T-pose and game object name mappings.
            var skeletonBones = new List<SkeletonBone>(mQtmSkeleton.QssJoints.Count + 1);
            skeletonBones.Add(new SkeletonBone()
            {
                name = this.SkeletonName,
                position = Vector3.zero,
                rotation = Quaternion.identity,
                scale = Vector3.one,
            });

            // Create remaining T-Pose bone definitions from Qtm joints
            foreach (var qssJoint in mQtmSkeleton.QssJoints.ToList())
            {
                skeletonBones.Add(new SkeletonBone()
                {
                    name = this.SkeletonName + "_" + qssJoint.Value.Name,
                    position = qssJoint.Value.TPosition,
                    rotation = Quaternion.identity,
                    scale = Vector3.one,
                });
            }

            mSourceAvatar = AvatarBuilder.BuildHumanAvatar(mStreamedRootObject,
                new HumanDescription()
                {
                    human = humanBones.ToArray(),
                    skeleton = skeletonBones.ToArray(),
                }
            );
            if (mSourceAvatar.isValid == false || mSourceAvatar.isHuman == false)
            {
                this.enabled = false;
                return;
            }

            mSourcePoseHandler = new HumanPoseHandler(mSourceAvatar, mStreamedRootObject.transform);
            mDestiationPoseHandler = new HumanPoseHandler(DestinationAvatar, this.transform);

            mStreamedRootObject.transform.parent = this.gameObject.transform;
        }

        private void CreateQtmToMecanimJointNameMap(string skeletonName)
        {
            mMecanimToQtmJointNames.Add("RightShoulder", skeletonName + "_RightShoulder");
            mMecanimToQtmJointNames.Add("RightUpperArm", skeletonName + "_RightArm");
            mMecanimToQtmJointNames.Add("RightLowerArm", skeletonName + "_RightForeArm");
            mMecanimToQtmJointNames.Add("RightHand", skeletonName + "_RightHand");
            mMecanimToQtmJointNames.Add("LeftShoulder", skeletonName + "_LeftShoulder");
            mMecanimToQtmJointNames.Add("LeftUpperArm", skeletonName + "_LeftArm");
            mMecanimToQtmJointNames.Add("LeftLowerArm", skeletonName + "_LeftForeArm");
            mMecanimToQtmJointNames.Add("LeftHand", skeletonName + "_LeftHand");

            mMecanimToQtmJointNames.Add("RightUpperLeg", skeletonName + "_RightUpLeg");
            mMecanimToQtmJointNames.Add("RightLowerLeg", skeletonName + "_RightLeg");
            mMecanimToQtmJointNames.Add("RightFoot", skeletonName + "_RightFoot");
            mMecanimToQtmJointNames.Add("RightToeBase", skeletonName + "_RightToeBase");
            mMecanimToQtmJointNames.Add("LeftUpperLeg", skeletonName + "_LeftUpLeg");
            mMecanimToQtmJointNames.Add("LeftLowerLeg", skeletonName + "_LeftLeg");
            mMecanimToQtmJointNames.Add("LeftFoot", skeletonName + "_LeftFoot");
            mMecanimToQtmJointNames.Add("LeftToeBase", skeletonName + "_LeftToeBase");

            mMecanimToQtmJointNames.Add("Hips", skeletonName + "_Hips");
            mMecanimToQtmJointNames.Add("Spine", skeletonName + "_Spine");
            mMecanimToQtmJointNames.Add("Chest", skeletonName + "_Spine1");
            mMecanimToQtmJointNames.Add("Neck", skeletonName + "_Neck");
            mMecanimToQtmJointNames.Add("Head", skeletonName + "_Head");
            /*
            mMecanimToQtmJointNames.Add("Left Thumb Proximal", skeletonName + "_LeftHandThumb1");
            mMecanimToQtmJointNames.Add("Left Thumb Intermediate", skeletonName + "_LeftHandThumb2");
            mMecanimToQtmJointNames.Add("Left Thumb Distal", skeletonName + "_LeftHandThumb3");
            mMecanimToQtmJointNames.Add("Left Index Proximal", skeletonName + "_LeftHandIndex1");
            mMecanimToQtmJointNames.Add("Left Index Intermediate", skeletonName + "_LeftHandIndex2");
            mMecanimToQtmJointNames.Add("Left Index Distal", skeletonName + "_LeftHandIndex3");
            mMecanimToQtmJointNames.Add("Left Middle Proximal", skeletonName + "_LeftHandMiddle1");
            mMecanimToQtmJointNames.Add("Left Middle Intermediate", skeletonName + "_LeftHandMiddle2");
            mMecanimToQtmJointNames.Add("Left Middle Distal", skeletonName + "_LeftHandMiddle3");
            mMecanimToQtmJointNames.Add("Left Ring Proximal", skeletonName + "_LeftHandRing1");
            mMecanimToQtmJointNames.Add("Left Ring Intermediate", skeletonName + "_LeftHandRing2");
            mMecanimToQtmJointNames.Add("Left Ring Distal", skeletonName + "_LeftHandRing3");
            mMecanimToQtmJointNames.Add("Left Little Proximal", skeletonName + "_LeftHandPinky1");
            mMecanimToQtmJointNames.Add("Left Little Intermediate", skeletonName + "_LeftHandPinky2");
            mMecanimToQtmJointNames.Add("Left Little Distal", skeletonName + "_LeftHandPinky3");

            mMecanimToQtmJointNames.Add("Right Thumb Proximal", skeletonName + "_RightHandThumb1");
            mMecanimToQtmJointNames.Add("Right Thumb Intermediate", skeletonName + "_RightHandThumb2");
            mMecanimToQtmJointNames.Add("Right Thumb Distal", skeletonName + "_RightHandThumb3");
            mMecanimToQtmJointNames.Add("Right Index Proximal", skeletonName + "_RightHandIndex1");
            mMecanimToQtmJointNames.Add("Right Index Intermediate", skeletonName + "_RightHandIndex2");
            mMecanimToQtmJointNames.Add("Right Index Distal", skeletonName + "_RightHandIndex3");
            mMecanimToQtmJointNames.Add("Right Middle Proximal", skeletonName + "_RightHandMiddle1");
            mMecanimToQtmJointNames.Add("Right Middle Intermediate", skeletonName + "_RightHandMiddle2");
            mMecanimToQtmJointNames.Add("Right Middle Distal", skeletonName + "_RightHandMiddle3");
            mMecanimToQtmJointNames.Add("Right Ring Proximal", skeletonName + "_RightHandRing1");
            mMecanimToQtmJointNames.Add("Right Ring Intermediate", skeletonName + "_RightHandRing2");
            mMecanimToQtmJointNames.Add("Right Ring Distal", skeletonName + "_RightHandRing3");
            mMecanimToQtmJointNames.Add("Right Little Proximal", skeletonName + "_RightHandPinky1");
            mMecanimToQtmJointNames.Add("Right Little Intermediate", skeletonName + "_RightHandPinky2");
            mMecanimToQtmJointNames.Add("Right Little Distal", skeletonName + "_RightHandPinky3");
            */
        }
    }
}