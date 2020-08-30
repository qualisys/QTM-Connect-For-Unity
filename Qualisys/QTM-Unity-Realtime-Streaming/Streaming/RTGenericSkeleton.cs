// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QTMRealTimeSDK;

namespace QualisysRealTime.Unity
{
    public class RTGenericSkeleton : MonoBehaviour
    {
        [Header("QTM settings")]
        public string SkeletonName = "Put QTM skeleton name here";
        
        [Header("Unity Settings")]
        public string RigSegmentPrefix = "VF:";

        private GameObject rootObject = null;
        private Dictionary<uint, GameObject> mQTmSegmentIdToGameObjectCustom = new Dictionary<uint, GameObject>();
        private Dictionary<string, GameObject> segmentNameToGameObject = new Dictionary<string, GameObject>();
        private Skeleton qtmSkeletonCache = null;
        
        private void Awake()
        {
            Stack<Transform> s = new Stack<Transform>();
            s.Push(transform);
            while (s.Count > 0) 
            {
                var t = s.Pop();
                string name = t.gameObject.name;
                
                if (name.StartsWith(RigSegmentPrefix)) 
                {
                    segmentNameToGameObject.Add(name.Replace(RigSegmentPrefix, ""), t.gameObject);
                }
                
                foreach (Transform child in t) 
                {
                    s.Push(child);
                }
            }
        }

        void Update()
        {
            var skeleton = RTClient.GetInstance().GetSkeleton(SkeletonName);

            if (qtmSkeletonCache != skeleton)
            {
                qtmSkeletonCache = skeleton;

                if (qtmSkeletonCache == null)
                    return;

                // User defined avatar/skeleton
                rootObject = null;

                mQTmSegmentIdToGameObjectCustom = new Dictionary<uint, GameObject>(qtmSkeletonCache.Segments.Count);

                foreach (var segment in qtmSkeletonCache.Segments)
                {
                    var gameObject = segmentNameToGameObject[segment.Value.Name];
                    if (!gameObject)
                    { 
                        print("Didn't Find " + RigSegmentPrefix + ":" + segment.Value.Name);
                    }
                    else
                    {
                        // First one is assumed to be the root
                        if (rootObject == null)
                            rootObject = gameObject;

                        mQTmSegmentIdToGameObjectCustom[segment.Value.Id] = gameObject;
                    }
                }

                if (rootObject)
                {
                    rootObject.transform.SetParent(this.transform, false);
                }

                return;
            }

            if (qtmSkeletonCache == null)
                return;

            // Update all the game objects
            foreach (var segment in qtmSkeletonCache.Segments)
            {
                GameObject gameObject;
                if (mQTmSegmentIdToGameObjectCustom.TryGetValue(segment.Key, out gameObject))
                {
                    gameObject.transform.localPosition = segment.Value.Position;
                    gameObject.transform.localRotation = segment.Value.Rotation;
                }
            }
        }
      

    }
}