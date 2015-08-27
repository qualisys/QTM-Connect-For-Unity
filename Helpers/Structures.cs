// Unity SDK for Qualisys Track Manager. Copyright 2015 Qualisys AB
//
using UnityEngine;

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
}
