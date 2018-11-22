// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;
using System.Collections;
using QTMRealTimeSDK;
using System.Collections.Generic;
using System.Linq;

namespace QualisysRealTime.Unity
{
    class RTSkeletonSegments : MonoBehaviour
    {
        public string SkeletonName = "Put QTM skeleton name here";

        protected RTClient rtClient;

        private Dictionary<uint, GameObject> segmentIDtoGameObjectDictionary = null;

        void Start()
        {
            rtClient = RTClient.GetInstance();
        }

        void Update()
        {
            if (rtClient == null) rtClient = RTClient.GetInstance();

            foreach (var skeleton in rtClient.Skeletons)
            {
                if (skeleton.Name != SkeletonName)
                    continue;

                if (segmentIDtoGameObjectDictionary == null)
                {
                    segmentIDtoGameObjectDictionary = new Dictionary<uint, GameObject>();

                    foreach (var segment in skeleton.Segments.ToList())
                    {
                        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        gameObject.GetComponent<Renderer>().material.color = Color.green;
                        gameObject.name = segment.Value.Name;
                        gameObject.transform.parent = segment.Value.ParentId == 0 ? this.gameObject.transform : segmentIDtoGameObjectDictionary[segment.Value.ParentId].transform;
                        gameObject.transform.localPosition = segment.Value.TPosition;
                        gameObject.transform.localRotation = segment.Value.TRotation;
                        gameObject.SetActive(false);
                        segmentIDtoGameObjectDictionary.Add(segment.Key, gameObject);
                    }
                }
                else
                {
                    foreach (var segment in skeleton.Segments.ToList())
                    {
                        var gameObject = segmentIDtoGameObjectDictionary[segment.Key];
                        gameObject.SetActive(true);
                        gameObject.transform.localPosition = segment.Value.Position;
                        gameObject.transform.localRotation = segment.Value.Rotation;
                    }
                }
                break;
            }
        }
    }
}