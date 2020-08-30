// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;
using System.Collections.Generic;

namespace QualisysRealTime.Unity
{
    public class RTGenericSkeleton : MonoBehaviour
    {
        [Header("QTM settings")]
        public string SkeletonName = "Put QTM skeleton name here";
        
        [Header("Unity Settings")]
        public string RigSegmentPrefix = "VF:";

        private Dictionary<uint, GameObject> mQtmSegmentIdToGameObject = new Dictionary<uint, GameObject>();
        private Dictionary<string, GameObject> mSegmentNameToGameObject = new Dictionary<string, GameObject>();
        private Skeleton mQtmSkeletonCache = null;
        
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
                    mSegmentNameToGameObject.Add(name.Replace(RigSegmentPrefix, ""), t.gameObject);
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

            if (mQtmSkeletonCache != skeleton)
            {
                mQtmSkeletonCache = skeleton;

                if (mQtmSkeletonCache == null)
                    return;

                GameObject rootObject = null;

                mQtmSegmentIdToGameObject.Clear();

                foreach (var segment in mQtmSkeletonCache.Segments)
                {
                    GameObject go;
                    if (!mSegmentNameToGameObject.TryGetValue(segment.Value.Name, out go))
                    { 
                        Debug.Log("Didn't Find " + RigSegmentPrefix + ":" + segment.Value.Name);
                    }
                    else
                    {
                        // First one is assumed to be the root
                        if (rootObject == null)
                            rootObject = go;

                        mQtmSegmentIdToGameObject[segment.Value.Id] = go;
                    }
                }

                if (rootObject)
                {
                    rootObject.transform.SetParent(this.transform, false);
                }

                return;
            }

            if (mQtmSkeletonCache == null)
                return;

            // Update all the game objects
            foreach (var segment in mQtmSkeletonCache.Segments)
            {
                GameObject gameObject;
                if (mQtmSegmentIdToGameObject.TryGetValue(segment.Key, out gameObject))
                {
                    gameObject.transform.localPosition = segment.Value.Position;
                    gameObject.transform.localRotation = segment.Value.Rotation;
                }
            }
        }
    }
}
