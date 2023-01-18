using System.Collections.Generic;
using System.Linq;
using QTMRealTimeSDK.Data;
using QTMRealTimeSDK.Settings;
using UnityEngine;

namespace QualisysRealTime.Unity
{

    internal class RTState : ICopyFrom<RTState>
    {
        internal List<SixDOFBody> bodies = new List<SixDOFBody>();
        internal List<LabeledMarker> markers = new List<LabeledMarker>();
        internal List<UnlabeledMarker> unlabeledMarkers = new List<UnlabeledMarker>();
        internal List<Bone> bones = new List<Bone>();
        internal List<GazeVector> gazeVectors = new List<GazeVector>();
        internal List<AnalogChannel> analogChannels = new List<AnalogChannel>();
        internal List<Skeleton> skeletons = new List<Skeleton>();
        internal List<ForceVector> forceVectors = new List<ForceVector>();

        internal List<ComponentType> componentsInStream = new List<ComponentType>();
        
        internal Axis upAxis = Axis.XAxisUpwards;
        internal Quaternion coordinateSystemChange = Quaternion.identity;
        internal RtProtocolVersion rtProtocolVersion = new RtProtocolVersion(0, 0);
        internal int frameNumber = 0;
        internal int frequency = 0;
        internal string errorString = "";
        internal RTConnectionState connectionState = RTConnectionState.Connecting;
        internal bool isStreaming = false;

        static void CopyFromList<T>(List<T> source, List<T> target)
            where T : ICopyFrom<T>, new()
        {
            int c = source.Count;
            while (c > target.Count) { target.Add(new T()); }
            while (c < target.Count) { target.RemoveAt(target.Count - 1); }
            for (int i = 0; i < c; ++i)
            {
                target[i].CopyFrom(source[i]);
            }
        }

        public void CopyFrom(RTState rtState)
        {

            CopyFromList(rtState.markers, this.markers);
            CopyFromList(rtState.unlabeledMarkers, this.unlabeledMarkers);
            CopyFromList(rtState.bodies, this.bodies);
            CopyFromList(rtState.gazeVectors, this.gazeVectors);
            CopyFromList(rtState.analogChannels, this.analogChannels);
            CopyFromList(rtState.bones, this.bones);
            CopyFromList(rtState.skeletons, this.skeletons);
            CopyFromList(rtState.forceVectors, this.forceVectors);

            this.rtProtocolVersion.CopyFrom(rtState.rtProtocolVersion);

            this.upAxis = rtState.upAxis;
            this.coordinateSystemChange = rtState.coordinateSystemChange;
            this.errorString = rtState.errorString;
            this.isStreaming = rtState.isStreaming;
            this.frameNumber = rtState.frameNumber;
            this.frequency = rtState.frequency;
            this.connectionState = rtState.connectionState;
        }

        public SixDOFBody GetBody(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            foreach (var body in bodies)
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

            foreach (var skeleton in skeletons)
            {
                if (skeleton.Name == name)
                {
                    return skeleton;
                }
            }
            return null;

        }

        public LabeledMarker GetMarker(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            foreach (var marker in markers)
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
            foreach (var marker in unlabeledMarkers)
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
            foreach (var analogChannel in analogChannels)
            {
                if (analogChannel.Name == name)
                {
                    return analogChannel;
                }
            }
            return null;
        }

        
        public ForceVector GetForcePlateVector(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            foreach (var force in forceVectors)
            {
                if (force.Name == name)
                {
                    return force;
                }
            }
            return null;
        }

        public List<AnalogChannel> GetAnalogChannels(List<string> names)
        {
            if (analogChannels.Count <= 0)
                return null;

            List<AnalogChannel> result = new List<AnalogChannel>();
            var analogChannelDictionary = analogChannels.ToDictionary(d => d.Name);
            foreach (var channelName in names)
            {
                AnalogChannel analogChannel;
                if (analogChannelDictionary.TryGetValue(channelName, out analogChannel))
                {
                    result.Add(analogChannel);
                }
            }
            if (result.Count == names.Count)
            { 
                return result;
            }

            return null;
        }
    }
}
