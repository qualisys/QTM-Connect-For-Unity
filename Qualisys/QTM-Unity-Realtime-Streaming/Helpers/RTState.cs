using System.Collections.Generic;
using System.Linq;
using QTMRealTimeSDK.Data;
using QTMRealTimeSDK.Settings;
using UnityEngine;

namespace QualisysRealTime.Unity
{

    internal class RTState : ICopyFrom<RTState>
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

        internal int mFrameNumber = 0;
        internal int mFrequency = 0;
        internal string mErrorString = "";
        internal ConnectionState mConnectionState = ConnectionState.Connecting;
        internal bool mStreaming = false;

        static void CopyFromList<T>(List<T> source, List<T> target)
            where T : ICopyFrom<T>, new()
        {
            {
                int c = source.Count;
                while (c > target.Count) {target.Add(new T()); }
                while (c < target.Count) {target.RemoveAt(target.Count - 1);}
                for (int i = 0; i < c; ++i)
                {
                    target[i].CopyFrom(source[i]);
                }
            }
        }

        public void CopyFrom(RTState rtState)
        {
            this.mUpAxis = rtState.mUpAxis;
            this.mCoordinateSystemChange = rtState.mCoordinateSystemChange;

            CopyFromList(rtState.mMarkers, mMarkers);
            CopyFromList(rtState.mUnlabeledMarkers, mUnlabeledMarkers);
            CopyFromList(rtState.mBodies, mBodies);
            CopyFromList(rtState.mGazeVectors, mGazeVectors);
            CopyFromList(rtState.mAnalogChannels, mAnalogChannels);
            CopyFromList(rtState.mBones, mBones);
            CopyFromList(rtState.mSkeletons, mSkeletons);

            this.mErrorString = rtState.mErrorString;
            this.mStreaming = rtState.mStreaming;
            this.mFrameNumber = rtState.mFrameNumber;
            this.mFrequency = rtState.mFrequency;
            this.mConnectionState = rtState.mConnectionState;
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
