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
        private List<Bone> boneData;
        private RTClient rtClient;
        private GameObject markerRoot;
        private List<LineRenderer> bones;

        public bool visibleBones = true;

        [Range(0.001f, 0.5f)]
        public float boneScale = 0.01f;

        public bool UseMakersColor = true;
        public Color color = Color.yellow;

        private bool streaming = false;

        // Use this for initialization
        void Start()
        {
            rtClient = RTClient.GetInstance();
            bones = new List<LineRenderer>();
            markerRoot = gameObject;
        }


        private void InitiateBones()
        {
            foreach (var bone in bones)
            {
                Destroy(bone.gameObject);
            }

            bones.Clear();
            boneData = rtClient.Bones;

            Material material = new Material(Shader.Find("Particles/Additive"));
            for (int i = 0; i < boneData.Count; i++)
            {
                GameObject newBone = new GameObject();
                LineRenderer lineRenderer = newBone.AddComponent<LineRenderer>();
                lineRenderer.name = boneData[i].From + " to " + boneData[i].To;
                lineRenderer.transform.parent = markerRoot.transform;
                lineRenderer.transform.localPosition = Vector3.zero;
                lineRenderer.material = material;
                lineRenderer.useWorldSpace = false;
                bones.Add(lineRenderer);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (rtClient.GetStreamingStatus() && !streaming)
            {
                InitiateBones();
                streaming = true;
            }
            if (!rtClient.GetStreamingStatus() && streaming)
            {
                streaming = false;
                InitiateBones();
            }

            boneData = rtClient.Bones;

            if (boneData == null && boneData.Count == 0)
                return;

            if (bones.Count != boneData.Count)
            {
                InitiateBones();
            }

            Color fromColor, toColor;
            for (int i = 0; i < boneData.Count; i++)
            {
                if (   boneData[i].FromMarker.Position.magnitude > 0
                    && boneData[i].ToMarker.Position.magnitude > 0)
                {
                    bones[i].SetPosition(0, boneData[i].FromMarker.Position);
                    bones[i].SetPosition(1, boneData[i].ToMarker.Position);
                    bones[i].SetWidth(boneScale, boneScale);
                    if (UseMakersColor)
                    {
                        fromColor = boneData[i].FromMarker.Color;
                        toColor = boneData[i].ToMarker.Color;
                    }
                    else
                    {
                        fromColor = toColor = color;
                    }
                    bones[i].SetColors(fromColor, toColor);
                    bones[i].gameObject.SetActive(true);
                    bones[i].GetComponent<Renderer>().enabled = visibleBones;
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

