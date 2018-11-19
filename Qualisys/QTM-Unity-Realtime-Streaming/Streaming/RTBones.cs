// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using QTMRealTimeSDK;

namespace QualisysRealTime.Unity
{
    /// Stream Bones from QTM
    public class RTBones : MonoBehaviour
    {
        private RTClient rtClient;
        private List<LineRenderer> lineRenderers;
        private GameObject allSegments;
        private Material material;
        public bool visibleSegments = true;
        [Range(0.001f, 1f)]
        public float segmentScale = 0.02f;

        void Start()
        {
            rtClient = RTClient.GetInstance();
            lineRenderers = new List<LineRenderer>();
            allSegments = new GameObject("Segments");
            allSegments.transform.parent = transform;
            allSegments.transform.localPosition = Vector3.zero;
            material = new Material(Shader.Find("Standard"));
        }


        private void InitiateSegments()
        {
            foreach (var segment in lineRenderers)
            {
                Destroy(segment.gameObject);
            }

            lineRenderers.Clear();

            var BoneData = rtClient.Bones;


            for (int i = 0; i < BoneData.Count; i++)
            {
                LineRenderer lineRenderer = new GameObject().AddComponent<LineRenderer>();
                lineRenderer.name = BoneData[i].From + " to " + BoneData[i].To;
                lineRenderer.transform.parent = allSegments.transform;
                lineRenderer.transform.localPosition = Vector3.zero;
                lineRenderer.material = material;
                lineRenderer.material.color = BoneData[i].Color;
                lineRenderer.useWorldSpace = false;
                lineRenderers.Add(lineRenderer);
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

            var BoneData = rtClient.Bones;

            if (BoneData == null || BoneData.Count == 0) return;

            if (lineRenderers.Count != BoneData.Count) InitiateSegments();

            allSegments.SetActive(true);
            for (int i = 0; i < BoneData.Count; i++)
            {
                if (BoneData[i].FromMarker.Position.magnitude > 0
                    && BoneData[i].ToMarker.Position.magnitude > 0)
                {
                    lineRenderers[i].SetPosition(0, BoneData[i].FromMarker.Position);
                    lineRenderers[i].SetPosition(1, BoneData[i].ToMarker.Position);
                    lineRenderers[i].startWidth = segmentScale;
                    lineRenderers[i].endWidth = segmentScale;

                    lineRenderers[i].gameObject.SetActive(true);
                }
                else
                {
                    //hide segments if we cant find markers.
                    lineRenderers[i].gameObject.SetActive(false);
                }
            }
        }
    }
}

