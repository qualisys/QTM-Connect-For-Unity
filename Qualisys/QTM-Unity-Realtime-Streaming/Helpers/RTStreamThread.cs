using System;
using System.Collections.Generic;
using System.Linq;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using QTMRealTimeSDK.Settings;
using UnityEngine;
using System.Threading;

namespace QualisysRealTime.Unity
{


    /// <summary>
    /// A class for handling a Connection to QTM and Real-time stream.
    /// Public methods are safe to use by the Main thread.
    /// Call Update from Unity to get the latest stream data.
    /// If update returns false, Dispose of the object instance and create a new one to retry.
    /// </summary>
    internal class RTStreamThread : IDisposable
    {
        class WriterThreadException : Exception
        {
            internal WriterThreadException(string message) :
                base(message)
            { }
        }

        const int LOWEST_SUPPORTED_UNITY_MAJOR_VERSION = 1;
        const int LOWEST_SUPPORTED_UNITY_MINOR_VERSION = 13;
        RTState writerThreadState { get; } = new RTState();
        public RTState readerThreadState { get; } = new RTState();

        object syncLock = new object();
        Thread writerThread;
        List<ComponentType> componentSelection = new List<ComponentType>();
        StreamRate streamRate;
        short udpPort;
        string IpAddress;
        volatile bool killThread = false;
        bool disposed = false;

        List<QTMRealTimeSDK.Data.Analog> cachedAnalog = new List<Analog>();
        List<QTMRealTimeSDK.Data.Q6DOF> cachedSixDof = new List<Q6DOF>();
        List<QTMRealTimeSDK.Data.Q3D> cachedLabeledMarkers = new List<QTMRealTimeSDK.Data.Q3D>();
        List<QTMRealTimeSDK.Data.Q3D> cachedUnabeledMarkers = new List<QTMRealTimeSDK.Data.Q3D>();
        List<QTMRealTimeSDK.Data.SkeletonData> cachedSkeletons = new List<QTMRealTimeSDK.Data.SkeletonData>();
        List<QTMRealTimeSDK.Data.GazeVector> cachedGazeVectors = new List<QTMRealTimeSDK.Data.GazeVector>();


        public RTStreamThread(string IpAddress, short udpPort, StreamRate streamRate, bool stream6d, bool stream3d, bool stream3dNoLabels, bool streamGaze, bool streamAnalog, bool streamSkeleton)
        {

            this.IpAddress = IpAddress;
            this.streamRate = streamRate;
            this.udpPort = udpPort;

            if (stream6d) componentSelection.Add(ComponentType.Component6d);
            if (stream3d) componentSelection.Add(ComponentType.Component3dResidual);
            if (stream3dNoLabels) componentSelection.Add(ComponentType.Component3dNoLabelsResidual);
            if (streamGaze) componentSelection.Add(ComponentType.ComponentGazeVector);
            if (streamAnalog) componentSelection.Add(ComponentType.ComponentAnalog);
            if (streamSkeleton) componentSelection.Add(ComponentType.ComponentSkeleton);

            killThread = false;
            writerThread = new Thread(WriterThreadFunction);
            writerThread.Name = "RTStreamThread::WriterThreadFunction";
            writerThread.Start();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                killThread = true;
                if (writerThread != null)
                {
                    writerThread.Join(TimeSpan.FromSeconds(1));
                    writerThread = null;
                }
            }

            disposed = true;
        }

        ~RTStreamThread()
        {
            Dispose(false);
        }

        /// <summary>
        /// Returns true as long as the object is in a valid state
        /// </summary>
        /// <returns></returns>
        public bool Update()
        {
            lock (syncLock)
            {
                readerThreadState.CopyFrom(writerThreadState);
            }
            if (readerThreadState.mConnectionState == ConnectionState.Disconnected)
            {
                Debug.Log(readerThreadState.mErrorString);
            }
            return readerThreadState.mConnectionState != ConnectionState.Disconnected;
        }

