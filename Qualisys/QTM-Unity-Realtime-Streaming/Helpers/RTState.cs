using System.Collections.Generic;
using System.Linq;
using QTMRealTimeSDK.Data;
using QTMRealTimeSDK.Settings;
using QualisysRealTime.Unity;
using UnityEngine;
using GazeVector = QualisysRealTime.Unity.GazeVector;

namespace Assets.Qualisys.QTM_Unity_Realtime_Streaming.Helpers
{
    internal class RTState 
    {
        internal List<SixDOFBody> mBodies = new List<SixDOFBody>();
        internal List<LabeledMarker> mMarkers = new List<LabeledMarker>();
        internal List<UnlabeledMarker> mUnlabeledMarkers = new List<UnlabeledMarker>();
        internal List<Bone> mBones = new List<Bone>();
        internal List<GazeVector> mGazeVectors = new List<GazeVector>();
        internal List<AnalogChannel> mAnalogChannels = new List<AnalogChannel>();
        internal List<Skeleton> mSkeletons = new List<Skeleton>();
        internal List<ComponentType> mActiveComponents = new List<ComponentType>();
        internal Axis mUpAxis = Axis.XAxisUpwards;
        internal Quaternion mCoordinateSystemChange = Quaternion.identity;

        internal bool mStreamingStatus = false;
        internal int mFrameNumber = 0;
        internal int mFrequency = 0;
        internal string mErrorString = "";
        internal bool mThreadIsAlive = true;
        internal void CopyFrom(RTState rtState)
        {
            this.mUpAxis = rtState.mUpAxis;
            this.mCoordinateSystemChange = rtState.mCoordinateSystemChange;
            this.mStreamingStatus = rtState.mStreamingStatus;

            this.mMarkers = rtState.mMarkers.Select(x => new LabeledMarker() { Color = x.Color, Name = x.Name, Position = x.Position, Residual = x.Residual }).ToList();
            this.mUnlabeledMarkers = rtState.mUnlabeledMarkers.Select(x => new UnlabeledMarker() { Position = x.Position, Residual = x.Residual, Id = x.Id }).ToList();
            this.mBodies = rtState.mBodies.Select(x => new SixDOFBody() { Color = x.Color, Name = x.Name, Position = x.Position, Rotation = x.Rotation }).ToList();
            this.mGazeVectors = rtState.mGazeVectors.Select(x => new GazeVector() { Name = x.Name, Direction = x.Direction, Position = x.Position }).ToList();
            this.mAnalogChannels = rtState.mAnalogChannels.Select(x => new AnalogChannel(x)).ToList();
            
            //References GetMarker which needs markers to be loaded first.
            this.mBones = rtState.mBones.Select(x => new Bone() { Color = x.Color, From = x.From, To = x.To, FromMarker = GetMarker(x.From), ToMarker = GetMarker(x.To) }).ToList();
            this.mErrorString = rtState.mErrorString;
            this.mSkeletons = rtState.mSkeletons.Select(x => {
                var index = this.mSkeletons.FindIndex(y => y.Name == x.Name);
                if (index == -1)
                {
                    return new Skeleton(x);
                }
                else 
                {
                    this.mSkeletons[index].CopyFrom(x);
                    return this.mSkeletons[index];
                }
            }).ToList();

            this.mFrameNumber = rtState.mFrameNumber;
            this.mFrequency = rtState.mFrequency;
            this.mThreadIsAlive = rtState.mThreadIsAlive;

        }

        public SixDOFBody GetBody(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            foreach (var body in mBodies)
            {
                if (body.Name == name)
                {
                    return body;
                }
            }
            return null;
        }

        public Skeleton GetSkeleton(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            foreach (var skeleton in mSkeletons)
            {
                if (skeleton.Name == name)
                {
                    return skeleton;
                }
            }
            return null;

        }

        // Get marker data from streamed data
        public LabeledMarker GetMarker(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            foreach (var marker in mMarkers)
            {
                if (marker.Name == name)
                {
                    return marker;
                }
            }
            return null;
        }

        public UnlabeledMarker GetUnlabeledMarker(uint id)
        {
            foreach (var marker in mUnlabeledMarkers)
            {
                if (marker.Id == id)
                {
                    return marker;
                }
            }
            return null;
        }

        public AnalogChannel GetAnalogChannel(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            foreach (var analogChannel in mAnalogChannels)
            {
                if (analogChannel.Name == name)
                {
                    return analogChannel;
                }
            }
            return null;
        }

        public List<AnalogChannel> GetAnalogChannels(List<string> names)
        {
            if (mAnalogChannels.Count <= 0)
                return null;

            List<AnalogChannel> analogChannels = new List<AnalogChannel>();
            var analogChannelDictionary = mAnalogChannels.ToDictionary(d => d.Name);
            foreach (var channelName in names)
            {
                AnalogChannel analogChannel;
                if (analogChannelDictionary.TryGetValue(channelName, out analogChannel))
                {
                    analogChannels.Add(analogChannel);
                }
            }
            if (analogChannels.Count == names.Count)
            { 
                return analogChannels;
            }

            return null;
        }
    }
}
