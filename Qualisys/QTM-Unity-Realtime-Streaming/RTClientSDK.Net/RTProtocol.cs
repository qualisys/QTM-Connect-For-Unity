// Realtime SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using QTMRealTimeSDK.Data;
using QTMRealTimeSDK.Network;
using QTMRealTimeSDK.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace QTMRealTimeSDK
{
    /// <summary>Streaming rate</summary>
    public enum StreamRate
    {
        RateAllFrames = 1,
        RateFrequency,
        RateFrequencyDivisor
    }

    /// <summary>Data with response from Discovery broadcast</summary>
    public struct DiscoveryResponse
    {
        /// <summary>Hostname of server</summary>
        public string HostName;
        /// <summary>IP to server</summary>
        public string IpAddress;
        /// <summary>Base port</summary>
        public short Port;
        /// <summary>Info text about host</summary>
        public string InfoText;
        /// <summary>Number of cameras connected to server</summary>
        public int CameraCount;
    }

    public class RTProtocol : IDisposable
    {
        /// <summary>Constants relating to Protocol</summary>
        public static class Constants
        {
            /// <summary>Latest major version of protocol</summary>
            public const int MAJOR_VERSION = 1;
            /// <summary>Latest minor version of protocol</summary>
            public const int MINOR_VERSION = 19;
            /// <summary>Maximum camera count</summary>
            public const int MAX_CAMERA_COUNT = 256;
            /// <summary>Maximum Analog device count</summary>
            public const int MAX_ANALOG_DEVICE_COUNT = 64;
            /// <summary>Maximum force plate count</summary>
            public const int MAX_FORCE_PLATE_COUNT = 64;
            /// <summary>Maximum Gaze vector count</summary>
            public const int MAX_GAZE_VECTOR_COUNT = 64;
            /// <summary>Size of all packet headers, packet size and packet type</summary>
            public const int PACKET_HEADER_SIZE = 8;
            /// <summary>Size of data packet header, timestamp, frame number and component count</summary>
            public const int DATA_PACKET_HEADER_SIZE = 16;
            /// <summary>Size of component header, component size and component type</summary>
            public const int COMPONENT_HEADER = 8;
            /// <summary>Default base port used by QTM</summary>
            public const int STANDARD_BASE_PORT = 22222;
            /// <summary>Port QTM listens to for discovery requests</summary>
            public const int STANDARD_BROADCAST_PORT = 22226;

        }

        public delegate void ProcessStream(RTPacket packet);
        /// <summary>Callback for processing real time data packets</summary>
        public ProcessStream RealTimeDataCallback;
        /// <summary>Callback for receiving events</summary>
        public ProcessStream EventDataCallback;

        /// <summary>Packet received from QTM</summary>
        public RTPacket Packet { get { return mPacket; } }

        SettingsGeneral mGeneralSettings;
        /// <summary>General settings from QTM</summary>
        public SettingsGeneral GeneralSettings { get { return mGeneralSettings; } }

        Settings3D m3DSettings;
        /// <summary>3D settings from QTM</summary>
        public Settings3D Settings3D { get { return m3DSettings; } }

        Settings6D m6DOFSettings;
        /// <summary>6DOF settings from QTM</summary>
        public Settings6D Settings6DOF { get { return m6DOFSettings; } }

        SettingsAnalog mAnalogSettings;
        /// <summary>Analog settings from QTM</summary>
        public SettingsAnalog AnalogSettings { get { return mAnalogSettings; } }

        SettingsForce mForceSettings;
        /// <summary>Force settings from QTM</summary>
        public SettingsForce ForceSettings { get { return mForceSettings; } }

        SettingsImage mImageSettings;
        /// <summary>Image settings from QTM</summary>
        public SettingsImage ImageSettings { get { return mImageSettings; } }

        SettingsGazeVectors mGazeVectorSettings;
        /// <summary>Gaze vector settings from QTM</summary>
        public SettingsGazeVectors GazeVectorSettings { get { return mGazeVectorSettings; } }

        private SkeletonSettingsCollection mSkeletonSettingsCollection;
        /// <summary>Skeleton settings from QTM</summary>
        public SkeletonSettingsCollection SkeletonSettingsCollection { get { return mSkeletonSettingsCollection; } }

        private bool mBroadcastSocketCreated = false;
        private Thread mProcessStreamthread;
        private RTNetwork mNetwork;
        private ushort mUDPport;
        private RTPacket mPacket;
        private int mMajorVersion;
        private int mMinorVersion;
        private volatile bool mThreadActive;
        private string mErrorString;

        private HashSet<DiscoveryResponse> mDiscoveryResponses;
        /// <summary>list of discovered QTM server possible to connect to</summary>
        public HashSet<DiscoveryResponse> DiscoveryResponses { get { return mDiscoveryResponses; } }

        /// <summary>
        /// Default constructor
        ///</summary>
        public RTProtocol(int majorVersion = Constants.MAJOR_VERSION, int minorVersion = Constants.MINOR_VERSION)
        {
            mMajorVersion = majorVersion;
            mMinorVersion = minorVersion;

            mPacket = new RTPacket(mMinorVersion, mMajorVersion);
            mErrorString = "";

            mNetwork = new RTNetwork();
            mBroadcastSocketCreated = false;
            mDiscoveryResponses = new HashSet<DiscoveryResponse>();
        }

        /// <summary>Create connection to server</summary>
        /// <param name="serverAddr">address to server</param>
        /// <param name="serverPortUDP">port to use if UDP socket is desired, set to 0 for automatic port selection</param>
        /// <param name="majorVersion">Major protocol version to use, default is latest</param>
        /// <param name="minorVersion">Minor protocol version to use, default is latest</param>
        /// <param name="port">base port for QTM server, default is 22222</param>
        /// <returns>true if connection was successful, otherwise false</returns>
        public bool Connect(string serverAddr, short serverPortUDP = -1, int majorVersion = Constants.MAJOR_VERSION, int minorVersion = Constants.MINOR_VERSION, int port = Constants.STANDARD_BASE_PORT)
        {
            Disconnect();
            mMajorVersion = majorVersion;
            mMinorVersion = minorVersion;

            if (mMajorVersion > 1 || mMinorVersion > 7)
            {
                // Allow for 1.8 and above versions
            }
            else
            {
                mErrorString = "Protocol version of 1.8 or lower can not be used by the c# software development kit";
                return false;
            }

            PacketType packetType;

            port += 1;

            mPacket = new RTPacket(majorVersion, minorVersion);

            if (mNetwork.Connect(serverAddr, port))
            {
                if (serverPortUDP >= 0)
                {
                    mUDPport = (ushort)serverPortUDP;

                    if (mNetwork.CreateUDPSocket(ref mUDPport, false) == false)
                    {
                        mErrorString = String.Format("Error creating UDP socket: {0}", mNetwork.GetErrorString());
                        Disconnect();
                        return false;
                    }
                }

                //Get connection response from server
                if (ReceiveRTPacket(out packetType) > 0)
                {
                    if (packetType == PacketType.PacketError)
                    {
                        //Error from QTM
                        mErrorString = mPacket.GetErrorString();
                        Disconnect();
                        return false;
                    }

                    if (packetType == PacketType.PacketCommand)
                    {
                        string response = mPacket.GetCommandString();
                        if (response == "QTM RT Interface connected")
                        {
                            if (SetVersion(mMajorVersion, mMinorVersion, out response))
                            {
                                string expectedResponse = String.Format("Version set to {0}.{1}", mMajorVersion, mMinorVersion);
                                if (response == expectedResponse)
                                {
                                    return true;
                                }
                                else
                                {
                                    mErrorString = "Unexpected response from server";
                                    Disconnect();
                                    return false;
                                }
                            }
                            else
                            {
                                //Error setting version
                                mErrorString = "Error setting version of protocol";
                                Disconnect();
                                return false;
                            }
                        }
                        else
                        {
                            //missing QTM response
                            mErrorString = "Missing response from QTM Server";
                            Disconnect();
                            return false;
                        }
                    }
                }
                else
                {
                    //Error receiving packet.
                    mErrorString = String.Format("Error Recieveing packet: {0}", mNetwork.GetErrorString());
                    Disconnect();
                    return false;
                }
            }
            else
            {
                if (mNetwork.GetError() == SocketError.ConnectionRefused)
                {
                    mErrorString = "Connection refused, Check if QTM is running on target machine";
                    Disconnect();
                }
                else
                {
                    mErrorString = String.Format("Error connecting TCP socket: {0}", mNetwork.GetErrorString());
                    Disconnect();
                }
                return false;

            }
            return false;
        }

        /// <summary>Create connection to server</summary>
        /// <param name="host">host detected via broadcast discovery</param>
        /// <param name="serverPortUDP">port to use if UDP socket is desired, set to 0 for automatic port selection</param>
        /// <param name="majorVersion">Major protocol version to use, default is latest</param>
        /// <param name="minorVersion">Minor protocol version to use, default is latest</param>
        /// <returns>true if connection was successful, otherwise false</returns>
        public bool Connect(DiscoveryResponse host, short serverPortUDP = -1, int majorVersion = Constants.MAJOR_VERSION, int minorVersion = Constants.MINOR_VERSION)
        {
            return Connect(host.IpAddress, serverPortUDP, majorVersion, minorVersion, host.Port);
        }

        public void ClearSettings()
        {
            m3DSettings = null;
            m6DOFSettings = null;
            mAnalogSettings = null;
            mForceSettings = null;
            mGazeVectorSettings = null;
            mSkeletonSettingsCollection = null;
            mGeneralSettings = null;
            mImageSettings = null;
        }

        /// <summary>Disconnect from server</summary>
        public void Disconnect()
        {
            mBroadcastSocketCreated = false;
            if (mProcessStreamthread != null)
            {
                mProcessStreamthread.Abort();
                mProcessStreamthread = null;
            }
            mNetwork.Disconnect();
            mDiscoveryResponses.Clear();

            ClearSettings();
        }

        /// <summary>Check if there is a tcp connection to the server available</summary>
        /// <returns>connection status of TCP socket </returns>
        public bool IsConnected()
        {
            return mNetwork.IsConnected();
        }

        private byte[] data = new byte[65535];
        private Object receiveLock = new Object();

        public int ReceiveRTPacket(out PacketType packetType, int timeout = 500000)
        {
            lock (receiveLock)
            {
                int receivedTotal = 0;
                int frameSize;

                packetType = PacketType.PacketNone;

                receivedTotal = 0;

                int received = mNetwork.Receive(ref data, 0, data.Length, true, timeout);
                if (received == 0)
                {
                    return 0; // Receive timeout
                }
                if (received < sizeof(int) * 2)
                {
                    // QTM header not received.
                    return -1;
                }
                if (received == -1)
                {
                    if (!mNetwork.IsConnected())
                    {
                        mErrorString = "Disconnected from server.";
                    }
                    else
                    {
                        mErrorString = "Socket Error.";
                    }
                    return -1;
                }
                receivedTotal += received;

                frameSize = RTPacket.GetPacketSize(data);
                packetType = RTPacket.GetPacketType(data);

                if (data == null || frameSize > data.Length)
                {
                    // Do some preventive additional allocation to reduce number of times allocation is needed
                    var newSize = (int)(frameSize * 1.47);
                    Array.Resize(ref data, newSize);
                }

                // Receive more data until we have read the whole packet
                while (receivedTotal < frameSize)
                {
                    // As long as we haven't received enough data, wait for more
                    received = mNetwork.Receive(ref data, receivedTotal, frameSize - receivedTotal, false, timeout);
                    if (received <= 0)
                    {
                        if (!mNetwork.IsConnected())
                        {
                            mErrorString = "Disconnected from server.";
                        }
                        else
                        {
                            mErrorString = "Socket Error.";
                        }
                        return -1;
                    }
                    receivedTotal += received;
                }
                mPacket.SetData(data);

                if (receivedTotal == frameSize)
                {
                    return receivedTotal;
                }
                mErrorString = "Packet truncated.";
                return -1;
            }
        }

        /// <summary>
        /// get all data from discovery packet
        /// </summary>
        /// <param name="discoveryResponse">data from packet</param>
        /// <returns>true if </returns>
        public static bool GetDiscoverData(byte[] data, out DiscoveryResponse discoveryResponse)
        {
            var packetSize = BitConverter.ToInt32(data, 0);
            byte[] portData = new byte[2];
            Array.Copy(data, packetSize - 2, portData, 0, 2);
            Array.Reverse(portData);
            discoveryResponse.Port = BitConverter.ToInt16(portData, 0);

            byte[] stringData = new byte[packetSize - 10];
            Array.Copy(data, 8, stringData, 0, packetSize - 10);
            string stringFromByteData = System.Text.Encoding.Default.GetString(stringData);
            string[] splittedData = stringFromByteData.Split(',');

            discoveryResponse.HostName = splittedData[0].Trim();
            discoveryResponse.InfoText = splittedData[1].Trim();

            string camcount = splittedData[2].Trim();
            Regex pattern = new Regex("\\d*");
            Match camMatch = pattern.Match(camcount);

            if (camMatch.Success)
            {
                camcount = camMatch.Groups[0].Value;
                discoveryResponse.CameraCount = int.Parse(camcount);
            }
            else
            {
                discoveryResponse.CameraCount = -1;
            }
            discoveryResponse.IpAddress = null;
            return true;
        }

        /// <summary>
        /// Send discovery packet to network to find available QTM Servers.
        ///</summary>
        /// <param name="replyPort">port for servers to reply.</param>
        /// <param name="discoverPort">port to send discovery packet.</param>
        /// <returns>true if discovery packet was sent successfully</returns>
        public bool DiscoverRTServers(ushort replyPort, ushort discoverPort = Constants.STANDARD_BROADCAST_PORT)
        {
            byte[] port = BitConverter.GetBytes(replyPort);
            byte[] size = BitConverter.GetBytes(10);
            byte[] cmd = BitConverter.GetBytes((int)PacketType.PacketDiscover);

            Array.Reverse(port);

            List<byte> b = new List<byte>();
            b.AddRange(size);
            b.AddRange(cmd);
            b.AddRange(port);

            byte[] msg = b.ToArray();

            //if we don't have a udp broadcast socket, create one
            if (mBroadcastSocketCreated || mNetwork.CreateUDPSocket(ref replyPort, true))
            {
                mBroadcastSocketCreated = true;
                var sendStatus = mNetwork.SendUDPBroadcast(msg, 10);
                if (!sendStatus)
                {
                    // Something major failed when trying to broadcast
                    return false;
                }

                mDiscoveryResponses.Clear();

                const int discoverBufferSize = 65535;
                byte[] discoverBuffer = new byte[discoverBufferSize];
                int received = 0;
                do
                {
                    EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                    received = mNetwork.ReceiveBroadcast(ref discoverBuffer, discoverBufferSize, ref remoteEP, 100000);
                    if (received != -1 && received > 8)
                    {
                        var packetType = RTPacket.GetPacketType(discoverBuffer);
                        if (packetType == PacketType.PacketCommand)
                        {
                            DiscoveryResponse response;
                            if (GetDiscoverData(discoverBuffer, out response))
                            {
                                response.IpAddress = (remoteEP as IPEndPoint).Address.ToString();
                                mDiscoveryResponses.Add(response);
                            }
                        }
                    }
                }
                while (received != -1 && received > 8);
            }

            return true;
        }

        /// <summary>
        /// tell protocol to start a new thread that listens to real time stream and send data to callback functions
        ///</summary>
        /// <returns>always returns true</returns>
        public void ListenToStream()
        {
            mProcessStreamthread = new Thread(ThreadedStreamFunction);
            mThreadActive = true;
            mProcessStreamthread.Start();
        }

        /// <summary>
        /// Function used in thread to listen to real time data stream.
        ///</summary>
        private void ThreadedStreamFunction()
        {
            PacketType packetType;

            while (mThreadActive)
            {
                if (ReceiveRTPacket(out packetType, 0) <= 0)
                {
                    continue;
                }

                var packet = mPacket;
                if (packet != null)
                {
                    if (packetType == PacketType.PacketData)
                    {
                        var realtimeDataCallback = RealTimeDataCallback;
                        if (realtimeDataCallback != null)
                            realtimeDataCallback(packet);
                    }
                    else if (packetType == PacketType.PacketEvent)
                    {
                        var eventDataCallback = EventDataCallback;
                        if (eventDataCallback != null)
                            eventDataCallback(packet);
                    }

                }

            }
        }

        /// <summary>
        /// Tell protocol to stop listening to stream and stop the thread.
        ///</summary>
        public void StopStreamListen()
        {
            mThreadActive = false;
            if (mProcessStreamthread != null)
            {
                mProcessStreamthread.Join(new TimeSpan(0, 1, 0));
            }
        }

        #region get set functions

        /// <summary>Get protocol version used from QTM server</summary>
        /// <param name="majorVersion">Major version of protocol used</param>
        /// <param name="minorVersion">Minor version of protocol used</param>
        /// <returns>true if command and response was successful</returns>
        public bool GetVersion(out int majorVersion, out int minorVersion)
        {
            string response;
            if (SendCommandExpectCommandResponse("Version", out response))
            {
                var versionString = response;
                versionString = versionString.Substring(11);
                Version ver = new Version(versionString);
                majorVersion = ver.Major;
                minorVersion = ver.Minor;
                return true;
            }
            majorVersion = 0;
            minorVersion = 0;
            return false;
        }

        /// <summary>Set what realtime server version QTM should provide</summary>
        /// <param name="majorVersion">Major version of protocol used</param>
        /// <param name="minorVersion">Minor version of protocol used</param>
        /// <returns>true if command was successful</returns>
        public bool SetVersion(int majorVersion, int minorVersion, out string response)
        {
            response = "";
            if (majorVersion < 0 || majorVersion > Constants.MAJOR_VERSION || minorVersion < 0)
            {
                mErrorString = "Incorrect version of protocol";
                return false;
            }

            if (SendCommandExpectCommandResponse("Version " + majorVersion + "." + minorVersion, out response))
            {
                return true;
            }
            return false;
        }

        /// <summary>Ask QTM server what version is used.</summary>
        /// <param name="version">what version server uses</param>
        /// <returns>true if command was sent successfully</returns>
        public bool GetQTMVersion(out string version)
        {
            string response;
            if (SendCommandExpectCommandResponse("QTMVersion", out response))
            {
                version = response;
                return true;
            }
            version = "";
            return false;
        }

        /// <summary>Check license towards QTM Server</summary>
        /// <param name="licenseCode">license code to check</param>
        /// <returns>true if command was successfully sent AND License passed, otherwise false</returns>
        public bool CheckLicense(string licenseCode)
        {
            string response;
            if (SendCommandExpectCommandResponse("CheckLicense " + licenseCode, out response))
            {
                if (response == "License pass")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Stream frames from QTM server
        ///</summary>
        /// <param name="streamRate">what rate server should stream at</param>
        /// <param name="streamValue">related to streamrate, not used if all frames are streamed</param>
        /// <param name="components">List of all component types deisred to stream</param>
        /// <param name="port">if set, streaming will be done by UDP on this port. Has to be set if ipAddress is specified</param>
        /// <param name="ipAddress">if UDP streaming should occur to other ip adress,
        /// if not set streaming occurs on same ip as command came from</param>
        /// <returns></returns>
        public bool StreamFrames(StreamRate streamRate, int streamValue, List<ComponentType> components = null, short udpPort = -1, string ipAddress = "")
        {
            string command = "streamframes";

            switch (streamRate)
            {
                case StreamRate.RateAllFrames:
                    command += " allFrames";
                    break;
                case StreamRate.RateFrequency:
                    command += " Frequency:" + streamValue;
                    break;
                case StreamRate.RateFrequencyDivisor:
                    command += " FrequencyDivisor:" + streamValue;
                    break;
            }

            if (udpPort > 0)
            {
                command += " udp:";
                if (!String.IsNullOrEmpty(ipAddress))
                {
                    command += ipAddress + ":";
                }
                command += udpPort;
            }

            command += BuildStreamString(components);

            return SendString(command, PacketType.PacketCommand);
        }


        public bool StreamFrames(StreamRate streamRate, int streamValue, ComponentType component, short port = -1, string ipAddress = "")
        {
            List<ComponentType> list = new List<ComponentType>();
            list.Add(component);
            return StreamFrames(streamRate, streamValue, list, port, ipAddress);
        }

        /// <summary>
        /// Tell QTM Server to stop streaming frames
        ///</summary>
        /// <returns>true if command was sent successfully</returns>
        public bool StreamFramesStop()
        {
            return SendString("StreamFrames Stop", PacketType.PacketCommand);
        }

        /// <summary>Get latest event from QTM server</summary>
        /// <param name="respondedEvent">even from qtm</param>
        /// <returns>true if command was sent successfully</returns>
        public bool GetState(out QTMEvent respondedEvent)
        {
            if (SendString("GetState", PacketType.PacketCommand))
            {
                int nReceived;
                PacketType packetType;
                do
                {
                    nReceived = ReceiveRTPacket(out packetType, 0);
                    if (nReceived > 0)
                    {
                        respondedEvent = mPacket.GetEvent();
                        return true;
                    }
                }
                while (nReceived > 0);
            }
            respondedEvent = QTMEvent.EventNone;
            return false;
        }

        /// <summary>Send trigger to QTM server</summary>
        /// <returns>True if command and trigger was received successfully</returns>
        public bool SendTrigger()
        {
            string response;
            if (SendCommandExpectCommandResponse("Trig", out response))
            {
                if (response == "Trig ok")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Set an event in QTM</summary>
        /// <param name="label">label of event</param>
        /// <returns>true if event was set successfully</returns>
        public bool SetQTMEvent(string label)
        {
            string response;
            if (SendCommandExpectCommandResponse("setQTMEvent " + label, out response))
            {
                if (response == "Event set")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Take control over QTM</summary>
        /// <param name="password">Password set in client</param>
        /// <returns>True if you become the master</returns>
        public bool TakeControl(string password = "")
        {
            string response;
            if (SendCommandExpectCommandResponse("TakeControl " + password, out response))
            {
                if (response == "You are now master")
                {
                    return true;
                }
                else
                {
                    mErrorString = response;
                    return false;
                }
            }
            return false;
        }

        /// <summary>Release master control over QTM</summary>
        /// <returns>true if control was released or if client already is a regular client</returns>
        public bool ReleaseControl()
        {
            string response;
            if (SendCommandExpectCommandResponse("releaseControl", out response))
            {
                if (response == "You are now a regular client" || response == "You are already a regular client")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Create a new measurement in QTM, connect to the cameras and enter RT (preview) mode. Needs to have control over QTM for this command to work</summary>
        /// <returns>true if succeeded</returns>
        public bool NewMeasurement()
        {
            string response;
            if (SendCommandExpectCommandResponse("New", out response))
            {
                if (response == "Creating new connection" || response == "Already connected")
                {
                    return true;
                }
                else
                {
                    mErrorString = response;
                    return false;
                }
            }
            return false;
        }

        /// <summary>Close the current measurement in QTM. Needs to have control over QTM for this command to work</summary>
        /// <returns>Returns true if measurement was closed or if there was nothing to close</returns>
        public bool CloseMeasurement()
        {
            string response;
            if (SendCommandExpectCommandResponse("Close", out response))
            {
                if (response == "Closing connection" ||
                    response == "No connection to close" ||
                    response == "Closing file")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Start capture in QTM. Needs to have control over QTM for this command to work</summary>
        /// <returns>true if measurement was started</returns>
        public bool StartCapture(bool RTFromFile = false)
        {
            string response;
            string command = (RTFromFile) ? "Start rtfromfile" : "Start";
            if (SendCommandExpectCommandResponse(command, out response))
            {
                if (response == "Starting measurement")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Stop current measurement in QTM. Needs to have control over QTM for this command to work</summary>
        /// <returns>true if measurement was stopped</returns>
        public bool StopCapture()
        {
            string response;
            if (SendCommandExpectCommandResponse("Stop", out response))
            {
                if (response == "Stopping measurement")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Load file, both relative and absolute path works. Needs to have control over QTM for this command to work</summary>
        /// <param name="filename">filename to load</param>
        /// <returns>true if measurement was loaded</returns>
        public bool LoadFile(string filename)
        {
            string response;
            if (SendCommandExpectCommandResponse("Load " + filename, out response))
            {
                if (response == "Measurement loaded")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Save current capture. Needs to have control over QTM for this command to work</summary>
        /// <param name="filename">filename to save at.</param>
        /// <param name="overwrite">if QTM is allowed to override existing files.</param>
        /// <param name="newFilename">if QTM is not allowed to overwrite, the new filename will be sent back </param>
        /// <returns>true if measurement was saved.</returns>
        public bool SaveFile(string filename, bool overwrite, ref string newFilename)
        {
            string command = "Save " + filename;
            if (overwrite)
            {
                command += " overwrite";
            }
            string response;
            if (SendCommandExpectCommandResponse(command, out response))
            {
                if (response.Contains("Measurement saved"))
                {
                    if (response.Contains("Measurement saved"))
                    {
                        Regex pattern = new Regex("'.*'$");
                        Match match = pattern.Match(response);
                        newFilename = match.Value.Replace("'", "");
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>Load project. Needs to have control over QTM for this command to work</summary>
        /// <param name="projectPath">path to project to load</param>
        /// <returns>true if project was loaded</returns>
        public bool LoadProject(string projectPath)
        {
            string response;
            if (SendCommandExpectCommandResponse("LoadProject " + projectPath, out response))
            {
                if (response.Contains("Project loaded"))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>Get general settings from QTM Server and saves data in protocol</summary>
        /// <returns>Returns true if settings was retrieved</returns>
        public bool GetGeneralSettings()
        {
            return GetSettings("General", "General", out mGeneralSettings);
        }

        /// <summary>Get 3D settings from QTM Server</summary>
        /// <returns>Returns true if settings was retrieved</returns>
        public bool Get3dSettings()
        {
            return GetSettings("3D", "The_3D", out m3DSettings);
        }

        /// <summary>Get 6DOF settings from QTM Server</summary>
        /// <returns>Returns true if settings was retrieved</returns>
        public bool Get6dSettings()
        {
            return GetSettings("6D", "The_6D", out m6DOFSettings);
        }

        /// <summary>Get Analog settings from QTM Server</summary>
        /// <returns>Returns true if settings was retrieved</returns>
        public bool GetAnalogSettings()
        {
            return GetSettings("Analog", "Analog", out mAnalogSettings);
        }

        /// <summary>Get Force settings from QTM Server</summary>
        /// <returns>Returns true if settings was retrieved</returns>
        public bool GetForceSettings()
        {
            return GetSettings("Force", "Force", out mForceSettings);
        }

        /// <summary>Get Image settings from QTM Server</summary>
        /// <returns>Returns true if settings was retrieved</returns>
        public bool GetImageSettings()
        {
            return GetSettings("Image", "Image", out mImageSettings);
        }

        /// <summary>Get Gaze vector settings from QTM Server</summary>
        /// <returns>Returns true if settings was retrieved</returns>
        public bool GetGazeVectorSettings()
        {
            return GetSettings("GazeVector", "Gaze_Vector", out mGazeVectorSettings);
        }

        /// <summary>Get Skeleton settings from QTM Server</summary>
        /// <returns>Returns true if settings was retrieved</returns>
        public bool GetSkeletonSettings()
        {
            return GetSettings("Skeleton", "Skeletons", out mSkeletonSettingsCollection);
        }

        internal bool GetSettings<TSettings>(string settingsName, string settingXmlName, out TSettings settingObject)
        {
            string xml;
            if (SendCommandExpectXMLResponse("GetParameters " + settingsName, out xml))
            {
                string error;
                settingObject = ReadSettings<TSettings>(settingXmlName, xml, out error);
                if (settingObject != null)
                {
                    return true;
                }
                else
                {
                    mErrorString = error;
                }

            }
            settingObject = default(TSettings);
            return false;
        }

        public static string CreateSettingsXml<TSettings>(TSettings settings, out string error)
        {
            try
            {
                error = string.Empty;
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
                {
                    OmitXmlDeclaration = true,
                };
                StringBuilder settingsOutput = new StringBuilder();
                using (var writer = XmlWriter.Create(settingsOutput, xmlWriterSettings))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(TSettings));
                    serializer.Serialize(writer, settings);
                    var xmlSettings = "<QTM_Settings>";
                    xmlSettings += settingsOutput.ToString();
                    xmlSettings += "</QTM_Settings>";
                    return xmlSettings;
                }
            }
            catch (System.Exception e)
            {
                error = e.Message;
            }
            return string.Empty;
        }

        #endregion

        #region Read settings internal methods
        internal static string ReplaceBadXMLCharacters(string xmldata)
        {
            return xmldata.Replace("True", "true").Replace("False", "false").Replace("None", "-1").Replace(",", ".");
        }

        internal static TSettings ReadSettings<TSettings>(string name, string xmldata, out string error)
        {
            xmldata = ReplaceBadXMLCharacters(xmldata);

            XmlSerializer serializer = new XmlSerializer(typeof(TSettings));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xmldata));

            error = string.Empty;
            XmlReader xmlReader = null;
            TSettings settings = default(TSettings);
            try
            {
                xmlReader = XmlReader.Create(ms);
                if (xmlReader.ReadToDescendant(name))
                {
                    settings = (TSettings)serializer.Deserialize(xmlReader.ReadSubtree());
                    if (settings is SettingsBase)
                    {
                        (settings as SettingsBase).Xml = xmldata;
                    }
                }
                else
                {
                    error = string.Format("Could not find '{0}' descendent in the given xml data {1}", name, xmldata);
                }
            }
            catch (Exception e)
            {
                error = e.Message;
            }
            if (xmlReader != null)
            {
                xmlReader.Close();
            }
            return settings;
        }
        #endregion

        #region Generic communication methods

        /// <summary>Send string to QTM server</summary>
        /// <param name="stringToSend">string with data to send</param>
        /// <param name="packetType">what type of packet it should be sent as</param>
        /// <returns>true if string was sent successfully</returns>
        internal bool SendString(string stringToSend, PacketType packetType)
        {
            if (mNetwork.IsConnected())
            {
                byte[] str = Encoding.ASCII.GetBytes(stringToSend);
                byte[] size = BitConverter.GetBytes(str.Length + Constants.PACKET_HEADER_SIZE);
                byte[] cmd = BitConverter.GetBytes((int)packetType);

                List<byte> b = new List<byte>();
                b.AddRange(size);
                b.AddRange(cmd);
                b.AddRange(str);

                byte[] msg = b.ToArray();
                bool status = mNetwork.Send(msg, str.Length + Constants.PACKET_HEADER_SIZE);

                return status;
            }
            return false;
        }

        /// <summary>Send command QTM server and expect a XML response packet</summary>
        /// <param name="command">Command to send</param>
        /// <param name="xml">Returned xml data string</param>
        /// <returns>true if command was sent successfully</returns>
        public bool SendCommandExpectXMLResponse(string command, out string xml)
        {
            if (SendString(command, PacketType.PacketCommand))
            {
                Thread.Sleep(50);
                PacketType packetType;
                while (ReceiveRTPacket(out packetType) > 0)
                {
                    if (packetType != PacketType.PacketXML)
                    {
                        if (packetType == PacketType.PacketError)
                        {
                            xml = "";
                            mErrorString = mPacket.GetErrorString();
                            return false;
                        }
                    }
                    else
                    {
                        xml = mPacket.GetXMLString();
                        return true;
                    }
                }
            }
            xml = "";
            return false;
        }

        /// <summary>Send command to QTM server and get a command response</summary>
        /// <param name="command">Command to send</param>
        /// <returns>true if server does not reply with error packet</returns>
        public bool SendCommandExpectCommandResponse(string command, out string response)
        {
            if (SendString(command, PacketType.PacketCommand))
            {
                PacketType packetType;
                while (ReceiveRTPacket(out packetType) > 0)
                {
                    if (packetType == PacketType.PacketCommand)
                    {
                        response = mPacket.GetCommandString();
                        return true;
                    }
                    if (packetType == PacketType.PacketError)
                    {
                        response = mPacket.GetErrorString();
                        mErrorString = mPacket.GetErrorString();
                        return false;
                    }
                }
            }
            response = "Command failed";
            return false;
        }

        /// <summary>Send XML data to QTM server</summary>
        /// <param name="xmlString">string with XML data to send</param>
        /// <returns>true if xml was sent successfully</returns>
        public bool SendXML(string xmlString, out string response)
        {
            if (SendString(xmlString, PacketType.PacketXML))
            {
                PacketType packetType;
                while (ReceiveRTPacket(out packetType) > 0)
                {
                    if (packetType == PacketType.PacketCommand)
                    {
                        response = mPacket.GetCommandString();
                        return true;
                    }
                    if (packetType == PacketType.PacketError)
                    {
                        response = mPacket.GetErrorString();
                        mErrorString = mPacket.GetErrorString();
                        return false;
                    }
                }
            }
            response = "Send xml failed";
            return false;
        }

        #endregion

        /// <summary>
        /// Error reported by protocol or from server packet
        ///</summary>
        /// <returns>Error message</returns>
        public string GetErrorString()
        {
            return mErrorString;
        }

        /// <summary>
        /// Builds string for components when using streamframes function
        ///</summary>
        /// <param name="componentTypes">component types to stream</param>
        /// <returns>string with protocol names of components</returns>
        private string BuildStreamString(List<ComponentType> componentTypes)
        {
            if (componentTypes == null)
            {
                return "";
            }

            string command = "";

            foreach (ComponentType type in componentTypes)
            {
                switch (type)
                {
                    case ComponentType.Component3d:
                        command += " 3D";
                        break;
                    case ComponentType.Component3dNoLabels:
                        command += " 3DNoLabels";
                        break;
                    case ComponentType.ComponentAnalog:
                        command += " Analog";
                        break;
                    case ComponentType.ComponentForce:
                        command += " Force";
                        break;
                    case ComponentType.Component6d:
                        command += " 6D";
                        break;
                    case ComponentType.Component6dEuler:
                        command += " 6DEuler";
                        break;
                    case ComponentType.Component2d:
                        command += " 2D";
                        break;
                    case ComponentType.Component2dLinearized:
                        command += " 2DLin";
                        break;
                    case ComponentType.Component3dResidual:
                        command += " 3DRes";
                        break;
                    case ComponentType.Component3dNoLabelsResidual:
                        command += " 3DNoLabelsRes";
                        break;
                    case ComponentType.Component6dResidual:
                        command += " 6DRes";
                        break;
                    case ComponentType.Component6dEulerResidual:
                        command += " 6DEulerRes";
                        break;
                    case ComponentType.ComponentAnalogSingle:
                        command += " AnalogSingle";
                        break;
                    case ComponentType.ComponentImage:
                        command += " Image";
                        break;
                    case ComponentType.ComponentTimecode:
                        command += " Timecode";
                        break;
                    case ComponentType.ComponentForceSingle:
                        command += " ForceSingle";
                        break;
                    case ComponentType.ComponentGazeVector:
                        command += " GazeVector";
                        break;
                    case ComponentType.ComponentSkeleton:
                        command += " Skeleton";
                        break;
                }
            }
            return command;
        }

        #region disposing

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    ReleaseControl();
                    Disconnect();
                    mNetwork.Dispose();                    
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

        ~RTProtocol()
        {
            Dispose(false);
        }

        #endregion
    }
}
