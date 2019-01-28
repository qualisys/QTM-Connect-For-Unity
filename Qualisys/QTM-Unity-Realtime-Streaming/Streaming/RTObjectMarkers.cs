// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;
using System.Collections.Generic;
using System;

namespace QualisysRealTime.Unity
{
    class RTObjectMarkers : RTObject
    {
        public bool useObjectOrientation = true;
        public bool visibleMarkers = true;

        [Range(0.001f, 1f)]
        public float markerScale = 0.05f;

        public Vector3 RelativePosition
        {
            get
            {
                return PositionOffset - bodyPosition;
            }
        }
        public Quaternion RelativeRotation
        {
            get
            {
                return Quaternion.Euler(RotationOffset) * Quaternion.Inverse(bodyRotation);
            }
        }

        protected List<LabeledMarker> markers;
        protected List<GameObject> markerGOs;

        private Vector3 bodyPosition = Vector3.zero;
        private Quaternion bodyRotation = Quaternion.identity;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        protected override void applyBodyTransform(SixDOFBody body)
        {
            if (useObjectOrientation)
                base.applyBodyTransform(body);

            // Avoid position is NaN
            if (!float.IsNaN(body.Position.sqrMagnitude))
            {
                bodyPosition = body.Position;
                bodyRotation = body.Rotation;
            }

            // Get the body markers, of which the 6DOF body consists
            markers = rtClient.Markers;
            if (markers == null || markers.Count == 0)
                return;

            if (markerGOs == null)
            {
                InitiateMarkers();
            }

            foreach (GameObject markerGO in markerGOs)
            {
                LabeledMarker marker = rtClient.GetMarker(markerGO.name);

                // Show and update existing markers
                if (marker != null && !float.IsNaN(marker.Position.sqrMagnitude))
                {
                    markerGO.transform.localPosition = Quaternion.Inverse(bodyRotation) * (marker.Position - bodyPosition);
                    markerGO.transform.localScale = Vector3.one * markerScale;
                    markerGO.SetActive(true);
                    markerGO.GetComponent<Renderer>().enabled = visibleMarkers;
                    markerGO.GetComponent<Renderer>().material.color = marker.Color;
                }
                else
                {
                    // Hide not existing markers.
                    markerGO.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitiateMarkers()
        {
            if (markerGOs == null)
                markerGOs = new List<GameObject>();
            foreach (var markerGO in markerGOs)
            {
                Destroy(markerGO);
            }
            markerGOs.Clear();

            markers = rtClient.Markers;
            foreach (LabeledMarker marker in markers)
            {
                if (marker.Name.StartsWith(this.ObjectName, StringComparison.CurrentCultureIgnoreCase))
                {
                    GameObject markerGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    markerGO.name = marker.Name;
                    markerGO.transform.parent = this.gameObject.transform;
                    markerGO.transform.localScale = Vector3.one * markerScale;
                    markerGO.SetActive(false);
                    markerGOs.Add(markerGO);
                }
            }
        }
    }
}
