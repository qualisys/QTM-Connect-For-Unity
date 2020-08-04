// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using UnityEngine;
using System.Collections.Generic;
using QTMRealTimeSDK.Data;
using System;

namespace QualisysRealTime.Unity
{
    internal interface ICopyFrom<T>
    {
        void CopyFrom(T source);
    }

    // Class for 6DOF with unity data types
    public class SixDOFBody : ICopyFrom<SixDOFBody>
    {
        public SixDOFBody() { }
        public string Name = "";
        public Vector3 Position = Vector3.zero;
        public Quaternion Rotation = Quaternion.identity;
        public Color Color = Color.yellow;
        public void CopyFrom(SixDOFBody source)
        {
            this.Name = source.Name;
            this.Position = source.Position;
            this.Rotation = source.Rotation;
            this.Color = source.Color;
        }
    }

    // Class for labeled markers with unity data types
    public class LabeledMarker : ICopyFrom<LabeledMarker>
    {
        public LabeledMarker() { }
        public string Name = "";
        public Vector3 Position  = Vector3.zero;
        public Color Color = Color.yellow;
        public float Residual = 0f;

        public void CopyFrom(LabeledMarker source)
        {
            this.Name = source.Name;
            this.Position = source.Position;
            this.Residual = source.Residual;
            this.Color = source.Color;
        }
    }

    public class UnlabeledMarker : ICopyFrom<UnlabeledMarker>
    {
        public uint Id = 0;
        public Vector3 Position = Vector3.zero;
        public float Residual = 0f;
        public void CopyFrom(UnlabeledMarker source)
        {
            this.Id = source.Id;
            this.Position = source.Position;
            this.Residual = source.Residual;
        }
    }

    // Class for user bones
    public class Bone : ICopyFrom<Bone>
    {
        public Bone() { }
        public string From = "";
        public LabeledMarker FromMarker = new LabeledMarker();
        public string To = "";
        public LabeledMarker ToMarker = new LabeledMarker();
        public Color Color = Color.yellow;
        public void CopyFrom(Bone source)
        {
            From = source.From;
            FromMarker?.CopyFrom(source.FromMarker);
            To = source.To;
            ToMarker.CopyFrom(source.ToMarker);
            Color = source.Color;
        }
    }

    // Class for gaze vectors
    public class GazeVector : ICopyFrom<GazeVector>
    {
        public GazeVector() { }
        public string Name = "" ;
        public Vector3 Position = Vector3.zero;
        public Vector3 Direction = Vector3.forward;
        public void CopyFrom(GazeVector source)
        {
            Name = source.Name;
            Position = source.Position;
            Direction = source.Direction;
        }
    }

    public class AnalogChannel : ICopyFrom<AnalogChannel>
    {
        public string Name = "";
        public float[] Values = new float[0];
        public AnalogChannel() { }
        public AnalogChannel(AnalogChannel analogChannel)
        {
            Name = analogChannel.Name;
            Values = new float[analogChannel.Values.Length];
            Array.Copy(analogChannel.Values, Values, analogChannel.Values.Length);
        }
        public void CopyFrom(AnalogChannel source)
        {
            Name = source.Name;
            if (Values.Length != source.Values.Length)
            {
                Array.Resize(ref Values, source.Values.Length);
            }
            Array.Copy(source.Values, Values, source.Values.Length);
        }

    }
    public class Segment
    {
        public string Name = "";
        public uint Id = 0;
        public uint ParentId = 0;
        public Vector3 Position = Vector3.zero;
        public Quaternion Rotation = Quaternion.identity;
        public Vector3 TPosition = Vector3.zero;
        public Quaternion TRotation = Quaternion.identity;
    }

    public class Skeleton : ICopyFrom<Skeleton>
    {
        public string Name = "";
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

        public void CopyFrom(Skeleton source) 
        {
            Name = source.Name;
            foreach (var kv in source.Segments)
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
