// Unity SDK for Qualisys Track Manager. Copyright 2015 Qualisys AB
//
using UnityEngine;
using System.Collections;
using QTMRealTimeSDK;

namespace QualisysRealTime.Unity
{
    class RTObject : MonoBehaviour
    {
        public string ObjectName = "Put QTM 6DOF object name here";
        public Vector3 PositionOffset = new Vector3(0, 0, 0);
        public Vector3 EulerOffset = new Vector3(0, 0, 0);

        private RTClient rtClient;

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            rtClient = RTClient.GetInstance();
            if (rtClient == null)
                return;

            SixDOFBody body = rtClient.GetBody(ObjectName);
            if (body != null)
            {
                if (body.Position.magnitude > 0) //just to avoid error when position is NaN
                {
                    transform.position = body.Position + PositionOffset;
                    transform.rotation = body.Rotation * Quaternion.Euler(EulerOffset);
                }
            }
        }
    }
}