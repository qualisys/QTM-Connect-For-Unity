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

        private Dictionary<uint, GameObject> goSegments = null;

        // Use this for initialization
        void Start()
        {
            rtClient = RTClient.GetInstance();
        }

        // Update is called once per frame
        void Update()
        {
            if (rtClient == null) rtClient = RTClient.GetInstance();

            foreach (var skeleton in rtClient.Skeletons)
            {
                if (skeleton.Name != SkeletonName)
                    continue;

                if (goSegments == null)
                {
                    goSegments = new Dictionary<uint, GameObject>();

                    foreach (var qtmSkeletonSegment in skeleton.QtmSkeletonSegments.ToList())
                    {
                        GameObject goSegment = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        goSegment.GetComponent<Renderer>().material.color = Color.green;
                        goSegment.name = qtmSkeletonSegment.Value.Name;
                        goSegment.transform.parent = qtmSkeletonSegment.Value.ParentId == 0 ? this.gameObject.transform : goSegments[qtmSkeletonSegment.Value.ParentId].transform;
                        goSegment.transform.localPosition = qtmSkeletonSegment.Value.TPosition;
                        goSegment.transform.localRotation = qtmSkeletonSegment.Value.TRotation;
                        goSegment.SetActive(false);
                        goSegments.Add(qtmSkeletonSegment.Key, goSegment);
                    }
                }
                else
                {
                    foreach (var qtmSkeletonSegment in skeleton.QtmSkeletonSegments.ToList())
                    {
                        var goSegment = goSegments[qtmSkeletonSegment.Key];
                        goSegment.SetActive(true);
                        goSegment.transform.localPosition = qtmSkeletonSegment.Value.Position;
                        goSegment.transform.localRotation = qtmSkeletonSegment.Value.Rotation;
                    }
                }
                break;
            }
        }
    }
}