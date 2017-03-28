// Unity SDK for Qualisys Track Manager. Copyright 2015 Qualisys AB
//
using UnityEngine;
using System.Collections.Generic;
using QualisysRealTime.Unity.Skeleton;

namespace QualisysRealTime.Unity
{
    // Class for 6DOF with unity data types
    public class SixDOFBody
    {
        public SixDOFBody() { }
        public string Name;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    // Class for labeled markers with unity data types
    public class LabeledMarker
    {
        public LabeledMarker() { }
        public string Label;
        public Vector3 Position;
        public Color Color;
    }

    // Class for user bones
    public class Bone
    {
        public Bone() { }
        public string From;
        public LabeledMarker FromMarker;
        public string To;
        public LabeledMarker ToMarker;
        public Color Color = Color.yellow;
    }

    // Class for gaze vectors
    public class GazeVector
    {
        public GazeVector() { }
        public string Name;
        public Vector3 Position;
        public Vector3 Direction;
    }

	public static class MarkerConverter 
	{
		public static List<Marker> Convert(this List<LabeledMarker> list)
		{
			var newList = new List<Marker>();
			foreach (LabeledMarker lm in list) {
				var m = new Marker();
				m.Label = lm.Label;
				m.Position = lm.Position.Convert();
				newList.Add(m);
			}
			return newList;
		}
	}
}
