// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;

namespace QualisysRealTime.Unity
{
    class RTObject : MonoBehaviour
    {
        public string ObjectName = "Put QTM 6DOF object name here";
        public Vector3 PositionOffset = new Vector3(0, 0, 0);
        public Vector3 RotationOffset = new Vector3(0, 0, 0);

        protected RTClient rtClient;

        protected virtual void applyBodyTransform(SixDOFBody body)
        {
            if (!float.IsNaN(body.Position.sqrMagnitude)) //just to avoid error when position is NaN
            {
                transform.position = body.Position + PositionOffset;
                if (transform.parent) transform.position += transform.parent.position;
                transform.rotation = body.Rotation * Quaternion.Euler(RotationOffset);
                if (transform.parent) transform.rotation *= transform.parent.rotation;
            }
        }

        // Use this for initialization
        void Start()
        {
            rtClient = RTClient.GetInstance();
        }

        // Update is called once per frame
        void Update()
        {
            if (rtClient == null) rtClient = RTClient.GetInstance();

            var body = rtClient.GetBody(ObjectName);
            if (body != null)
                applyBodyTransform(body);
        }
    }
}