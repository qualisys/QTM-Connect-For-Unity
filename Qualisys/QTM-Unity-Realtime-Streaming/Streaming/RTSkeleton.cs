// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;
using System.Collections;
using QTMRealTimeSDK;
using System.Collections.Generic;
using System.Linq;

namespace QualisysRealTime.Unity
{
    public class RTSkeleton : MonoBehaviour
    {
        public string SkeletonName = "Put QTM skeleton name here";
        public Avatar DestinationAvatar;
        private Avatar mSourceAvatar;

        private HumanPose mHumanPose = new HumanPose();
        private QssSkeleton mSkeleton;
        private GameObject mStreamedRootObject;
        private Dictionary<uint, GameObject> mJointIdToGameObject;
        private Dictionary<string, string> mMecanimToQtmBoneNames = new Dictionary<string, string>();

        private HumanPoseHandler mSourcePoseHandler;
        private HumanPoseHandler mDestiationPoseHandler;

        protected RTClient rtClient;


        void Start()
        {
            CreateQtmToMecanimJointNameMap(SkeletonName);
        }

        bool buildingSkeleton = false;

        void Update()
        {
            if (rtClient == null) rtClient = RTClient.GetInstance();

            if (mSkeleton == null)
            {
                // Join mecanim and qtm skeleton information
                foreach (var skeleton in rtClient.Skeletons)
                {
                    if (skeleton.Name != SkeletonName)
                        continue;

                    this.mSkeleton = skeleton;
                    break;
                }

                if (mSkeleton == null)
                    return;

                if (buildingSkeleton)
                    return;

                buildingSkeleton = true;

                mStreamedRootObject = new GameObject("QTM : " + this.SkeletonName);

                mJointIdToGameObject = new Dictionary<uint, GameObject>(mSkeleton.QssJoints.Count);

                foreach (var qssJoint in mSkeleton.QssJoints.ToList())
                {
                    var boneObject = new GameObject(this.SkeletonName + "_" + qssJoint.Value.Name);
                    boneObject.transform.parent = qssJoint.Value.ParentId == 0 ? mStreamedRootObject.transform : mJointIdToGameObject[qssJoint.Value.ParentId].transform;
                    boneObject.transform.localPosition = qssJoint.Value.TPosition;
                    mJointIdToGameObject[qssJoint.Value.Id] = boneObject;
                }

                // Hook up retargeting between those GameObjects and the destination Avatar.
                SetupMecanimTPose();

                // Can't reparent this until after Mecanim setup, or else Mecanim gets confused.
                mStreamedRootObject.transform.parent = this.gameObject.transform;
                mStreamedRootObject.transform.localPosition = Vector3.zero;
                mStreamedRootObject.transform.localRotation = Quaternion.identity;

                buildingSkeleton = false;
                return;
            }

            if (mSkeleton == null)
                return;

            // Update all the joint transforms and then retarget
            foreach (var qssJoint in mSkeleton.QssJoints.ToList())
            {
                GameObject jointGameObject;
                bool foundObject = mJointIdToGameObject.TryGetValue(qssJoint.Key, out jointGameObject);
                if (foundObject)
                {
                    jointGameObject.transform.localPosition = qssJoint.Value.Position;
                    jointGameObject.transform.localRotation = qssJoint.Value.Rotation;
                }
            }
            if (mSourcePoseHandler != null && mDestiationPoseHandler != null)
            {
                mSourcePoseHandler.GetHumanPose(ref mHumanPose);
                mDestiationPoseHandler.SetHumanPose(ref mHumanPose);
            }
        }

        private void SetupMecanimTPose()
        {
            var humanBones = new List<HumanBone>(mSkeleton.QssJoints.Count);
            for (int index = 0; index < HumanTrait.BoneName.Length; index++)
            {
                var humanBoneName = HumanTrait.BoneName[index];
                if (mMecanimToQtmBoneNames.ContainsKey(humanBoneName))
                {
                    var bone = new HumanBone()
                    {
                        humanName = humanBoneName,
                        boneName = mMecanimToQtmBoneNames[humanBoneName],
                    };
                    bone.limit.useDefaultValues = true;
                    humanBones.Add(bone);
                }
            }

            // Set up the T-pose and game object name mappings.
            var skeletonBones = new List<SkeletonBone>(mSkeleton.QssJoints.Count + 1);

            // Set Root TPose to zero
            skeletonBones.Add(new SkeletonBone()
            {
                name = mStreamedRootObject.name,
                position = Vector3.zero,
                rotation = Quaternion.identity,
                scale = Vector3.one,
            });

            // Create remaining TPose bone definitions from Qtm joints
            foreach (var qssJoint in mSkeleton.QssJoints.ToList())
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
        }

