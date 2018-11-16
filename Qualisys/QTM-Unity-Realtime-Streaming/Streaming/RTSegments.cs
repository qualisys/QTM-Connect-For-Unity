// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using QTMRealTimeSDK;

namespace QualisysRealTime.Unity
{
    /// Stream segments from QTM
    public class RTSegments : MonoBehaviour
    {
        private RTClient rtClient;
        private List<LineRenderer> segments;
        private GameObject allSegments;
        private Material material;
        public bool visibleSegments = true;
        [Range(0.001f, 1f)]
        public float segmentScale = 0.02f;

        void Start()
        {
            rtClient = RTClient.GetInstance();
            segments = new List<LineRenderer>();
            allSegments = new GameObject("Segments");
            allSegments.transform.parent = transform;
            allSegments.transform.localPosition = Vector3.zero;
            material = new Material(Shader.Find("Standard"));
        }


        private void InitiateSegments()
        {
            foreach (var segment in segments)
            {
                Destroy(segment.gameObject);
            }

            segments.Clear();

            var segmentData = rtClient.Segments;


            for (int i = 0; i < segmentData.Count; i++)
            {
                LineRenderer lineRenderer = new GameObject().AddComponent<LineRenderer>();
                lineRenderer.name = segmentData[i].From + " to " + segmentData[i].To;
                lineRenderer.transform.parent = allSegments.transform;
                lineRenderer.transform.localPosition = Vector3.zero;
                lineRenderer.material = material;
                lineRenderer.material.color = segmentData[i].Color;
                lineRenderer.useWorldSpace = false;
                segments.Add(lineRenderer);
            }
        }

        void Update()
        {
            if (!visibleSegments)
            {
                allSegments.SetActive(false);
                return;
            }
            if (rtClient == null) rtClient = RTClient.GetInstance();
            if (!rtClient.GetStreamingStatus()) return;

            var segmentData = rtClient.Segments;

            if (segmentData == null || segmentData.Count == 0) return;

            if (segments.Count != segmentData.Count) InitiateSegments();

            allSegments.SetActive(true);
            for (int i = 0; i < segmentData.Count; i++)
            {
                if (segmentData[i].FromMarker.Position.magnitude > 0
                    && segmentData[i].ToMarker.Position.magnitude > 0)
                {
                    segments[i].SetPosition(0, segmentData[i].FromMarker.Position);
                    segments[i].SetPosition(1, segmentData[i].ToMarker.Position);
                    segments[i].startWidth = segmentScale;
                    segments[i].endWidth = segmentScale;

                    segments[i].gameObject.SetActive(true);
                }
                else
                {
                    //hide segments if we cant find markers.
                    segments[i].gameObject.SetActive(false);
                }
            }
        }
    }
}

