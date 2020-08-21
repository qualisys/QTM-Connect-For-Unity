// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;

namespace QualisysRealTime.Unity
{
    class RTUnlabeledMarker : MonoBehaviour
    {
        public uint MarkerId = 0;
        public Vector3 PositionOffset = new Vector3(0, 0, 0);

        protected RTClient rtClient;
        protected UnlabeledMarker marker;

        public virtual void applyMarkerTransform()
        {
            if (!float.IsNaN(marker.Position.sqrMagnitude)) //just to avoid error when position is NaN
            {
                transform.position = marker.Position + PositionOffset;
                if (transform.parent)
                    transform.position += transform.parent.position;
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

            marker = rtClient.GetUnlabeledMarker(MarkerId);
            if (marker != null)
            {
                this.applyMarkerTransform();
            }
        }
    }
}