// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using QTMRealTimeSDK;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEditor.MemoryProfiler;

namespace QualisysRealTime.Unity
{
    public class RTClient : IDisposable
    {
        static RtProtocolVersion rtProtocolVersionMax = new RtProtocolVersion(RTProtocol.Constants.MAJOR_VERSION, RTProtocol.Constants.MINOR_VERSION);
        static RTClient instance;
        int previousFrame = -1;
        ushort replyPort = (ushort)new System.Random().Next(1333, 1388);
        bool disposed = false;

        RTStreamThread rtStreamThread = null;
        string errorString = string.Empty;
        QtmCommandHandler.QtmCommandAwaiter qtmCommandAwaiter = new QtmCommandHandler.QtmCommandAwaiter();
        Queue<QtmCommandHandler.ICommand> qtmCommands = new Queue<QtmCommandHandler.ICommand>();
        RTState networkState = new RTState();
        public event Action<QTMRealTimeSDK.Data.QTMEvent> onNetworkEvent;

        public List<SixDOFBody> Bodies { 
            get { 
                return networkState.bodies; 
            }
        }

        public List<LabeledMarker> Markers { 
            get { 
                return networkState.markers; 
            }
        }

        public List<UnlabeledMarker> UnlabeledMarkers { 
            get { 
                return networkState.unlabeledMarkers; 
            } 
        }

        public List<Bone> Bones { 
            get { 
                return networkState.bones; 
            } 
        }

        public List<GazeVector> GazeVectors { 
            get { 
                return  networkState.gazeVectors;
            }
        }
        
        public RtProtocolVersion RtProtocolVersion 
        {
            get{ return networkState.rtProtocolVersion; }
        }
        
        public RtProtocolVersion RtProtocolVersionMax
        {
            get{ return rtProtocolVersionMax; }
        }


        public List<AnalogChannel> AnalogChannels { 
            get { 
                return networkState.analogChannels; 
            } 
        }

        public List<Skeleton> Skeletons { 
            get { 
                return networkState.skeletons; 
            } 
        }

        public RTConnectionState ConnectionState
        {
            get {
                return rtStreamThread == null 
                    ? RTConnectionState.Disconnected : networkState.connectionState;
            }
        }

        public string CurrentCommand
        {
            get
            {
                return qtmCommandAwaiter.CurrentCommand;
            }
        }

        public void SendSaveFile(string path, Action<QtmCommandResult> onResult)
        {
            qtmCommands.Enqueue(new QtmCommandHandler.Save(path, onResult));
        }

        public void SendCloseFile(Action<QtmCommandResult> onResult)
        {
            qtmCommands.Enqueue(new QtmCommandHandler.Close(onResult));
        }

        public void SendNewMeasurement(Action<QtmCommandResult> onResult)
        {
            qtmCommands.Enqueue(new QtmCommandHandler.New(onResult));
        }

        public void SendStartCapture(Action<QtmCommandResult> onResult)
        {
            qtmCommands.Enqueue(new QtmCommandHandler.StartCapture(onResult));
        }
        public void SendTakeControl(string password, Action<QtmCommandResult> onResult)
        {
            qtmCommands.Enqueue(new QtmCommandHandler.TakeControl(password, onResult));
        }
        public void SendReleaseControl(Action<QtmCommandResult> onResult)
        {
            qtmCommands.Enqueue(new QtmCommandHandler.ReleaseControl(onResult));
        }
        public void SendStop(Action<QtmCommandResult> onResult)
        {
            qtmCommands.Enqueue(new QtmCommandHandler.Stop(onResult));
        }

        public void SendStartRtFromFile(Action<QtmCommandResult> onResult)
        {
            qtmCommands.Enqueue(new QtmCommandHandler.StartRtFromFile(onResult));
        }

        public void CancelAllCommands()
        {
            if (qtmCommandAwaiter.IsAwaiting)
            { 
                qtmCommandAwaiter.CancelAwait();
            }
            qtmCommands.Clear();
        }

        // Get frame number from latest packet
        public int GetFrame()
        {
            return networkState.frameNumber;
        }

        public int GetFrequency()
        {
            return networkState.frequency;
        }

        public static RTClient GetInstance()
        {
            if (instance == null)
            {
                instance = new RTClient();
            }
            RTClientUpdater.AssertExistence();
            return instance;
        }

        public SixDOFBody GetBody(string name)
        {
            return networkState.GetBody(name);
        }

        public Skeleton GetSkeleton(string name)
        {
            return networkState.GetSkeleton(name);
        }

        public LabeledMarker GetMarker(string name)
        {
            return networkState.GetMarker(name);
        }

        public UnlabeledMarker GetUnlabeledMarker(uint id)
        {
            return networkState.GetUnlabeledMarker(id);
        }

        public AnalogChannel GetAnalogChannel(string name)
        {
            return networkState.GetAnalogChannel(name);
        }
        public List<AnalogChannel> GetAnalogChannels(List<string> names)
        {
            return networkState.GetAnalogChannels(names);
        }

        public bool GetStreamingStatus() 

        {
            return ConnectionState == RTConnectionState.Connected && networkState.isStreaming;
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
            return ConnectionState != RTConnectionState.Disconnected;
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
            networkState = new RTState();
            networkState.connectionState = RTConnectionState.Connecting;

            errorString = string.Empty;
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
            while (ConnectionState == RTConnectionState.Connecting)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(200));
                UpdateThread();
            }
            return ConnectionState == RTConnectionState.Connected;
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
            while (ConnectionState == RTConnectionState.Connecting)
            {
                Thread.Sleep(TimeSpan.FromMilliseconds(200));
                UpdateThread();
            }
            return ConnectionState == RTConnectionState.Connected;
        }

        public string GetErrorString()
        {
            return errorString;
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

            if (ConnectionState != RTConnectionState.Disconnected)
            {
                UpdateThread();
            }
        }

        void UpdateThread() 
        {
            var oldConnectionState = ConnectionState;
            rtStreamThread.UpdateState(networkState);


            if (!string.IsNullOrEmpty(networkState.errorString)) 
            {
                errorString = networkState.errorString;
            }
            
            foreach (var x in networkState.events)
            {

                if (onNetworkEvent != null)
                {
                    onNetworkEvent(x);
                }
            }

            if (qtmCommandAwaiter.IsAwaiting)
            {
                QtmCommandHandler.CommandAndResultPair pair;
                foreach (var x in networkState.events) 
                {
                    qtmCommandAwaiter.AppendEvent(x);
                }
                foreach (var x in networkState.commandStrings)
                {
                    qtmCommandAwaiter.AppendCommandString(x);
                }

                if (qtmCommandAwaiter.TryGetResult(out pair))
                {
                    pair.command.OnResult(pair.result);
                }
            }

            if (!qtmCommandAwaiter.IsAwaiting && qtmCommands.Count > 0)
            {
                var c = qtmCommands.Dequeue();
                qtmCommandAwaiter.Await(c);
                rtStreamThread.SendCommand(c.CommandString);
            }

            if (ConnectionState == RTConnectionState.Disconnected)
            {
                Disconnect();
            }

            if (ConnectionState == RTConnectionState.Connected && oldConnectionState != RTConnectionState.Connected)
            {
                if (RtProtocolVersion < RtProtocolVersionMax)
                {
                    Debug.Log("Using RT protocol " + RtProtocolVersionMax + " failed. QTM is using an older version (" + RtProtocolVersion + "). Some features might not be available.");
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
                Disconnect();
                disposed = true;
                instance = null;
            }
        }
        
    }
}
