// Unity SDK for Qualisys Track Manager. Copyright 2015 Qualisys AB
//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using QTMRealTimeSDK;

namespace QualisysRealTime.Unity
{
    /// Stream bones from QTM
    public class RTBones : MonoBehaviour
    {
        private RTClient rtClient;
        private List<LineRenderer> bones;
        private GameObject allBones;

        public bool visibleBones = true;
        [Range(0.001f, 0.5f)]
        public float boneScale = 0.01f;
        public Color color = Color.yellow;

        // Use this for initialization
        void Start()
        {
            rtClient = RTClient.GetInstance();
            bones = new List<LineRenderer>();
            allBones = new GameObject("Bones");
            allBones.transform.parent = transform;
            allBones.transform.localPosition = Vector3.zero;
        }


        private void InitiateBones()
        {
            foreach (var bone in bones)
            {
                Destroy(bone.gameObject);
            }

            bones.Clear();
            var boneData = rtClient.Bones;

            Material material = new Material(Shader.Find("Unlit/Color"));
            for (int i = 0; i < boneData.Count; i++)
            {
                LineRenderer lineRenderer = new GameObject().AddComponent<LineRenderer>();
                lineRenderer.name = boneData[i].From + " to " + boneData[i].To;
                lineRenderer.transform.parent = allBones.transform;
                lineRenderer.transform.localPosition = Vector3.zero;
                lineRenderer.material = material;
                lineRenderer.useWorldSpace = false;
                bones.Add(lineRenderer);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!visibleBones)
            {
                allBones.SetActive(false);
                return;
            }
            if (rtClient == null) rtClient = RTClient.GetInstance();

            if (!rtClient.GetStreamingStatus()) return;

            var boneData = rtClient.Bones;

            if (boneData == null && boneData.Count == 0) return;

            if (bones.Count != boneData.Count) InitiateBones();

            allBones.SetActive(true);
            for (int i = 0; i < boneData.Count; i++)
            {
                if (boneData[i].FromMarker.Position.magnitude > 0
                    && boneData[i].ToMarker.Position.magnitude > 0)
                {
                    bones[i].SetPosition(0, boneData[i].FromMarker.Position);
                    bones[i].SetPosition(1, boneData[i].ToMarker.Position);
                    bones[i].SetWidth(boneScale, boneScale);
                    bones[i].material.color = color;
                    bones[i].gameObject.SetActive(true);
                }
                else
                {
                    //hide bones if we cant find markers.
                    bones[i].gameObject.SetActive(false);
                }
            }
        }
    }
}