        void WriterThreadFunction() 
        {
            try
            {
                using (var rtProtocol = new RTProtocol(LOWEST_SUPPORTED_UNITY_MAJOR_VERSION, LOWEST_SUPPORTED_UNITY_MINOR_VERSION))
                {
                    if (!rtProtocol.Connect(IpAddress, udpPort, RTProtocol.Constants.MAJOR_VERSION, RTProtocol.Constants.MINOR_VERSION))
                    {
                        if (!rtProtocol.Connect(IpAddress, udpPort, LOWEST_SUPPORTED_UNITY_MAJOR_VERSION, LOWEST_SUPPORTED_UNITY_MINOR_VERSION))
                        {
                            throw new WriterThreadException($"Error Creating Connection to server {rtProtocol.GetErrorString()}");
                        }
                    }
                    lock (syncLock) 
                    {
                        writerThreadState.mConnectionState = ConnectionState.Connected;
                        
                        if (!UpdateSettings(writerThreadState, rtProtocol, componentSelection))
                        {
                            throw new WriterThreadException($"Failed to update settings {rtProtocol.GetErrorString()}");
                        }

                        if (!StartStreaming(writerThreadState, rtProtocol, streamRate, udpPort))
                        {
                            throw new WriterThreadException($"Failed to start stream {rtProtocol.GetErrorString()}");
                        }
                    }


                    while (true)
                    {
                        if (!rtProtocol.IsConnected())
                        {
                            throw new WriterThreadException($"Connection lost");
                        }

                        if (killThread)
                        {
                            throw new WriterThreadException($"Thread was killed");
                        }

                        PacketType packetType;
                        if (rtProtocol.ReceiveRTPacket(out packetType, 0) <= 0)
                        {
                            continue;
                        }

                        var packet = rtProtocol.Packet;
                        if (packet != null)
                        {
                            if (packetType == PacketType.PacketData)
                            {
                                lock (syncLock)
                                {
                                    Process(writerThreadState, packet);
                                }
                            }
                            else if (packetType == PacketType.PacketEvent)
                            {
                                QTMEvent currentEvent = packet.GetEvent();
                                switch (currentEvent)
                                {
                                    case QTMEvent.EventQTMShuttingDown:
                                    case QTMEvent.EventConnectionClosed:
                                        throw new WriterThreadException($"Qtm closed connection");

                                    case QTMEvent.EventRTFromFileStarted:
                                    case QTMEvent.EventConnected:
                                    case QTMEvent.EventCaptureStarted:
                                    case QTMEvent.EventCalibrationStarted:
                                    case QTMEvent.EventCameraSettingsChanged:
                                        lock (syncLock)
                                        {
                                            // reload settings when we start streaming to get proper settings
                                            if (!UpdateSettings(writerThreadState, rtProtocol, componentSelection))
                                            {
                                                throw new WriterThreadException($"Failed to update settings {rtProtocol.GetErrorString()}");
                                            }
                                            
                                            if (!StartStreaming(writerThreadState, rtProtocol, streamRate, udpPort))
                                            {
                                                throw new WriterThreadException($"Failed to start stream {rtProtocol.GetErrorString()}");
                                            }

                                        }
                                        break;
                                    default: break;
                                }
                            }
                        }
                    }
                }
            }
            catch (WriterThreadException writerThreadException)
            {
                lock (syncLock)
                {
                    writerThreadState.mErrorString = writerThreadException.Message;
                    writerThreadState.mConnectionState = ConnectionState.Disconnected;
                }
            }
            catch (System.Exception e)
            {
                lock (syncLock)
                {
                    
                    writerThreadState.mErrorString = $"Exception {e.GetType().Name}: {e.Message}\n{e.StackTrace.Replace(" at ", "\n at ")}";
                    
                    writerThreadState.mConnectionState = ConnectionState.Disconnected;
                }
            }
        }

