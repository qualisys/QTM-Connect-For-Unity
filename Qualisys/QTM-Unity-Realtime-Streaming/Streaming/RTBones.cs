// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;
using System.Collections.Generic;

namespace QualisysRealTime.Unity
{
    /// Stream bones from QTM
    public class RTBones : MonoBehaviour
    {
        private RTClient rtClient;
        private List<LineRenderer> bones;
        private GameObject allBones;
        private Material material;
        public bool visibleBones = true;
        [Range(0.001f, 1f)]
        public float boneScale = 0.02f;

        void Start()
        {
            rtClient = RTClient.GetInstance();
            bones = new List<LineRenderer>();
            allBones = new GameObject("Bones");
            allBones.transform.parent = transform;
            allBones.transform.localPosition = Vector3.zero;
            material = new Material(Shader.Find("Standard"));
        }


        private void InitiateBones()
        {
            foreach (var bone in bones)
            {
                Destroy(bone.gameObject);
            }

            bones.Clear();

            var boneData = rtClient.Bones;


            for (int i = 0; i < boneData.Count; i++)
            {
                LineRenderer lineRenderer = new GameObject().AddComponent<LineRenderer>();
                lineRenderer.name = boneData[i].From + " to " + boneData[i].To;
                lineRenderer.transform.parent = allBones.transform;
                lineRenderer.transform.localPosition = Vector3.zero;
                lineRenderer.material = material;
                lineRenderer.material.color = boneData[i].Color;
                lineRenderer.useWorldSpace = false;
                bones.Add(lineRenderer);
            }
        }

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

            if (boneData == null || boneData.Count == 0) return;

            if (bones.Count != boneData.Count) InitiateBones();

            allBones.SetActive(true);
            for (int i = 0; i < boneData.Count; i++)
            {
                if (!float.IsNaN(boneData[i].FromMarker.Position.sqrMagnitude) &&
                    !float.IsNaN(boneData[i].ToMarker.Position.sqrMagnitude))
                {
                    bones[i].SetPosition(0, boneData[i].FromMarker.Position);
                    bones[i].SetPosition(1, boneData[i].ToMarker.Position);
                    bones[i].startWidth = boneScale;
                    bones[i].endWidth = boneScale;

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

