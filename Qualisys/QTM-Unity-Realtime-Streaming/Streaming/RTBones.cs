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
        private List<LineRenderer> allLineRenderers;
        private GameObject lineRenderer;
        private Material material;
        public bool visibleSegments = true;
        [Range(0.001f, 1f)]
        public float lineWidth = 0.02f;

        void Start()
        {
            rtClient = RTClient.GetInstance();
            allLineRenderers = new List<LineRenderer>();
            lineRenderer = new GameObject("LineRenderers");
            lineRenderer.transform.parent = transform;
            lineRenderer.transform.localPosition = Vector3.zero;
            material = new Material(Shader.Find("Standard"));
        }


        private void InitiateLineRenderers()
        {
            foreach (var lineRenderer in allLineRenderers)
            {
                Destroy(lineRenderer.gameObject);
            }

            allLineRenderers.Clear();

            var BoneData = rtClient.Bones;


            for (int i = 0; i < BoneData.Count; i++)
            {
                LineRenderer lineRenderer = new GameObject().AddComponent<LineRenderer>();
                lineRenderer.name = BoneData[i].From + " to " + BoneData[i].To;
                lineRenderer.transform.parent = this.lineRenderer.transform;
                lineRenderer.transform.localPosition = Vector3.zero;
                lineRenderer.material = material;
                lineRenderer.material.color = BoneData[i].Color;
                lineRenderer.useWorldSpace = false;
                allLineRenderers.Add(lineRenderer);
            }
        }
        
        void Update()
        {
            if (!visibleSegments)
            {
                lineRenderer.SetActive(false);
                return;
            }
            if (rtClient == null) rtClient = RTClient.GetInstance();
            if (!rtClient.GetStreamingStatus()) return;

            var BoneData = rtClient.Bones;

            if (BoneData == null || BoneData.Count == 0) return;

            if (allLineRenderers.Count != BoneData.Count) InitiateLineRenderers();

            lineRenderer.SetActive(true);
            for (int i = 0; i < BoneData.Count; i++)
            {
                if (BoneData[i].FromMarker.Position.magnitude > 0
                    && BoneData[i].ToMarker.Position.magnitude > 0)
                {
                    allLineRenderers[i].SetPosition(0, BoneData[i].FromMarker.Position);
                    allLineRenderers[i].SetPosition(1, BoneData[i].ToMarker.Position);
                    allLineRenderers[i].startWidth = lineWidth;
                    allLineRenderers[i].endWidth = lineWidth;

                    allLineRenderers[i].gameObject.SetActive(true);
                }
                else
                {
                    //hide segments if we cant find markers.
                    allLineRenderers[i].gameObject.SetActive(false);
                }
            }
        }
    }
}