        static bool UpdateSettings(RTState rtState, RTProtocol rtProtocol, List<ComponentType> componentSelection)
        {
            if (GetGeneralSettings(rtState, rtProtocol) == false) {
                return false;
            }

            rtState.mActiveComponents = componentSelection.Select(x =>
            {
                switch (x)
                {
                    case ComponentType.Component3dResidual: return Get3DSettings(rtState, rtProtocol) ? x : ComponentType.ComponentNone;
                    case ComponentType.Component3dNoLabelsResidual: return Get3DSettings(rtState, rtProtocol) ? x : ComponentType.ComponentNone;
                    case ComponentType.Component6d: return Get6DOFSettings(rtState, rtProtocol) ? x : ComponentType.ComponentNone;
                    case ComponentType.ComponentGazeVector: return GetGazeVectorSettings(rtState, rtProtocol) ? x : ComponentType.ComponentNone;
                    case ComponentType.ComponentAnalog: return GetAnalogSettings(rtState, rtProtocol) ? x : ComponentType.ComponentNone;
                    case ComponentType.ComponentSkeleton: return GetSkeletonSettings(rtState, rtProtocol) ? x : ComponentType.ComponentNone;
                    default: return ComponentType.ComponentNone;
                };
            })
             .Where(x => x != ComponentType.ComponentNone)
             .ToList();
            return true;
        }

        static bool StartStreaming(RTState state, RTProtocol rtProtocol, StreamRate streamRate, short udpPort)
        {
            if (rtProtocol.StreamFrames(streamRate, -1, state.mActiveComponents, udpPort) == false)
            {
                state.mStreaming = false;
                Debug.LogError($"StreamFrames error: {rtProtocol.GetErrorString()}");
            }
            else 
            { 
                state.mStreaming = true;
            }

            return state.mStreaming;
        }

        static bool GetGazeVectorSettings(RTState state, RTProtocol mProtocol)
        {
            bool getStatus = mProtocol.GetGazeVectorSettings();

            if (getStatus)
            {
                state.mGazeVectors.Clear();
                SettingsGazeVectors settings = mProtocol.GazeVectorSettings;
                foreach (var gazeVector in settings.GazeVectors)
                {
                    var newGazeVector = new GazeVector();
                    newGazeVector.Name = gazeVector.Name;
                    newGazeVector.Position = Vector3.zero;
                    newGazeVector.Direction = Vector3.zero;
                    state.mGazeVectors.Add(newGazeVector);
                }

                return true;
            }
            return false;
        }

        static bool GetAnalogSettings(RTState state, RTProtocol mProtocol)
        {
            bool getStatus = mProtocol.GetAnalogSettings();
            if (getStatus)
            {
                state.mAnalogChannels.Clear();
                var settings = mProtocol.AnalogSettings;
                foreach (var device in settings.Devices)
                {
                    foreach (var channel in device.ChannelInformation)
                    {
                        var analogChannel = new AnalogChannel();
                        analogChannel.Name = channel.Name;
                        analogChannel.Values = new float[0];
                        state.mAnalogChannels.Add(analogChannel);
                    }
                }
                return true;
            }
            return false;
        }

        static bool Get6DOFSettings(RTState state, RTProtocol mProtocol)
        {
            // Get settings and information for streamed bodies
            bool getstatus = mProtocol.Get6dSettings();
            if (getstatus)
            {
                state.mBodies.Clear();
                Settings6D settings = mProtocol.Settings6DOF;
                foreach (Settings6DOF body in settings.Bodies)
                {
                    SixDOFBody newbody = new SixDOFBody();
                    newbody.Name = body.Name;
                    newbody.Color.r = (body.ColorRGB) & 0xFF;
                    newbody.Color.g = (body.ColorRGB >> 8) & 0xFF;
                    newbody.Color.b = (body.ColorRGB >> 16) & 0xFF;
                    newbody.Color /= 255;
                    newbody.Color.a = 1F;
                    newbody.Position = Vector3.zero;
                    newbody.Rotation = Quaternion.identity;
                    state.mBodies.Add(newbody);

                }

                return true;
            }

            return false;
        }

