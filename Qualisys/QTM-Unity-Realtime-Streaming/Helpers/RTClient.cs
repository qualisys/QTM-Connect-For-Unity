// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using QTMRealTimeSDK.Settings;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace QualisysRealTime.Unity
{
    public class RTClient : IDisposable
    {
        const int LOWEST_SUPPORTED_UNITY_MAJOR_VERSION = 1;
        const int LOWEST_SUPPORTED_UNITY_MINOR_VERSION = 13;

        RTProtocol mProtocol;
        private static RTClient mInstance;
        private ushort replyPort = (ushort)new System.Random().Next(1333, 1388);

        private List<SixDOFBody> mBodies;
        public List<SixDOFBody> Bodies { get { return mBodies; } }

        private List<LabeledMarker> mMarkers;
        public List<LabeledMarker> Markers { get { return mMarkers; } }

        private List<UnlabeledMarker> mUnlabeledMarkers;
        public List<UnlabeledMarker> UnlabeledMarkers { get { return mUnlabeledMarkers; } }

        private List<Bone> mBones;
        public List<Bone> Bones { get { return mBones; } }

        private List<GazeVector> mGazeVectors;
        public List<GazeVector> GazeVectors { get { return mGazeVectors; } }

        private List<AnalogChannel> mAnalogChannels;
        public List<AnalogChannel> AnalogChannels { get { return mAnalogChannels; } }
        

        private Axis mUpAxis;
        private Quaternion mCoordinateSystemChange;
        private RTPacket mPacket;
        private bool mStreamingStatus;


        // processor of realtime data
        // Function is called every time protocol receives a datapacket from server
        public void Process(RTPacket packet)
        {
            mPacket = packet;

            var bodyData = packet.Get6DOFData();
            var labeledMarkerData = packet.Get3DMarkerResidualData();
            var unlabeledMarkerData = packet.Get3DMarkerNoLabelsResidualData();
            var gazeVectorData = packet.GetGazeVectorData();
            var analogData = packet.GetAnalogData();

            if (bodyData != null)
            {
                for (int i = 0; i < bodyData.Count; i++)
                {
                    Vector3 position = new Vector3(bodyData[i].Position.X, bodyData[i].Position.Y, bodyData[i].Position.Z);

                    //Set rotation and position to work with unity
                    position /= 1000;

                    mBodies[i].Position = QuaternionHelper.Rotate(mCoordinateSystemChange, position);
                    mBodies[i].Position.z *= -1;

                    mBodies[i].Rotation = mCoordinateSystemChange * QuaternionHelper.FromMatrix(bodyData[i].Matrix);
                    mBodies[i].Rotation.z *= -1;
                    mBodies[i].Rotation.w *= -1;

                    mBodies[i].Rotation *= QuaternionHelper.RotationZ(Mathf.PI * .5f);
                    mBodies[i].Rotation *= QuaternionHelper.RotationX(-Mathf.PI * .5f);

                }
            }

            // Get marker data that is labeled and update values
            if (labeledMarkerData != null)
            {
                for (int i = 0; i < labeledMarkerData.Count; i++)
                {
                    Q3D marker = labeledMarkerData[i];
                    Vector3 position = new Vector3(marker.Position.X, marker.Position.Y, marker.Position.Z);

                    position /= 1000;

                    mMarkers[i].Position = QuaternionHelper.Rotate(mCoordinateSystemChange, position);
                    mMarkers[i].Position.z *= -1;
                    mMarkers[i].Residual = labeledMarkerData[i].Residual;
                }
            }

            // Get unlabeled marker data
            if (unlabeledMarkerData != null)
            {
                mUnlabeledMarkers.Clear();
                for (int i = 0; i < unlabeledMarkerData.Count; i++)
                {
                    UnlabeledMarker unlabeledMarker = new UnlabeledMarker();
                    Q3D marker = unlabeledMarkerData[i];
                    Vector3 position = new Vector3(marker.Position.X, marker.Position.Y, marker.Position.Z);

                    position /= 1000;

                    unlabeledMarker.Position = QuaternionHelper.Rotate(mCoordinateSystemChange, position);
                    unlabeledMarker.Position.z *= -1;
                    unlabeledMarker.Residual = unlabeledMarkerData[i].Residual;
                    unlabeledMarker.Id = unlabeledMarkerData[i].Id;
                    mUnlabeledMarkers.Add(unlabeledMarker);
                }
            }

            if (gazeVectorData != null)
            {
                for (int i = 0; i < gazeVectorData.Count; i++)
                {
                    QTMRealTimeSDK.Data.GazeVector gazeVector = gazeVectorData[i];

                    Vector3 position = new Vector3(gazeVector.Position.X, gazeVector.Position.Y, gazeVector.Position.Z);
                    position /= 1000;
                    mGazeVectors[i].Position = QuaternionHelper.Rotate(mCoordinateSystemChange, position);
                    mGazeVectors[i].Position.z *= -1;

                    Vector3 direction = new Vector3(gazeVector.Gaze.X, gazeVector.Gaze.Y, gazeVector.Gaze.Z);
                    mGazeVectors[i].Direction = QuaternionHelper.Rotate(mCoordinateSystemChange, direction);
                    mGazeVectors[i].Direction.z *= -1;

                }
            }

            if (analogData != null)
            {
                int channelIndex = 0;
                foreach (var analogDevice in analogData)
                {
                    for (int i = 0; i < analogDevice.Channels.Length; i++)
                    {
                        var analogChannel = analogDevice.Channels[i];
                        mAnalogChannels[channelIndex].Values = analogChannel.Samples;
                        channelIndex++;
                    }
                }
            }
        }

        // called every time a event is broadcasted from QTM server.
        public void Events(RTPacket packet)
        {
            QTMEvent currentEvent = packet.GetEvent();
            Debug.Log("Event occurred! : " + currentEvent);

            if (currentEvent == QTMEvent.EventRTFromFileStarted ||
                currentEvent == QTMEvent.EventConnected ||
                currentEvent == QTMEvent.EventCaptureStarted ||
                currentEvent == QTMEvent.EventCalibrationStarted)
            {
                // reload settings when we start streaming to get proper settings
                Debug.Log("Reloading settings from QTM");

                Get3DSettings();
                Get6DOFSettings();
                GetGazeVectorSettings();
                GetAnalogSettings();
            }
        }

        // Get frame number from latest packet
        public int GetFrame()
        {
            return mPacket.Frame;
        }

        public int GetFrequency()
        {
            if (mProtocol.GeneralSettings == null)
            {
                mProtocol.GetGeneralSettings();
            }
            return mProtocol.GeneralSettings.CaptureFrequency;
        }

        private RTClient()
        {
            // New instance of protocol, contains a RT packet
            mProtocol = new RTProtocol();
            mBodies = new List<SixDOFBody>();
            mMarkers = new List<LabeledMarker>();
            mUnlabeledMarkers = new List<UnlabeledMarker>();
            mBones = new List<Bone>();
            mGazeVectors = new List<GazeVector>();
            mAnalogChannels = new List<AnalogChannel>();

            mStreamingStatus = false;
            mPacket = RTPacket.ErrorPacket;
        }

        ~RTClient()
        {
            Dispose(false);
        }

        public static RTClient GetInstance()
        {
            // Singleton method since we only want one instance (one connection to server)
            if (mInstance == null)
            {
                mInstance = new RTClient();
            }
            return mInstance;
        }

        //Method for objects to call to get data from body
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
        // Get analog channel data from streamed data
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
                return analogChannels;
            return null;
        }

        /// <summary>
        /// Get list of servers available on network (always add localhost)
        /// </summary>
        /// <returns><c>true</c>, if discovery packet was sent, <c>false</c> otherwise.</returns>
        /// <param name="list">List of discovered servers</param>
        public List<DiscoveryResponse> GetServers()
        {
            // Send discovery packet
            List<DiscoveryResponse> list = new List<DiscoveryResponse>();
            if (mProtocol.DiscoverRTServers(replyPort))
            {
                if (mProtocol.DiscoveryResponses.Count > 0)
                {
                    //Get list of all servers from protocol
                    foreach (var discoveryResponse in mProtocol.DiscoveryResponses)
                    {
                        //add them to our list for user to pick from
                        list.Add(discoveryResponse);
                    }
                }
            }
            list.Add(new DiscoveryResponse
            {
                HostName = "Localhost",
                IpAddress = "127.0.0.1",
                Port = RTProtocol.Constants.STANDARD_BASE_PORT,
                InfoText = "",
                CameraCount = 0
            });
            return list;
        }

        public bool IsConnected()
        {
            return mProtocol.IsConnected();
        }

        /// <summary>
        /// Connect the specified pickedServer.
        /// </summary>
        /// <param name="pickedServer">Picked server.</param>
        /// <param name="udpPort">UDP port streaming should occur on.</param>
        /// <param name="stream6d">if 6DOF data should be streamed.</param>
        /// <param name="stream3d">if labeled markers should be streamed.</param>
        /// <param name="stream3dNoLabels">if unlabeled markers should be streamed.</param>
        /// <param name="streamGaze">if gaze vectors should be streamed.</param>
        /// <param name="streamAnalog">if analog data should be streamed.</param>
        public bool Connect(DiscoveryResponse discoveryResponse, short udpPort, bool stream6d, bool stream3d, bool stream3dNoLabels, bool streamGaze, bool streamAnalog)
        {
            if (!mProtocol.Connect(discoveryResponse, udpPort, RTProtocol.Constants.MAJOR_VERSION, RTProtocol.Constants.MINOR_VERSION))
            {
                if (!mProtocol.Connect(discoveryResponse, udpPort, LOWEST_SUPPORTED_UNITY_MAJOR_VERSION, LOWEST_SUPPORTED_UNITY_MINOR_VERSION))
                {
                    Debug.Log("Error Creating Connection to server");
                    return false;
                }
            }
            return ConnectStream(udpPort, StreamRate.RateAllFrames, stream6d, stream3d, stream3dNoLabels, streamGaze, streamAnalog);
        }

        /// <summary>
        /// Connect the specified IpAddress.
        /// </summary>
        /// <param name="IpAddress">IP adress</param>
        /// <param name="udpPort">UDP port streaming should occur on.</param>
        /// <param name="stream6d">if 6DOF data should be streamed.</param>
        /// <param name="stream3d">if labeled markers should be streamed.</param>
        /// <param name="stream3d">if unlabeled markers should be streamed.</param>
        /// <param name="streamGaze">if gaze vectors should be streamed.</param>
        /// <param name="streamAnalog">if analog data should be streamed.</param>
        public bool Connect(string IpAddress, short udpPort, bool stream6d, bool stream3d, bool stream3dNoLabels, bool streamGaze, bool streamAnalog)
        {
            if (mProtocol.Connect(IpAddress, udpPort))
            {
                return ConnectStream(udpPort, StreamRate.RateAllFrames, stream6d, stream3d, stream3dNoLabels, streamGaze, streamAnalog);
            }
            Debug.Log("Error Creating Connection to server");
            return false;
        }

        // Get protocol error string
        public string GetErrorString()
        {
            return mProtocol.GetErrorString();
        }

        // Get streaming status of client
        public bool GetStreamingStatus()
        {
            return mStreamingStatus;
        }

        // Disconnect from server
        public void Disconnect()
        {
            mBodies.Clear();
            mMarkers.Clear();
            mUnlabeledMarkers.Clear();
            mBones.Clear();
            mGazeVectors.Clear();
            mAnalogChannels.Clear();
            mStreamingStatus = false;
            mProtocol.RealTimeDataCallback -= Process;
            mProtocol.EventDataCallback -= Events;
            mProtocol.StreamFramesStop();
            mProtocol.StopStreamListen();
            mProtocol.Disconnect();
        }

        private bool GetGazeVectorSettings()
        {
            bool getStatus = mProtocol.GetGazeVectorSettings();

            if (getStatus)
            {
                mGazeVectors.Clear();
                SettingsGazeVectors settings = mProtocol.GazeVectorSettings;
                foreach (var gazeVector in settings.GazeVectors)
                {
                    var newGazeVector = new GazeVector();
                    newGazeVector.Name = gazeVector.Name;
                    newGazeVector.Position = Vector3.zero;
                    newGazeVector.Direction = Vector3.zero;
                    mGazeVectors.Add(newGazeVector);
                }

                return true;
            }
            return false;
        }

        private bool GetAnalogSettings()
        {
            bool getStatus = mProtocol.GetAnalogSettings();
            if (getStatus)
            {
                mAnalogChannels.Clear();
                var settings = mProtocol.AnalogSettings;
                foreach (var device in settings.Devices)
                {
                    foreach (var channel in device.ChannelInformation)
                    {
                        var analogChannel = new AnalogChannel();
                        analogChannel.Name = channel.Name;
                        analogChannel.Values = new float[0];
                        mAnalogChannels.Add(analogChannel);
                    }
                }
                return true;
            }
            return false;
        }

        private bool Get6DOFSettings()
        {
            // Get settings and information for streamed bodies
            bool getstatus = mProtocol.Get6dSettings();

            if (getstatus)
            {
                mBodies.Clear();
                Settings6D settings = mProtocol.Settings6DOF;
                foreach (Settings6DOF body in settings.Bodies)
                {
                    SixDOFBody newbody = new SixDOFBody();
                    newbody.Name = body.Name;
                    newbody.Position = Vector3.zero;
                    newbody.Rotation = Quaternion.identity;
                    mBodies.Add(newbody);

                }

                return true;
            }

            return false;
        }

        private bool Get3DSettings()
        {
            bool getstatus = mProtocol.Get3dSettings();
            if (getstatus)
            {
                mUpAxis = mProtocol.Settings3D.AxisUpwards;

                Rotation.ECoordinateAxes xAxis, yAxis, zAxis;
                Rotation.GetCalibrationAxesOrder(mUpAxis, out xAxis, out yAxis, out zAxis);

                mCoordinateSystemChange = Rotation.GetAxesOrderRotation(xAxis, yAxis, zAxis);

                // Save marker settings
                mMarkers.Clear();
                foreach (Settings3DLabel marker in mProtocol.Settings3D.Labels)
                {
                    LabeledMarker newMarker = new LabeledMarker();
                    newMarker.Name = marker.Name;
                    newMarker.Position = Vector3.zero;
                    newMarker.Residual = 0;
                    newMarker.Color.r = (marker.ColorRGB) & 0xFF;
                    newMarker.Color.g = (marker.ColorRGB >> 8) & 0xFF;
                    newMarker.Color.b = (marker.ColorRGB >> 16) & 0xFF;
                    newMarker.Color /= 255;
                    newMarker.Color.a = 1F;

                    mMarkers.Add(newMarker);
                }

                // Save bone settings
                if (mProtocol.Settings3D.Bones != null)
                {
                    Bones.Clear();

                    //Save bone settings
                    foreach (var settingsBone in mProtocol.Settings3D.Bones)
                    {
                        Bone bone = new Bone();
                        bone.From = settingsBone.From;
                        bone.FromMarker = GetMarker(settingsBone.From);
                        bone.To = settingsBone.To;
                        bone.ToMarker = GetMarker(settingsBone.To);
                        bone.Color.r = (settingsBone.Color) & 0xFF;
                        bone.Color.g = (settingsBone.Color >> 8) & 0xFF;
                        bone.Color.b = (settingsBone.Color >> 16) & 0xFF;
                        bone.Color /= 255;
                        bone.Color.a = 1F;
                        mBones.Add(bone);
                    }
                }

                return true;
            }
            return false;
        }

        public bool ConnectStream(short udpPort, StreamRate streamRate, bool stream6d, bool stream3d, bool stream3dNoLabels, bool streamGaze, bool streamAnalog)
        {
            List<ComponentType> streamedTypes = new List<ComponentType>();
            if (stream3d)
                streamedTypes.Add(ComponentType.Component3dResidual);
            if (stream3dNoLabels)
                streamedTypes.Add(ComponentType.Component3dNoLabelsResidual);
            if (stream6d)
                streamedTypes.Add(ComponentType.Component6d);
            if (streamGaze)
                streamedTypes.Add(ComponentType.ComponentGazeVector);
            if (streamAnalog)
                streamedTypes.Add(ComponentType.ComponentAnalog);

            if (!mProtocol.GetGeneralSettings())
            {
                Debug.Log("Error retrieving general QTM streaming settings");
                return false;
            }

            if (stream3d)
            {
                if (!Get3DSettings())
                {
                    Debug.Log("Error retrieving 3d settings from stream");
                    return false;
                }
            }

            if (stream6d)
            {
                if (!Get6DOFSettings())
                {
                    Debug.Log("Error retrieving 6dof settings from stream");
                    return false;
                }
            }

            if (streamGaze)
            {
                if (!GetGazeVectorSettings())
                {
                    // Don't fail too hard since gaze only has been available for a short while... but still give an error in the log.
                    Debug.Log("Error retrieving gaze settings from stream");
                }
            }

            if (streamAnalog)
            {
                if (!GetAnalogSettings())
                {
                    // Don't fail too hard since gaze only has been available for a short while... but still give an error in the log.
                    Debug.Log("Error retrieving analog settings from stream");
                }
            }

            // we register our function "process" as a callback for when protocol receives real time data packets
            // (eventDataCallback is also available to listen to events)
            mProtocol.RealTimeDataCallback += Process;
            mProtocol.EventDataCallback += Events;

            //Start streaming and get the settings
            if (mProtocol.StreamFrames(streamRate, -1, streamedTypes, udpPort))
            {
                //Tell protocol to start listening to real time data
                mProtocol.ListenToStream();
                mStreamingStatus = true;
                return true;
            }
            else
            {
                Debug.Log("Error creating connection to server");
            }
            return false;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    Disconnect();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;
    }
}
