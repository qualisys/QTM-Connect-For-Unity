// Unity SDK for Qualisys Track Manager. Copyright 2015-2021 Qualisys AB
//

#define FINGERS
//#define DEBUG_SKELETON

using UnityEngine;
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
        private GameObject mStreamedRootObject;
        private Dictionary<uint, GameObject> mQTmSegmentIdToGameObject;
        private Dictionary<string, string> mMecanimToQtmSegmentNames = new Dictionary<string, string>();

        private HumanPoseHandler mSourcePoseHandler;
        private HumanPoseHandler mDestiationPoseHandler;

        private Skeleton mQtmSkeletonCache;
#if DEBUG_SKELETON
        private GameObject mTPoseRoot;
#endif
        void Update()
        {
            var skeleton = RTClient.GetInstance().GetSkeleton(SkeletonName);

            if (mQtmSkeletonCache != skeleton)
            {
                mQtmSkeletonCache = skeleton;

                if (mQtmSkeletonCache == null)
                    return;

                CreateMecanimToQtmSegmentNames(SkeletonName);

                if(mStreamedRootObject != null)
                    GameObject.Destroy(mStreamedRootObject);
                mStreamedRootObject = new GameObject(this.SkeletonName);

#if DEBUG_SKELETON
                if (mTPoseRoot != null)
                    GameObject.Destroy(mTPoseRoot);
                mTPoseRoot = new GameObject();
                mStreamedRootObject.AddComponent<DebugHierarchyRotations>().color = Color.yellow;
#endif

                mQTmSegmentIdToGameObject = new Dictionary<uint, GameObject>(mQtmSkeletonCache.Segments.Count);

                foreach (var segment in mQtmSkeletonCache.Segments)
                {
                    var gameObject = new GameObject(this.SkeletonName + "_" + segment.Value.Name);
                    gameObject.transform.parent = segment.Value.ParentId == 0 ? mStreamedRootObject.transform : mQTmSegmentIdToGameObject[segment.Value.ParentId].transform;
                    gameObject.transform.localPosition = segment.Value.TPosition;
                    gameObject.transform.localRotation = segment.Value.TRotation;
                    mQTmSegmentIdToGameObject[segment.Value.Id] = gameObject;
                }

                mStreamedRootObject.transform.SetParent(this.transform, false);
#if DEBUG_SKELETON
                mTPoseRoot = GameObject.Instantiate<GameObject>(mStreamedRootObject.gameObject, this.transform, false);
                mTPoseRoot.name = this.SkeletonName + "-TPose";
                mTPoseRoot.GetComponent<DebugHierarchyRotations>().color = Color.white;
#endif
                BuildMecanimAvatarFromQtmTPose();

                return;
            }

            if (mQtmSkeletonCache == null)
                return;

            //Update all the game objects
            foreach (var segment in mQtmSkeletonCache.Segments)
            {
                GameObject gameObject;
                if (mQTmSegmentIdToGameObject.TryGetValue(segment.Key, out gameObject))
                {
                    gameObject.transform.localPosition = segment.Value.Position;
                    gameObject.transform.localRotation = segment.Value.Rotation;
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
            var humanBones = new List<HumanBone>(mQtmSkeletonCache.Segments.Count);
            for (int index = 0; index < HumanTrait.BoneName.Length; index++)
            {
                var humanBoneName = HumanTrait.BoneName[index];
                if (mMecanimToQtmSegmentNames.ContainsKey(humanBoneName))
                {
                    var bone = new HumanBone()
                    {
                        humanName = humanBoneName,
                        boneName = mMecanimToQtmSegmentNames[humanBoneName],
                    };
                    bone.limit.useDefaultValues = true;
                    humanBones.Add(bone);
                }
            }

            var skeletonBones = new List<SkeletonBone>(mQtmSkeletonCache.Segments.Count + 1);
            skeletonBones.Add(new SkeletonBone()
            {
                name = this.SkeletonName,
                position = Vector3.zero,
                // In QTM default poses are facing +Y which becomes -Z in Unity. 
                // We rotate 180 degrees to match unity forward.
                rotation = Quaternion.AngleAxis(180, Vector3.up), 
                scale = Vector3.one,
            }); 

            foreach (var segment in mQtmSkeletonCache.Segments)
            {
                skeletonBones.Add(new SkeletonBone()
                {
                    name = this.SkeletonName + "_" + segment.Value.Name,
                    position = segment.Value.TPosition,
                    rotation = segment.Value.TRotation,
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

        private void CreateMecanimToQtmSegmentNames(string skeletonName)
        {
            mMecanimToQtmSegmentNames.Clear();
            mMecanimToQtmSegmentNames.Add("RightShoulder", skeletonName + "_RightShoulder");
            mMecanimToQtmSegmentNames.Add("RightUpperArm", skeletonName + "_RightArm");
            mMecanimToQtmSegmentNames.Add("RightLowerArm", skeletonName + "_RightForeArm");
            mMecanimToQtmSegmentNames.Add("RightHand", skeletonName + "_RightHand");
            mMecanimToQtmSegmentNames.Add("LeftShoulder", skeletonName + "_LeftShoulder");
            mMecanimToQtmSegmentNames.Add("LeftUpperArm", skeletonName + "_LeftArm");
            mMecanimToQtmSegmentNames.Add("LeftLowerArm", skeletonName + "_LeftForeArm");
            mMecanimToQtmSegmentNames.Add("LeftHand", skeletonName + "_LeftHand");

            mMecanimToQtmSegmentNames.Add("RightUpperLeg", skeletonName + "_RightUpLeg");
            mMecanimToQtmSegmentNames.Add("RightLowerLeg", skeletonName + "_RightLeg");
            mMecanimToQtmSegmentNames.Add("RightFoot", skeletonName + "_RightFoot");
            mMecanimToQtmSegmentNames.Add("RightToeBase", skeletonName + "_RightToeBase");
            mMecanimToQtmSegmentNames.Add("LeftUpperLeg", skeletonName + "_LeftUpLeg");
            mMecanimToQtmSegmentNames.Add("LeftLowerLeg", skeletonName + "_LeftLeg");
            mMecanimToQtmSegmentNames.Add("LeftFoot", skeletonName + "_LeftFoot");
            mMecanimToQtmSegmentNames.Add("LeftToeBase", skeletonName + "_LeftToeBase");

            mMecanimToQtmSegmentNames.Add("Hips", skeletonName + "_Hips");
            mMecanimToQtmSegmentNames.Add("Spine", skeletonName + "_Spine");
            mMecanimToQtmSegmentNames.Add("Chest", skeletonName + "_Spine1");
            mMecanimToQtmSegmentNames.Add("UpperChest", skeletonName + "_Spine2");
            mMecanimToQtmSegmentNames.Add("Neck", skeletonName + "_Neck");
            mMecanimToQtmSegmentNames.Add("Head", skeletonName + "_Head");
#if FINGERS
            mMecanimToQtmSegmentNames.Add("Left Thumb Proximal", skeletonName + "_LeftHandThumb1");
            mMecanimToQtmSegmentNames.Add("Left Thumb Intermediate", skeletonName + "_LeftHandThumb2");
            mMecanimToQtmSegmentNames.Add("Left Thumb Distal", skeletonName + "_LeftHandThumb3");
            mMecanimToQtmSegmentNames.Add("Left Index Proximal", skeletonName + "_LeftHandIndex1");
            mMecanimToQtmSegmentNames.Add("Left Index Intermediate", skeletonName + "_LeftHandIndex2");
            mMecanimToQtmSegmentNames.Add("Left Index Distal", skeletonName + "_LeftHandIndex3");
            mMecanimToQtmSegmentNames.Add("Left Middle Proximal", skeletonName + "_LeftHandMiddle1");
            mMecanimToQtmSegmentNames.Add("Left Middle Intermediate", skeletonName + "_LeftHandMiddle2");
            mMecanimToQtmSegmentNames.Add("Left Middle Distal", skeletonName + "_LeftHandMiddle3");
            mMecanimToQtmSegmentNames.Add("Left Ring Proximal", skeletonName + "_LeftHandRing1");
            mMecanimToQtmSegmentNames.Add("Left Ring Intermediate", skeletonName + "_LeftHandRing2");
            mMecanimToQtmSegmentNames.Add("Left Ring Distal", skeletonName + "_LeftHandRing3");
            mMecanimToQtmSegmentNames.Add("Left Little Proximal", skeletonName + "_LeftHandPinky1");
            mMecanimToQtmSegmentNames.Add("Left Little Intermediate", skeletonName + "_LeftHandPinky2");
            mMecanimToQtmSegmentNames.Add("Left Little Distal", skeletonName + "_LeftHandPinky3");

            mMecanimToQtmSegmentNames.Add("Right Thumb Proximal", skeletonName + "_RightHandThumb1");
            mMecanimToQtmSegmentNames.Add("Right Thumb Intermediate", skeletonName + "_RightHandThumb2");
            mMecanimToQtmSegmentNames.Add("Right Thumb Distal", skeletonName + "_RightHandThumb3");
            mMecanimToQtmSegmentNames.Add("Right Index Proximal", skeletonName + "_RightHandIndex1");
            mMecanimToQtmSegmentNames.Add("Right Index Intermediate", skeletonName + "_RightHandIndex2");
            mMecanimToQtmSegmentNames.Add("Right Index Distal", skeletonName + "_RightHandIndex3");
            mMecanimToQtmSegmentNames.Add("Right Middle Proximal", skeletonName + "_RightHandMiddle1");
            mMecanimToQtmSegmentNames.Add("Right Middle Intermediate", skeletonName + "_RightHandMiddle2");
            mMecanimToQtmSegmentNames.Add("Right Middle Distal", skeletonName + "_RightHandMiddle3");
            mMecanimToQtmSegmentNames.Add("Right Ring Proximal", skeletonName + "_RightHandRing1");
            mMecanimToQtmSegmentNames.Add("Right Ring Intermediate", skeletonName + "_RightHandRing2");
            mMecanimToQtmSegmentNames.Add("Right Ring Distal", skeletonName + "_RightHandRing3");
            mMecanimToQtmSegmentNames.Add("Right Little Proximal", skeletonName + "_RightHandPinky1");
            mMecanimToQtmSegmentNames.Add("Right Little Intermediate", skeletonName + "_RightHandPinky2");
            mMecanimToQtmSegmentNames.Add("Right Little Distal", skeletonName + "_RightHandPinky3");
#endif
        }
    }
}
