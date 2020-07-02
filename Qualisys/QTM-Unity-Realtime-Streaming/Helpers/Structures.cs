// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;
using System.Collections.Generic;
using QTMRealTimeSDK.Data;
using System;

namespace QualisysRealTime.Unity
{
    // Class for 6DOF with unity data types
    public class SixDOFBody
    {
        public SixDOFBody() { }
        public string Name;
        public Vector3 Position;
        public Quaternion Rotation;
        public Color Color;
    }

    // Class for labeled markers with unity data types
    public class LabeledMarker
    {
        public LabeledMarker() { }
        public string Name;
        public Vector3 Position;
        public Color Color;
        public float Residual;
    }

    public class UnlabeledMarker
    {
        public uint Id;
        public Vector3 Position;
        public float Residual;
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

    public class AnalogChannel
    {
        public string Name;
        public float[] Values;
        public AnalogChannel() { }
        public AnalogChannel(AnalogChannel analogChannel)
        {
            Name = analogChannel.Name;
            Values = new float[analogChannel.Values.Length];
            Array.Copy(analogChannel.Values, Values, analogChannel.Values.Length);
        }
    }
    public class Segment
    {
        public string Name;
        public uint Id;
        public uint ParentId;
        public Vector3 Position = Vector3.zero;
        public Quaternion Rotation = Quaternion.identity;
        public Vector3 TPosition = Vector3.zero;
        public Quaternion TRotation = Quaternion.identity;
    }

    public class Skeleton
    {
        public string Name;
        public Dictionary<uint, Segment> Segments = new Dictionary<uint, Segment>();
        public Skeleton() { }
        public Skeleton(Skeleton skeleton) 
        {
            Name = skeleton.Name;
            foreach (var kv in skeleton.Segments) {
                var segment = kv.Value;
                var key = kv.Key;
                Segments.Add(key, new Segment() {  
                    Id = segment.Id, 
                    Name = segment.Name, 
                    ParentId = segment.ParentId, 
                    Position = segment.Position, 
                    Rotation = segment.Rotation, 
                    TPosition = segment.TPosition, 
                    TRotation = segment.TRotation
                });
            }

        }

        public void CopyFrom(Skeleton skeleton) 
        {
            Name = skeleton.Name;
            foreach (var kv in skeleton.Segments)
            {
                var segment = kv.Value;
                var key = kv.Key;
                Segment s = null;
                if (Segments.TryGetValue(key, out s))
                {
                    s.Id = segment.Id;
                    s.Name = segment.Name;
                    s.ParentId = segment.ParentId;
                    s.Position = segment.Position;
                    s.Rotation = segment.Rotation;
                    s.TPosition = segment.TPosition;
                    s.TRotation = segment.TRotation;
                }
                else
                {
                    Segments.Add(key, new Segment()
                    {
                        Id = segment.Id,
                        Name = segment.Name,
                        ParentId = segment.ParentId,
                        Position = segment.Position,
                        Rotation = segment.Rotation,
                        TPosition = segment.TPosition,
                        TRotation = segment.TRotation
                    });
                }
            }
        } 
    }
}
