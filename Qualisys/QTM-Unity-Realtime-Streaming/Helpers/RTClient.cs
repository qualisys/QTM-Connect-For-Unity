// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using QTMRealTimeSDK;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

namespace QualisysRealTime.Unity
{
    public class RTClient : IDisposable
    {
        private static RTClient mInstance;
        private int previousFrame = -1;
        private ushort replyPort = (ushort)new System.Random().Next(1333, 1388);
        private bool disposed = false;
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

        public ConnectionState ConnectionState
        {
            get {
                return rtStreamThread == null
                    ? ConnectionState.Disconnected
                    : rtStreamThread.readerThreadState.mConnectionState;
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
            if (rtStreamThread == null)
            {
                return 0;
            }
            return rtStreamThread.readerThreadState.mFrequency;
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
            return ConnectionState == ConnectionState.Connected && rtStreamThread.readerThreadState.mStreaming;
        }
        /// <summary>
        /// Get list of servers available on network (always add localhost)
        /// </summary>
        /// <returns><c>true</c>, if discovery packet was sent, <c>false</c> otherwise.</returns>
        /// <param name="list">List of discovered servers</param>
        public List<DiscoveryResponse> GetServers()
        {
            // Send discovery packet
            using (var protocol = new RTProtocol())
            {
                List<DiscoveryResponse> list = new List<DiscoveryResponse>();
                if (protocol.DiscoverRTServers(replyPort))
                {
                    if (protocol.DiscoveryResponses.Count > 0)
                    {
                        //Get list of all servers from protocol
                        foreach (var discoveryResponse in protocol.DiscoveryResponses)
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
        }

        [Obsolete("IsConnected is deprecated, use ConnectionState property instead.")]
        public bool IsConnected()
        {
            return ConnectionState != ConnectionState.Disconnected;
        }

        /// <summary>
        /// Connect to QTM and start streaming
        /// This method is non blocking
        /// </summary>
        /// <param name="pickedServer">Picked server.</param>
        /// <param name="udpPort">UDP port streaming should occur on.</param>
        /// <param name="stream6d">if 6DOF data should be streamed.</param>
        /// <param name="stream3d">if labeled markers should be streamed.</param>
        /// <param name="stream3dNoLabels">if unlabeled markers should be streamed.</param>
        /// <param name="streamGaze">if gaze vectors should be streamed.</param>
        /// <param name="streamAnalog">if analog data should be streamed.</param>
        public void StartConnecting(string IpAddress, short udpPort, bool stream6d, bool stream3d, bool stream3dNoLabels, bool streamGaze, bool streamAnalog, bool streamSkeleton)
        {
            if (rtStreamThread != null)
            {
                rtStreamThread.Dispose();
                rtStreamThread = null;
            }
            rtStreamThread = new RTStreamThread(IpAddress, udpPort, StreamRate.RateAllFrames, stream6d, stream3d, stream3dNoLabels, streamGaze, streamAnalog, streamSkeleton);
        }

        /// <summary>
        /// Connect to QTM and start streaming
        /// This method blocks the calling thread.
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
            StartConnecting(discoveryResponse.IpAddress, udpPort, stream6d, stream3d, stream3dNoLabels, streamGaze, streamAnalog, streamSkeleton);
            while (ConnectionState == ConnectionState.Connecting)
            {
                if (!rtStreamThread.Update())
                {
                    Disconnect();
                }
                else 
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(200));
                }
            }
            return ConnectionState == ConnectionState.Connected;
        }

        /// <summary>
        /// Connect to QTM and start streaming
        /// This method blocks the calling thread.
        /// </summary>
        /// <param name="ipAddress">IP address of the QTM host</param>
        /// <param name="udpPort">UDP port streaming should occur on.</param>
        /// <param name="stream6d">if 6DOF data should be streamed.</param>
        /// <param name="stream3d">if labeled markers should be streamed.</param>
        /// <param name="stream3d">if unlabeled markers should be streamed.</param>
        /// <param name="streamGaze">if gaze vectors should be streamed.</param>
        /// <param name="streamAnalog">if analog data should be streamed.</param>
        public bool Connect(string ipAddress, short udpPort, bool stream6d, bool stream3d, bool stream3dNoLabels, bool streamGaze, bool streamAnalog, bool streamSkeleton)
        {
            StartConnecting(ipAddress, udpPort, stream6d, stream3d, stream3dNoLabels, streamGaze, streamAnalog, streamSkeleton);
            while (ConnectionState == ConnectionState.Connecting)
            {
                if (!rtStreamThread.Update())
                {
                    Disconnect();
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(200));
                }
            }
            return ConnectionState == ConnectionState.Connected;
        }

        /// <summary>
        /// Async method for connecting to QTM and start streaming
        /// </summary>
        /// <param name="ipAddress">IP address of the QTM host</param>
        /// <param name="udpPort">UDP port streaming should occur on. -1 if TCP should be used</param>
        /// <param name="stream6d">if 6DOF data should be streamed.</param>
        /// <param name="stream3d">if labeled markers should be streamed.</param>
        /// <param name="stream3dNoLabels">if unlabeled markers should be streamed.</param>
        /// <param name="streamGaze">if gaze vectors should be streamed.</param>
        /// <param name="streamAnalog">if analog data should be streamed.</param>
        public async Task<bool> ConnectAsync(string ipAddress, short udpPort, bool stream6d, bool stream3d, bool stream3dNoLabels, bool streamGaze, bool streamAnalog, bool streamSkeleton) 
        {
            StartConnecting(ipAddress, udpPort, stream6d, stream3d, stream3dNoLabels, streamGaze, streamAnalog, streamSkeleton);
            while (ConnectionState == ConnectionState.Connecting) 
            {
                //Relies on RTClientUpdater for updates.
                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }
            return ConnectionState == ConnectionState.Connected;
        }

        public string GetErrorString()
        {
            if (rtStreamThread == null)
                return "";

            return rtStreamThread.readerThreadState.mErrorString;
        }

        public void Update() 
        {
            int frameNumber = Time.frameCount;
            if (previousFrame == frameNumber)
            {
                return;
            }
            else 
            {
                previousFrame = frameNumber;
            }

            if (ConnectionState != ConnectionState.Disconnected)
            {
                if (!rtStreamThread.Update()) 
                {
                    Disconnect();
                }
            }
        }

        public void Disconnect()
        {
            if (rtStreamThread != null)
            {
                rtStreamThread.Dispose();
                rtStreamThread = null;
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                Application.quitting -= Dispose;
                if (rtStreamThread != null)
                {
                    rtStreamThread.Dispose();
                    rtStreamThread = null;
                }
                disposed = true;
            }
        }
        
    }
}
