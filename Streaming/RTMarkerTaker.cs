using UnityEngine;
using System.Collections.Generic;

//Script for marker coordinates transfer to it's parent by marker name.

namespace QualisysRealTime.Unity
{

	class MarkerTaker : MonoBehaviour
	{
	
		public string markerName;

		private List<LabeledMarker> markerData;
		private RTClient rtClient;
		private GameObject markerRoot;
		private Vector3 markerPosition = new Vector3();
		private bool streaming = false;

		// Use this for initialization
		void Start()
		{
			rtClient = RTClient.GetInstance();
			markerRoot = gameObject;
		}

		// Update is called once per frame
		void Update()
		{
			
			if (rtClient == null) rtClient = RTClient.GetInstance();
			if (rtClient.GetStreamingStatus() && !streaming)
			{
				streaming = true;
			}
			if (!rtClient.GetStreamingStatus() && streaming)
			{
				streaming = false;
			}

			markerData = rtClient.Markers;
			if (markerData == null && markerData.Count == 0)
				return;
			for (int i = 0; i < markerData.Count; i++)
			{
					if (markerData[i].Label == markerName)
					{
						markerPosition = markerData[i].Position;
						transform.position = markerPosition;
					}
			}
		}
	}
}
