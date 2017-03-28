using UnityEngine;
using System.Collections.Generic;

namespace QualisysRealTime.Unity
{
    class RTObjectMarkers : RTObject
    {
        public bool useObjectOrientation;
        public string objectMarkerSuffix = "_";
        public uint numberOfObjectMarkers = 4;
        public bool visibleMarkers;

        [Range(0.001f, 1f)]
        public float markerScale = 0.012f;

        public Vector3 RelativePosition
        {
            get { return PositionOffset - bodyPosition; }
        }
        public Quaternion RelativeRotation
        {
            get { return Quaternion.Euler(RotationOffset) * Quaternion.Inverse(bodyRotation); }
        }

        protected List<LabeledMarker> markers;
        protected List<GameObject> markerGOs;

        private Vector3 bodyPosition = Vector3.zero;
        private Quaternion bodyRotation = Quaternion.identity;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        public override void applyBody()
        {
            if (useObjectOrientation)
                base.applyBody();

            // Avoid position is NaN
            if (body.Position.magnitude > 0)
            {
                bodyPosition = body.Position;
                bodyRotation = body.Rotation;
            }

            // Get the body markers, of which the 6DOF body consists
            markers = rtClient.Markers;
            if (markers == null || markers.Count == 0)
                return;

            if (markerGOs == null || markerGOs.Count != numberOfObjectMarkers)
            {
                InitiateMarkers();
            }

            foreach (GameObject markerGO in markerGOs)
            {
                LabeledMarker marker = rtClient.GetMarker(markerGO.name);

                // Show and update existing markers
                if (marker != null && marker.Position.magnitude > 0)
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
            if (markerGOs == null) markerGOs = new List<GameObject>();
            foreach (var markerGO in markerGOs)
            {
                Destroy(markerGO);
            }
            markerGOs.Clear();

            markers = rtClient.Markers;
            foreach (LabeledMarker marker in markers)
            {
                if (marker.Label.StartsWith(this.ObjectName + objectMarkerSuffix))
                {
                    GameObject markerGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    markerGO.name = marker.Label;
                    markerGO.transform.parent = this.gameObject.transform;
                    markerGO.transform.localScale = Vector3.one * markerScale;
                    markerGO.SetActive(false);
                    markerGOs.Add(markerGO);
                }
                if (markerGOs.Count >= numberOfObjectMarkers) break;
            }
        }
    }
}