        static bool GetSkeletonSettings(RTState state, RTProtocol mProtocol)
        {
            bool getStatus = mProtocol.GetSkeletonSettings();
            if (!getStatus)
                return false;

            state.mSkeletons.Clear();
            var skeletonSettings = mProtocol.SkeletonSettingsCollection;
            foreach (var settingSkeleton in skeletonSettings.SettingSkeletonList)
            {
                Skeleton skeleton = new Skeleton();
                skeleton.Name = settingSkeleton.Name;
                foreach (var settingSegment in settingSkeleton.SettingSegmentList)
                {
                    var segment = new Segment();
                    segment.Name = settingSegment.Name;
                    segment.Id = settingSegment.Id;
                    segment.ParentId = settingSegment.ParentId;

                    // Set rotation and position to work with unity
                    segment.TPosition = new Vector3(settingSegment.Position.X / 1000, settingSegment.Position.Z / 1000, settingSegment.Position.Y / 1000);
                    segment.TRotation = new Quaternion(settingSegment.Rotation.X, settingSegment.Rotation.Z, settingSegment.Rotation.Y, -settingSegment.Rotation.W);

                    skeleton.Segments.Add(segment.Id, segment);
                }
                state.mSkeletons.Add(skeleton);
            }
            return true;
        }

        static bool GetGeneralSettings(RTState state, RTProtocol mProtocol)
        {
            bool getStatus = mProtocol.GetGeneralSettings();
            if (!getStatus)
                return false;

            state.mFrequency = mProtocol.GeneralSettings.CaptureFrequency;
            return true;
        }


