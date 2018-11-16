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

        private Dictionary<uint, GameObject> goJoints = null;

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

                if (goJoints == null)
                {
                    goJoints = new Dictionary<uint, GameObject>();

                    foreach (var joint in skeleton.QssJoints.ToList())
                    {
                        GameObject goJoint = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        goJoint.GetComponent<Renderer>().material.color = Color.green;
                        goJoint.name = joint.Value.Name;
                        goJoint.transform.parent = joint.Value.ParentId == 0 ? this.gameObject.transform : goJoints[joint.Value.ParentId].transform;
                        goJoint.transform.localPosition = joint.Value.TPosition;
                        goJoint.transform.localRotation = joint.Value.TRotation;
                        goJoint.SetActive(false);
                        goJoints.Add(joint.Key, goJoint);
                    }
                }
                else
                {
                    foreach (var joint in skeleton.QssJoints.ToList())
                    {
                        var goJoint = goJoints[joint.Key];
                        goJoint.SetActive(true);
                        goJoint.transform.localPosition = joint.Value.Position;
                        goJoint.transform.localRotation = joint.Value.Rotation;
                    }
                }
                break;
            }
        }
    }
}