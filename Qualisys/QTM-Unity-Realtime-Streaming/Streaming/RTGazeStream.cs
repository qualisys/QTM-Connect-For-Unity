// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;
using System.Collections.Generic;

namespace QualisysRealTime.Unity
{
    public class RTGazeStream : MonoBehaviour
    {
        private List<GazeVector> gazeVectorData;
        private RTClient rtClient;

        private List<LineRenderer> gazeVectors;
        private GameObject gazeRoot;
        private Material material;

        [Range(0.1f, 10f)]
        public float gazeVectorLength = 2.0f;

        [Range(0.001f, 0.1f)]
        public float gazeVectorWidth = 0.015f;

        private bool streaming = false;

        // Use this for initialization
        void Start()
        {
            rtClient = RTClient.GetInstance();

            gazeVectors = new List<LineRenderer>();
            gazeRoot = new GameObject("GazeVectors");
            gazeRoot.transform.parent = transform;
            gazeRoot.transform.localPosition = Vector3.zero;
            material = new Material(Shader.Find("Standard"));

            gazeVectorLength = 1;
            gazeVectorWidth = 0.01f;
        }

        private void InitiateGazeVectors()
        {
            foreach (var gazeVector in gazeVectors)
            {
                Destroy(gazeVector.gameObject);
            }

            gazeVectors.Clear();
            gazeVectorData = rtClient.GazeVectors;

            for (int i = 0; i < gazeVectorData.Count; i++)
            {
                LineRenderer lineRenderer = new GameObject().AddComponent<LineRenderer>();
                lineRenderer.transform.parent = gazeRoot.transform;
                lineRenderer.transform.localPosition = Vector3.zero;
                lineRenderer.material = material;
                lineRenderer.material.color = Color.red;
                lineRenderer.useWorldSpace = false;
                lineRenderer.name = gazeVectorData[i].Name;
                gazeVectors.Add(lineRenderer);
            }
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
            {
                return;
            }

            if (gazeVectors.Count != gazeVectorData.Count)
            {
                InitiateGazeVectors();
            }


            gazeRoot.SetActive(true);
            for (int i = 0; i < gazeVectorData.Count; i++)
            {
                if (!float.IsNaN(gazeVectorData[i].Position.sqrMagnitude))
                {
                    gazeVectors[i].SetPosition(0, gazeVectorData[i].Position);
                    gazeVectors[i].SetPosition(1, gazeVectorData[i].Position + gazeVectorData[i].Direction * gazeVectorLength);
                    gazeVectors[i].startWidth = gazeVectorWidth;
                    gazeVectors[i].endWidth = gazeVectorWidth;
                    gazeVectors[i].gameObject.SetActive(true);
                }
                else
                {
                    gazeVectors[i].gameObject.SetActive(true);
                }
            }
        }
    }
}