        static bool Get3DSettings(RTState state, RTProtocol mProtocol)
        {
            bool getstatus = mProtocol.Get3dSettings();
            if (getstatus)
            {
                state.mUpAxis = mProtocol.Settings3D.AxisUpwards;

                Rotation.ECoordinateAxes xAxis, yAxis, zAxis;
                Rotation.GetCalibrationAxesOrder(state.mUpAxis, out xAxis, out yAxis, out zAxis);

                state.mCoordinateSystemChange = Rotation.GetAxesOrderRotation(xAxis, yAxis, zAxis);

                // Save marker settings
                state. mMarkers.Clear();
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

                    state.mMarkers.Add(newMarker);
                }

                // Save bone settings
                if (mProtocol.Settings3D.Bones != null)
                {
                    state.mBones.Clear();

                    foreach (var settingsBone in mProtocol.Settings3D.Bones)
                    {
                        Bone bone = new Bone();
                        bone.From = settingsBone.From;
                        bone.FromMarker = state.GetMarker(settingsBone.From);
                        bone.To = settingsBone.To;
                        bone.ToMarker = state.GetMarker(settingsBone.To);
                        bone.Color.r = (settingsBone.Color) & 0xFF;
                        bone.Color.g = (settingsBone.Color >> 8) & 0xFF;
                        bone.Color.b = (settingsBone.Color >> 16) & 0xFF;
                        bone.Color /= 255;
                        bone.Color.a = 1F;
                        state.mBones.Add(bone);
                    }
                }

                return true;
            }
            return false;
        }

        void Process(RTState state, RTPacket packet)
        {
            state.mFrameNumber = packet.Frame;
            packet.Get6DOFData(cachedSixDof);
            for (int i = 0; i < cachedSixDof.Count; i++)
            {
                Vector3 position = new Vector3(cachedSixDof[i].Position.X, cachedSixDof[i].Position.Y, cachedSixDof[i].Position.Z);

                //Set rotation and position to work with unity
                position /= 1000;

                state.mBodies[i].Position = QuaternionHelper.Rotate(state.mCoordinateSystemChange, position);
                state.mBodies[i].Position.z *= -1;

                state.mBodies[i].Rotation = state.mCoordinateSystemChange * QuaternionHelper.FromMatrix(cachedSixDof[i].Matrix);
                state.mBodies[i].Rotation.z *= -1;
                state.mBodies[i].Rotation.w *= -1;

                state.mBodies[i].Rotation *= QuaternionHelper.RotationZ(Mathf.PI * .5f);
                state.mBodies[i].Rotation *= QuaternionHelper.RotationX(-Mathf.PI * .5f);
            }
            

            // Get marker data that is labeled and update values
            packet.Get3DMarkerResidualData(cachedLabeledMarkers);
            for (int i = 0; i < cachedLabeledMarkers.Count; i++)
            {
                Q3D marker = cachedLabeledMarkers[i];
                Vector3 position = new Vector3(marker.Position.X, marker.Position.Y, marker.Position.Z);

                position /= 1000;

                state.mMarkers[i].Position = QuaternionHelper.Rotate(state.mCoordinateSystemChange, position);
                state.mMarkers[i].Position.z *= -1;
                state.mMarkers[i].Residual = cachedLabeledMarkers[i].Residual;
            }


            //// Get unlabeled marker data
            packet.Get3DMarkerNoLabelsResidualData(cachedUnabeledMarkers);
            state.mUnlabeledMarkers.Clear();
            for (int i = 0; i < cachedUnabeledMarkers.Count; i++)
            {
                UnlabeledMarker unlabeledMarker = new UnlabeledMarker();
                Q3D marker = cachedUnabeledMarkers[i];
                Vector3 position = new Vector3(marker.Position.X, marker.Position.Y, marker.Position.Z);

                position /= 1000;

                unlabeledMarker.Position = QuaternionHelper.Rotate(state.mCoordinateSystemChange, position);
                unlabeledMarker.Position.z *= -1;
                unlabeledMarker.Residual = cachedUnabeledMarkers[i].Residual;
                unlabeledMarker.Id = cachedUnabeledMarkers[i].Id;
                state.mUnlabeledMarkers.Add(unlabeledMarker);
            }

            packet.GetGazeVectorData(cachedGazeVectors);
            for (int i = 0; i < cachedGazeVectors.Count; i++)
            {
                QTMRealTimeSDK.Data.GazeVector gazeVector = cachedGazeVectors[i];

                Vector3 position = new Vector3(gazeVector.Position.X, gazeVector.Position.Y, gazeVector.Position.Z);
                position /= 1000;
                state.mGazeVectors[i].Position = QuaternionHelper.Rotate(state.mCoordinateSystemChange, position);
                state.mGazeVectors[i].Position.z *= -1;

                Vector3 direction = new Vector3(gazeVector.Gaze.X, gazeVector.Gaze.Y, gazeVector.Gaze.Z);
                state.mGazeVectors[i].Direction = QuaternionHelper.Rotate(state.mCoordinateSystemChange, direction);
                state.mGazeVectors[i].Direction.z *= -1;
            }
            

            packet.GetAnalogData(cachedAnalog);
            if (cachedAnalog != null)
            {
                int channelIndex = 0;
                foreach (var analogDevice in cachedAnalog)
                {
                    for (int i = 0; i < analogDevice.Channels.Length; i++)
                    {
                        var analogChannel = analogDevice.Channels[i];
                        state.mAnalogChannels[channelIndex].Values = analogChannel.Samples;
                        channelIndex++;
                    }
                }
            }

            packet.GetSkeletonData(cachedSkeletons);
            for (int skeletonIndex = 0; skeletonIndex < cachedSkeletons.Count; skeletonIndex++)
            {
                foreach (var segmentData in cachedSkeletons[skeletonIndex].SegmentDataList)
                {
                    Segment targetSegment;
                    if (!state.mSkeletons[skeletonIndex].Segments.TryGetValue(segmentData.Id, out targetSegment))
                        continue;

                    targetSegment.Position = new Vector3(segmentData.Position.X / 1000, segmentData.Position.Z / 1000, segmentData.Position.Y / 1000);
                    targetSegment.Rotation = new Quaternion(segmentData.Rotation.X, segmentData.Rotation.Z, segmentData.Rotation.Y, -segmentData.Rotation.W);
                    state.mSkeletons[skeletonIndex].Segments[segmentData.Id] = targetSegment;
                }
            }
        }
    }
}
