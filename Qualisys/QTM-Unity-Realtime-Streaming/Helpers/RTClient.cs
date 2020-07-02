// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;
using QTMRealTimeSDK.Settings;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Qualisys.QTM_Unity_Realtime_Streaming.Helpers;
using UnityEngine.Scripting;

namespace QualisysRealTime.Unity
{
    public class RTClient : IDisposable
    {

        RTProtocol mProtocol;
        private static RTClient mInstance;
        private ushort replyPort = (ushort)new System.Random().Next(1333, 1388);

        public List<SixDOFBody> Bodies { 
            get { 
                return rtStreamThread == null 
                    ? new List<SixDOFBody>() 
                    : rtStreamThread.readerThreadState.mBodies; 
            }
        }

        public List<LabeledMarker> Markers { 
            get { 
                return rtStreamThread == null 
                    ? new List<LabeledMarker>() 
                    : rtStreamThread.readerThreadState.mMarkers; 
            }
        }

        public List<UnlabeledMarker> UnlabeledMarkers { 
            get { 
                return rtStreamThread == null 
                    ? new List<UnlabeledMarker>() 
                    : rtStreamThread.readerThreadState.mUnlabeledMarkers; 
            } 
        }

        public List<Bone> Bones { 
            get { 
                return rtStreamThread == null 
                    ? new List<Bone>() 
                    : rtStreamThread.readerThreadState.mBones; 
            } 
        }

        public List<GazeVector> GazeVectors { 
            get { 
                return rtStreamThread == null
                    ? new List<GazeVector>()
                    : rtStreamThread.readerThreadState.mGazeVectors;
            }
        }

        public List<AnalogChannel> AnalogChannels { 
            get { 
                return rtStreamThread == null 
                    ? new List<AnalogChannel>() 
                    : rtStreamThread.readerThreadState.mAnalogChannels; 
            } 
        }

        public List<Skeleton> Skeletons { 
            get { 
                return rtStreamThread == null 
                    ? new List<Skeleton>() 
                    : rtStreamThread.readerThreadState.mSkeletons; 
            } 
        }

        RTStreamThread rtStreamThread = null;
    
        // Get frame number from latest packet
        public int GetFrame()
        {
            if (rtStreamThread == null)
            { 
                return 0;
            }

            return rtStreamThread.readerThreadState.mFrameNumber;
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
            // we register our function "process" as a callback for when protocol receives real time data packets
            // (eventDataCallback is also available to listen to events)
        }



        public static RTClient GetInstance()
        {
            // Singleton method since we only want one instance (one connection to server)
            if (mInstance == null)
            {
                mInstance = new RTClient();
                Application.quitting += mInstance.Dispose;
            }
            RTClientUpdater.AssertExistence();
            return mInstance;
        }

        //Method for objects to call to get data from body
        public SixDOFBody GetBody(string name)
        {
            if (rtStreamThread == null)
            {
                return null;
            }
            return rtStreamThread.readerThreadState.GetBody(name);
        }

        public Skeleton GetSkeleton(string name)
        {
            if (rtStreamThread == null)
            {
                return null;
            }
            return rtStreamThread.readerThreadState.GetSkeleton(name);
        }

        public LabeledMarker GetMarker(string name)
        {
            if (rtStreamThread == null)
            {
                return null;
            }
            return rtStreamThread.readerThreadState.GetMarker(name);
        }

        public UnlabeledMarker GetUnlabeledMarker(uint id)
        {
            if (rtStreamThread == null)
            {
                return null;
            }
            return rtStreamThread.readerThreadState.GetUnlabeledMarker(id);
        }

        public AnalogChannel GetAnalogChannel(string name)
        {
            if (rtStreamThread == null)
            {
                return null;

            }
            return rtStreamThread.readerThreadState.GetAnalogChannel(name);
        }
        public List<AnalogChannel> GetAnalogChannels(List<string> names)
        {
            if (rtStreamThread == null)
            {
                return new List<AnalogChannel>();
            }
            return rtStreamThread.readerThreadState.GetAnalogChannels(names);
        }
        public bool GetStreamingStatus()
        {
            if (rtStreamThread == null) 
            {
                return false;
            }
            return rtStreamThread.readerThreadState.mStreamingStatus;
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
            return rtStreamThread != null && rtStreamThread.IsAlive;
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
        public bool Connect(DiscoveryResponse discoveryResponse, short udpPort, bool stream6d, bool stream3d, bool stream3dNoLabels, bool streamGaze, bool streamAnalog, bool streamSkeleton)
        {
            if (rtStreamThread != null) 
            { 
                rtStreamThread.Dispose();
                rtStreamThread = null;
            }
            rtStreamThread = new RTStreamThread(discoveryResponse.IpAddress, udpPort, StreamRate.RateAllFrames, stream6d, stream3d, stream3dNoLabels, streamGaze, streamAnalog, streamSkeleton);
            return rtStreamThread.Update(Time.frameCount);
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
        public bool Connect(string IpAddress, short udpPort, bool stream6d, bool stream3d, bool stream3dNoLabels, bool streamGaze, bool streamAnalog, bool streamSkeleton)
        {
            if (rtStreamThread != null)
            {
                rtStreamThread.Dispose();
                rtStreamThread = null;
            }
            rtStreamThread = new RTStreamThread(IpAddress, udpPort, StreamRate.RateAllFrames, stream6d, stream3d, stream3dNoLabels, streamGaze, streamAnalog, streamSkeleton);
            return rtStreamThread.Update(Time.frameCount);
        }

        // Get protocol error string
        public string GetErrorString()
        {
            return mProtocol.GetErrorString();
        }

        public void Update() 
        {
            if (rtStreamThread != null && rtStreamThread.IsAlive) 
            {
                if (!rtStreamThread.Update(Time.frameCount)) 
                {
                    Disconnect();
                }
            }
        }

        // Get streaming status of client


        // Disconnect from server
        public void Disconnect()
        {
            if (rtStreamThread != null)
            {
                rtStreamThread.Dispose();
                rtStreamThread = null;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (IsConnected()) 
                    { 
                        Disconnect();
                    }
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Application.quitting -= Dispose;
            GC.SuppressFinalize(this);
            Dispose(true);
        }
        ~RTClient () 
        {
            Dispose(false);
        }
        private bool disposed = false;
    }
}