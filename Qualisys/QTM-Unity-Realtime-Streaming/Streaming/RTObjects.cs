// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;
using System.Collections.Generic;

namespace QualisysRealTime.Unity
{
    public class RTObjects : MonoBehaviour
    {
        private List<SixDOFBody> bodies;
        private RTClient rtClient;
        private GameObject bodyRootGO;
        private List<GameObject> bodiesGO;

        public bool visibleBodies = true;

        [Range(0.001f, 1f)]
        public float bodyScale = 0.05f;

        private bool streaming = false;

        // Use this for initialization
        void Start()
        {
            rtClient = RTClient.GetInstance();
            bodiesGO = new List<GameObject>();
            bodyRootGO = gameObject;
        }


        private void InitiateBodies()
        {
            foreach (var marker in bodiesGO)
            {
                Destroy(marker);
            }

            bodiesGO.Clear();
            bodies = rtClient.Bodies;

            for (int i = 0; i < bodies.Count; i++)
            {
                GameObject newBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newBody.name = bodies[i].Name;
                newBody.transform.parent = bodyRootGO.transform;
                newBody.transform.localScale = Vector3.one * bodyScale;
                newBody.SetActive(false);
                bodiesGO.Add(newBody);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (rtClient == null) rtClient = RTClient.GetInstance();
            if (rtClient.GetStreamingStatus() && !streaming)
            {
                InitiateBodies();
                streaming = true;
            }
            if (!rtClient.GetStreamingStatus() && streaming)
            {
                streaming = false;
                InitiateBodies();
            }

            bodies = rtClient.Bodies;

            if (bodies == null || bodies.Count == 0)
                return;

            if (bodiesGO.Count != bodies.Count)
            {
                InitiateBodies();
            }

            for (int i = 0; i < bodies.Count; i++)
            {
                if (!float.IsNaN(bodies[i].Position.sqrMagnitude))
                {
                    bodiesGO[i].name = bodies[i].Name;
                    bodiesGO[i].GetComponent<Renderer>().material.color = bodies[i].Color;
                    bodiesGO[i].transform.localPosition = bodies[i].Position;
                    bodiesGO[i].transform.localRotation = bodies[i].Rotation;
                    bodiesGO[i].SetActive(true);
                    bodiesGO[i].GetComponent<Renderer>().enabled = visibleBodies;
                    bodiesGO[i].transform.localScale = Vector3.one * bodyScale;
                }
                else
                {
                    // Hide markers if we cant find them
                    bodiesGO[i].SetActive(false);
                }
            }
        }
    }
}