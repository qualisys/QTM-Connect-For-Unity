// Unity SDK for Qualisys Track Manager. Copyright 2015 Qualisys AB
//
using UnityEngine;
using System.Collections.Generic;

namespace QualisysRealTime.Unity
{
    public class RTGazeStream : MonoBehaviour
    {
        private List<GazeVector> gazeVectorData;
        private RTClient rtClient;
        private GameObject gazeRoot;

        [Range(0.1f, 10f)]
        public float gazeVectorLength = 1;

        private bool streaming = false;

        // Use this for initialization
        void Start()
        {
            rtClient = RTClient.GetInstance();
            gazeRoot = gameObject;
        }

        private void InitiateGazeVectors()
        {
            gazeVectorData = rtClient.GazeVectors;
        }

        // Update is called once per frame
        void Update()
        {
            if (rtClient == null) rtClient = RTClient.GetInstance();
            if (rtClient.GetStreamingStatus() && !streaming)
            {
                InitiateGazeVectors();
                streaming = true;
            }
            if (!rtClient.GetStreamingStatus() && streaming)
            {
                streaming = false;
                InitiateGazeVectors();
            }

            gazeVectorData = rtClient.GazeVectors;

            if (gazeVectorData == null && gazeVectorData.Count == 0)
                return;

            for (int i = 0; i < gazeVectorData.Count; i++)
            {
                if (gazeVectorData[i].Position.magnitude > 0)
                {
                    var start = gazeRoot.transform.TransformPoint(gazeVectorData[i].Position);
                    var direction = gazeRoot.transform.TransformDirection(gazeVectorData[i].Direction * gazeVectorLength);
                    Debug.DrawRay(start, direction, Color.red, 0);
                }
            }
        }
    }
}