        private void CreateQtmToMecanimJointNameMap(string skeletonName)
        {
            mMecanimToQtmBoneNames.Add("Hips", skeletonName + "_Hips");
            mMecanimToQtmBoneNames.Add("Spine", skeletonName + "_Spine");
            mMecanimToQtmBoneNames.Add("Chest", skeletonName + "_Spine1");
            mMecanimToQtmBoneNames.Add("Neck", skeletonName + "_Neck");
            mMecanimToQtmBoneNames.Add("Head", skeletonName + "_Head");

            mMecanimToQtmBoneNames.Add("RightShoulder", skeletonName + "_RightShoulder");
            mMecanimToQtmBoneNames.Add("RightUpperArm", skeletonName + "_RightArm");
            mMecanimToQtmBoneNames.Add("RightLowerArm", skeletonName + "_RightForeArm");
            mMecanimToQtmBoneNames.Add("RightHand", skeletonName + "_RightHand");
            mMecanimToQtmBoneNames.Add("LeftShoulder", skeletonName + "_LeftShoulder");
            mMecanimToQtmBoneNames.Add("LeftUpperArm", skeletonName + "_LeftArm");
            mMecanimToQtmBoneNames.Add("LeftLowerArm", skeletonName + "_LeftForeArm");
            mMecanimToQtmBoneNames.Add("LeftHand", skeletonName + "_LeftHand");

            mMecanimToQtmBoneNames.Add("RightUpperLeg", skeletonName + "_RightUpLeg");
            mMecanimToQtmBoneNames.Add("RightLowerLeg", skeletonName + "_RightLeg");
            mMecanimToQtmBoneNames.Add("RightFoot", skeletonName + "_RightFoot");
            mMecanimToQtmBoneNames.Add("RightToeBase", skeletonName + "_RightToeBase");
            mMecanimToQtmBoneNames.Add("LeftUpperLeg", skeletonName + "_LeftUpLeg");
            mMecanimToQtmBoneNames.Add("LeftLowerLeg", skeletonName + "_LeftLeg");
            mMecanimToQtmBoneNames.Add("LeftFoot", skeletonName + "_LeftFoot");
            mMecanimToQtmBoneNames.Add("LeftToeBase", skeletonName + "_LeftToeBase");
/*
            mMecanimToQtmBoneNames.Add("Right Thumb Proximal", skeletonName + "_RightHandThumb1");
            mMecanimToQtmBoneNames.Add("Right Thumb Intermediate", skeletonName + "_RightHandThumb2");
            mMecanimToQtmBoneNames.Add("Right Thumb Distal", skeletonName + "_RightHandThumb3");

            mMecanimToQtmBoneNames.Add("Left Thumb Proximal", skeletonName + "_LeftHandThumb1");
            mMecanimToQtmBoneNames.Add("Left Thumb Intermediate", skeletonName + "_LeftHandThumb2");
            mMecanimToQtmBoneNames.Add("Left Thumb Distal", skeletonName + "_LeftHandThumb3");

            mMecanimToQtmBoneNames.Add("Left Index Proximal", skeletonName + "_LeftHandIndex1");
            mMecanimToQtmBoneNames.Add("Left Index Intermediate", skeletonName + "_LeftHandIndex2");
            mMecanimToQtmBoneNames.Add("Left Index Distal", skeletonName + "_LeftHandIndex3");
            mMecanimToQtmBoneNames.Add("Right Index Proximal", skeletonName + "_RightHandIndex1");
            mMecanimToQtmBoneNames.Add("Right Index Intermediate", skeletonName + "_RightHandIndex2");
            mMecanimToQtmBoneNames.Add("Right Index Distal", skeletonName + "_RightHandIndex3");

            mMecanimToQtmBoneNames.Add("Left Middle Proximal", skeletonName + "_LeftHandMiddle1");
            mMecanimToQtmBoneNames.Add("Left Middle Intermediate", skeletonName + "_LeftHandMiddle2");
            mMecanimToQtmBoneNames.Add("Left Middle Distal", skeletonName + "_LeftHandMiddle3");
            mMecanimToQtmBoneNames.Add("Right Middle Proximal", skeletonName + "_RightHandMiddle1");
            mMecanimToQtmBoneNames.Add("Right Middle Intermediate", skeletonName + "_RightHandMiddle2");
            mMecanimToQtmBoneNames.Add("Right Middle Distal", skeletonName + "_RightHandMiddle3");

            mMecanimToQtmBoneNames.Add("Left Ring Proximal", skeletonName + "_LeftHandRing1");
            mMecanimToQtmBoneNames.Add("Left Ring Intermediate", skeletonName + "_LeftHandRing2");
            mMecanimToQtmBoneNames.Add("Left Ring Distal", skeletonName + "_LeftHandRing3");
            mMecanimToQtmBoneNames.Add("Right Ring Proximal", skeletonName + "_RightHandRing1");
            mMecanimToQtmBoneNames.Add("Right Ring Intermediate", skeletonName + "_RightHandRing2");
            mMecanimToQtmBoneNames.Add("Right Ring Distal", skeletonName + "_RightHandRing3");

            mMecanimToQtmBoneNames.Add("Left Little Proximal", skeletonName + "_LeftHandPinky1");
            mMecanimToQtmBoneNames.Add("Left Little Intermediate", skeletonName + "_LeftHandPinky2");
            mMecanimToQtmBoneNames.Add("Left Little Distal", skeletonName + "_LeftHandPinky3");
            mMecanimToQtmBoneNames.Add("Right Little Proximal", skeletonName + "_RightHandPinky1");
            mMecanimToQtmBoneNames.Add("Right Little Intermediate", skeletonName + "_RightHandPinky2");
            mMecanimToQtmBoneNames.Add("Right Little Distal", skeletonName + "_RightHandPinky3");
*/
        }
    }
}