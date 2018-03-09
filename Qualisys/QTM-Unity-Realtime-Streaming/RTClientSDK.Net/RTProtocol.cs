// Realtime SDK for Qualisys Track Manager. Copyright 2015 Qualisys AB
//
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Threading;
using System.Linq;

using QTMRealTimeSDK.Settings;
using QTMRealTimeSDK.Network;
using QTMRealTimeSDK.Data;

namespace QTMRealTimeSDK
{
    #region enums
    /// <summary>streaming rate</summary>
    public enum StreamRate
    {
        RateAllFrames = 1,
        RateFrequency,
        RateFrequencyDivisor
    }

    /// <summary>Camera models</summary>
    public enum CameraModel
    {
        [XmlEnum("MacReflex")]
        ModelMacReflex = 0,
        [XmlEnum("ProReflex 120")]
        ModelProReflex120,
        [XmlEnum("ProReflex 240")]
        ModelProReflex240,
        [XmlEnum("ProReflex 500")]
        ModelProReflex500,
        [XmlEnum("ProReflex 1000")]
        ModelProReflex1000,
        [XmlEnum("Oqus 100 ")]
        ModelQqus100,
        [XmlEnum("Oqus 300")]
        ModelQqus300,
        [XmlEnum("Oqus 300 Plus")]
        ModelQqus300Plus,
        [XmlEnum("Oqus 400")]
        ModelQqus400,
        [XmlEnum("Oqus 500")]
        ModelQqus500,
        [XmlEnum("Oqus 200 C")]
        ModelQqus200C,
        [XmlEnum("Oqus 500 Plus")]
        ModelQqus500Plus,
        [XmlEnum("Oqus 700")]
        ModelQqus700,
        [XmlEnum("Oqus 700 Plus")]
        ModelQqus700Plus,
    }

    /// <summary>Camera modes</summary>
    public enum CameraMode
    {
        [XmlEnum("Marker")]
        ModeMarker = 0,
        [XmlEnum("Marker Intensity")]
        ModeMarkerIntensity,
        [XmlEnum("Video")]
        ModeVideo
    }

    /// <summary>Sync out modes</summary>
    public enum SyncOutFreqMode
    {
        [XmlEnum("Shutter out")]
        ModeShutterOut = 0,
        [XmlEnum("Multiplier")]
        ModeMultiplier,
        [XmlEnum("Divisor")]
        ModeDivisor,
        [XmlEnum("Camera independent")]
        ModeActualFreq,
        [XmlEnum("Measurement time")]
        ModeActualMeasurementTime,
        [XmlEnum("SRAM wired")]
        ModeSRAMWireSync,
        [XmlEnum("Continuous 100Hz")]
        ModeFixed100Hz
    }

    /// <summary>Signal sources</summary>
    public enum SignalSource
    {
        [XmlEnum("Control port")]
        SourceControlPort = 0,
        [XmlEnum("IR_receiver")]
        SourceIRReceiver,
        [XmlEnum("SMPTE")]
        SourceSMPTE,
        [XmlEnum("Video_sync")]
        SourceVideoSync
    }

    /// <summary>Signal modes</summary>
    public enum SignalMode
    {
        [XmlEnum("Periodic")]
        Periodic = 0,
        [XmlEnum("Non-periodic")]
        NonPeriodic
    }

    /// <summary>Axises</summary>
    public enum Axis
    {
        [XmlEnum("+X")]
        XAxisUpwards = 0,
        [XmlEnum("-X")]
        XAxisDownwards,
        [XmlEnum("+Y")]
        YAxisUpwards,
        [XmlEnum("-Y")]
        YAxisDownwards,
        [XmlEnum("+Z")]
        ZAxisUpwards,
        [XmlEnum("-Z")]
        ZAxisDownwards
    }

    /// <summary>Signal Edge</summary>
    public enum SignalEdge
    {
        [XmlEnum("Negative")]
        Negative = 0,
        [XmlEnum("Positive")]
        Positive
    }

    /// <summary>Signal Polarity</summary>
    public enum SignalPolarity
    {
        [XmlEnum("Negative")]
        Negative = 0,
        [XmlEnum("Positive")]
        Positive
    }

    /// <summary>Image formats Available</summary>
    public enum ImageFormat
    {
        [XmlEnum("RAWGrayscale")]
        FormatRawGrayScale = 0,
        [XmlEnum("RAWBGR")]
        FormatRawBGR,
        [XmlEnum("JPG")]
        FormatJPG,
        [XmlEnum("PNG")]
        FormatPNG
    }
    #endregion

    public class RTProtocol : IDisposable
    {
        /// <summary>Constants relating to Protocol</summary>
        public static class Constants
        {
            /// <summary>Latest major version of protocol</summary>
            public const int MAJOR_VERSION = 1;
            /// <summary>Latest minor version of protocol</summary>
            public const int MINOR_VERSION = 14;
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

        SettingsGazeVector mGazeVectorSettings;
        /// <summary>Gaze vector settings from QTM</summary>
        public SettingsGazeVector GazeVectorSettings { get { return mGazeVectorSettings; } }

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

        /// <summary>
        /// Create connection to server
        ///</summary>
        /// <param name="serverAddr">Adress to server</param>
        /// <param name="serverPortUDP">port to use if UDP socket is desired, set to 0 for automatic port selection</param>
        /// <param name="majorVersion">Major protocol version to use, default is latest</param>
        /// <param name="minorVersion">Minor protocol version to use, default is latest</param>
        /// <param name="port">base port for QTM server, default is 22222</param>
        /// <returns>true if connection was successful, otherwise false</returns>
        public bool Connect(string serverAddr, short serverPortUDP = -1,
        int majorVersion = Constants.MAJOR_VERSION, int minorVersion = Constants.MINOR_VERSION,
        int port = Constants.STANDARD_BASE_PORT)
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
                mErrorString = "Version of 1.8 or less of the protocol can not be used by the c# sdk";
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
                            if (SetVersion(mMajorVersion, mMinorVersion))
                            {
                                string expectedResponse = String.Format("Version set to {0}.{1}", mMajorVersion, mMinorVersion);
                                response = mPacket.GetCommandString();
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

        /// <summary>
        /// Create connection to server
        ///</summary>
        /// <param name="host">host detected via broadcast discovery</param>
        /// <param name="serverPortUDP">port to use if UDP socket is desired, set to 0 for automatic port selection</param>
        /// <param name="majorVersion">Major protocol version to use, default is latest</param>
        /// <param name="minorVersion">Minor protocol version to use, default is latest</param>
        /// <returns>true if connection was successful, otherwise false</returns>
        public bool Connect(DiscoveryResponse host, short serverPortUDP = -1, int majorVersion = Constants.MAJOR_VERSION, int minorVersion = Constants.MINOR_VERSION)
        {
            return Connect(host.IpAddress, serverPortUDP, majorVersion, minorVersion, host.Port);
        }

        /// <summary>
        /// Creates an UDP socket
        ///</summary>
        /// <param name="udpPort">Port to listen to. </param>
        /// <param name="broadcast">if port should be able to send broadcast packets</param>
        /// <returns></returns>
        public bool CreateUDPSocket(ref ushort udpPort, bool broadcast = false)
        {
            return mNetwork.CreateUDPSocket(ref udpPort, broadcast);
        }

        /// <summary>
        /// Disconnect sockets from server
        ///</summary>
        public void Disconnect()
        {
            mBroadcastSocketCreated = false;
            if (mProcessStreamthread != null)
            {
                mProcessStreamthread.Abort();
                mProcessStreamthread = null;
            }
            mNetwork.Disconnect();
        }

        /// <summary>
        /// Check it our TCP is connected
        ///</summary>
        /// <returns>connection status of TCP socket </returns>
        public bool IsConnected()
        {
            return mNetwork.IsConnected();
        }

        byte[] data = null;

        /// <summary>
        /// Receive data from sockets and save to protocol packet.
        ///</summary>
        /// <param name="packetType">type of packet received from sockets. </param>
        /// <returns>number of bytes received</returns>
        private int ReceiveRTPacket(out PacketType packetType)
        {
            if (data == null)
                data = new byte[65536];

            int recvBytes = mNetwork.Receive(ref data);
            if (recvBytes > 0)
            {
                mPacket.SetData(data);
                packetType = mPacket.PacketType;
            }
            else
            {
                packetType = PacketType.PacketNone;
            }

            return recvBytes;
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
            bool status = false;

            //if we don't have a udp broadcast socket, create one
            if (mBroadcastSocketCreated || mNetwork.CreateUDPSocket(ref replyPort, true))
            {
                mBroadcastSocketCreated = true;
                status = mNetwork.SendUDPBroadcast(msg, 10);

                mDiscoveryResponses.Clear();

                int receieved = 0;
                PacketType packetType;
                do
                {
                    receieved = ReceiveRTPacket(out packetType);
                    if (packetType == PacketType.PacketCommand)
                    {
                        DiscoveryResponse response;
                        if (mPacket.GetDiscoverData(out response))
                        {
                            mDiscoveryResponses.Add(response);
                        }
                    }
                }
                while (receieved > 0);
            }

            return status;
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
                ReceiveRTPacket(out packetType);

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

        /// <summary>
        /// Get protocol version used from QTM server
        ///</summary>
        /// <param name="majorVersion">Major version of protocol used</param>
        /// <param name="minorVersion">Minor version of protocol used</param>
        /// <returns>true if command and response was successful</returns>
        public bool GetVersion(out int majorVersion, out int minorVersion)
        {
            if (SendCommand("Version"))
            {
                PacketType responsePacket = mPacket.PacketType;
                if (responsePacket != PacketType.PacketError)
                {
                    string versionString = mPacket.GetCommandString();
                    versionString = versionString.Substring(11);
                    Version ver = new Version(versionString);
                    majorVersion = ver.Major;
                    minorVersion = ver.Minor;
                    return true;
                }
            }
            majorVersion = 0;
            minorVersion = 0;
            return false;
        }

        /// <summary>
        /// Set what version QTM server should use
        ///</summary>
        /// <param name="majorVersion">Major version of protocol used</param>
        /// <param name="minorVersion">Minor version of protocol used</param>
        /// <returns>true if command was successful</returns>
        public bool SetVersion(int majorVersion, int minorVersion)
        {
            if (majorVersion < 0 || majorVersion > Constants.MAJOR_VERSION || minorVersion < 0)
            {
                mErrorString = "Incorrect version of protocol";
                return false;
            }

            if (SendCommand("Version " + majorVersion + "." + minorVersion))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Ask QTM server what version is used.
        ///</summary>
        /// <param name="version">what version server uses</param>
        /// <returns>true if command was sent successfully</returns>
        public bool GetQTMVersion(out string version)
        {
            if (SendCommand("QTMVersion"))
            {
                PacketType responsePacketType = mPacket.PacketType;
                if (responsePacketType == PacketType.PacketCommand)
                {
                    version = mPacket.GetCommandString();
                    return true;
                }
            }
            version = "";
            return false;
        }

        /// <summary>
        /// Get byte order used by server
        ///</summary>
        /// <param name="bigEndian">response from server if it uses big endian or not</param>
        /// <returns>true if command was sent successfully</returns>
        public bool GetByteOrder(out bool bigEndian)
        {
            if (SendCommand("ByteOrder"))
            {
                PacketType responsePacketType = mPacket.PacketType;
                if (responsePacketType == PacketType.PacketCommand)
                {
                    string response = mPacket.GetCommandString();
                    if (response == "Byte order is big endian")
                        bigEndian = true;
                    else
                        bigEndian = false;
                    return true;
                }
            }
            bigEndian = false;
            return false;
        }

        /// <summary>
        /// Check license towards QTM Server
        ///</summary>
        /// <param name="licenseCode">license code to check</param>
        /// <returns>true if command was successfully sent AND License passed, otherwise false</returns>
        public bool CheckLicense(string licenseCode)
        {
            if (SendCommand("CheckLicense " + licenseCode))
            {
                PacketType responsePacketType = mPacket.PacketType;
                if (responsePacketType == PacketType.PacketCommand)
                {
                    string response = mPacket.GetCommandString();
                    if (response == "License pass")
                    {
                        return true;

                    }
                    else
                    {
                        mErrorString = "Wrong license code.";
                        return false;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Get current frame from server
        ///</summary>
        /// <param name="streamAll">boolean if all component types should be streamed</param>
        /// <param name="components">list of specific component types to stream, ignored if streamAll is set to true</param>
        /// <returns>true if command was sent successfully and response was a datapacket with frame</returns>
        public bool GetCurrentFrame(bool streamAll, List<ComponentType> components = null)
        {
            string command = "getCurrentFrame";

            if (streamAll)
                command += " all";
            else
                command += BuildStreamString(components);

            if (SendCommand(command))
            {
                PacketType responsePacketType = mPacket.PacketType;
                if (responsePacketType == PacketType.PacketData)
                {
                    return true;
                }
                else if (responsePacketType == PacketType.PacketNoMoreData)
                {
                    mErrorString = "No data available";
                    return false;
                }
                else
                {
                    mErrorString = mPacket.GetErrorString();
                    return false;

                }
            }
            return false;
        }

        ///</summary>
        /// Get current frame from server
        ///</summary>
        /// <param name="streamAll">boolean if all component types should be streamed</param>
        /// <param name="packet">packet with data returned from server</param>
        /// <param name="components">list of specific component types to stream, ignored if streamAll is set to true</param>
        /// <returns>true if command was sent successfully and response was a datapacket with frame</returns>
        public bool GetCurrentFrame(out RTPacket packet, bool streamAll, List<ComponentType> components = null)
        {
            bool status;
            if (components != null)
            {
                status = GetCurrentFrame(streamAll, components);
            }
            else
            {
                status = GetCurrentFrame(streamAll);
            }

            if (status)
            {
                packet = mPacket;
                return true;
            }
            else
            {
                packet = RTPacket.ErrorPacket;
                return false;
            }
        }

        /// <summary>
        /// Stream frames from QTM server
        ///</summary>
        /// <param name="streamRate">what rate server should stream at</param>
        /// <param name="streamValue">related to streamrate, not used if all frames are streamed</param>
        /// <param name="streamAllComponents">If all component types should be streamed</param>
        /// <param name="components">List of all component types deisred to stream</param>
        /// <param name="port">if set, streaming will be done by UDP on this port. Has to be set if ipadress is specified</param>
        /// <param name="ipAdress">if UDP streaming should occur to other ip adress,
        /// if not set streaming occurs on same ip as command came from</param>
        /// <returns></returns>
        public bool StreamFrames(StreamRate streamRate, int streamValue,
        bool streamAllComponents, List<ComponentType> components = null,
        short port = -1, string ipAdress = "")
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

            if (ipAdress != "")
            {
                if (port > 0)
                {
                    command += " UDP:" + ipAdress + ":" + port;
                }
                else
                {
                    mErrorString = "If an IP-adress was specified for UDP streaming, a port must be specified aswell";
                    return false;
                }
            }
            else if (port > 0)
            {
                command += " UDP:" + port;
            }

            if (streamAllComponents)
                command += " all";
            else
                command += BuildStreamString(components);

            return SendString(command, PacketType.PacketCommand);
        }


        public bool StreamFrames(StreamRate streamRate, int streamValue,
        bool streamAllComponents, ComponentType component,
        short port = -1, string ipAdress = "")
        {
            List<ComponentType> list = new List<ComponentType>();
            list.Add(component);
            return StreamFrames(streamRate, streamValue, streamAllComponents, list, port, ipAdress);
        }

        /// <summary>
        /// Tell QTM Server to stop streaming frames
        ///</summary>
        /// <returns>true if command was sent successfully</returns>
        public bool StreamFramesStop()
        {
            return SendString("StreamFrames Stop", PacketType.PacketCommand);
        }

        /// <summary>
        /// Get latest event from QTM server
        ///</summary>
        /// <param name="respondedEvent">even from qtm</param>
        /// <returns>true if command was sent successfully</returns>
        public bool GetState(out QTMEvent respondedEvent)
        {
            if (SendCommand("GetState"))
            {
                respondedEvent = mPacket.GetEvent();
                return true;
            }
            respondedEvent = QTMEvent.EventNone;
            return false;
        }

        /// <summary>
        /// Send trigger to QTM server
        ///</summary>
        /// <returns>True if command and trigger was received successfully</returns>
        public bool SendTrigger()
        {
            if (SendCommand("Trig"))
            {
                if (mPacket.GetCommandString() == "Trig ok")
                {
                    return true;
                }
                else
                {
                    mErrorString = mPacket.GetCommandString();
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Set an event in QTM.
        ///</summary>
        /// <param name="label">label of event</param>
        /// <returns>true if event was set successfully</returns>
        public bool SetQTMEvent(string label)
        {
            if (SendCommand("setQTMEvent " + label))
            {
                string response = mPacket.GetCommandString();
                if (response == "Event set")
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

        /// <summary>
        /// Take control over QTM
        ///</summary>
        /// <param name="password">Password set in client</param>
        /// <returns>True if you become the master</returns>
        public bool TakeControl(string password = "")
        {
            if (SendCommand("TakeControl " + password))
            {
                string response = mPacket.GetCommandString();
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

        /// <summary>
        /// Release master control over QTM
        ///</summary>
        /// <returns>true if control was released or if client already is a regular client</returns>
        public bool ReleaseControl()
        {
            if (SendCommand("releaseControl"))
            {
                string response = mPacket.GetCommandString();
                if (response == "You are now a regular client" || response == "You are already a regular client")
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

        /// <summary>
        /// Create a new measurement in QTM, connect to the cameras and enter RT (preview) mode.
        /// Needs to have control over QTM for this command to work
        ///</summary>
        /// <returns></returns>
        public bool NewMeasurement()
        {
            if (SendCommand("New"))
            {
                string response = mPacket.GetCommandString();
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

        /// <summary>
        /// Close the current measurement in QTM.
        /// Needs to have control over QTM for this command to work
        ///</summary>
        /// <returns>returns true if measurement was closed or if there was nothing to close</returns>
        public bool CloseMeasurement()
        {
            if (SendCommand("Close"))
            {
                string response = mPacket.GetCommandString();
                if (response == "Closing connection" ||
                response == "No connection to close" ||
                response == "Closing file")
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

        /// <summary>
        /// Start capture in QTM.
        /// Needs to have control over QTM for this command to work
        ///</summary>
        /// <returns>true if measurement was started</returns>
        public bool StartCapture(bool RTFromFile = false)
        {
            string command = (RTFromFile) ? "Start rtfromfile" : "Start";
            if (SendCommand(command))
            {
                string response = mPacket.GetCommandString();
                if (response == "Starting measurement")
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

        /// <summary>
        /// Stop current measurement in QTM.
        /// Needs to have control over QTM for this command to work
        ///</summary>
        /// <returns>true if measurement was stopped</returns>
        public bool StopCapture()
        {
            if (SendCommand("Stop"))
            {
                string response = mPacket.GetCommandString();
                if (response == "Stopping measurement")
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

        /// <summary>
        /// Load capture at path filename, both relative and absolute path works
        /// Needs to have control over QTM for this command to work
        ///</summary>
        /// <param name="filename">filename to load</param>
        /// <returns>true if measurement was loaded</returns>
        public bool LoadCapture(string filename)
        {
            if (SendCommand("Load " + filename))
            {
                string response = mPacket.GetCommandString();
                if (response == "Measurement loaded")
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

        /// <summary>
        /// Save capture with filename.
        /// Needs to have control over QTM for this command to work
        ///</summary>
        /// <param name="filename">filename to save at.</param>
        /// <param name="overwrite">if QTM is allowed to override existing files.</param>
        /// <param name="newFilename">if QTM is not allowed to overwrite, the new filename will be sent back </param>
        /// <returns>true if measurement was saved.</returns>
        public bool SaveCapture(string filename, bool overwrite, ref string newFilename)
        {
            string command = "Save " + filename;
            if (overwrite)
                command += " overwrite";
            if (SendCommand(command))
            {
                string response = mPacket.GetCommandString();
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
                else
                {
                    mErrorString = response;
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// load project at path
        /// Needs to have control over QTM for this command to work
        ///</summary>
        /// <param name="projectPath">path to project to load</param>
        /// <returns>true if project was loaded</returns>
        public bool LoadProject(string projectPath)
        {
            if (SendCommand("LoadProject " + projectPath))
            {
                string response = mPacket.GetCommandString();
                if (response.Contains("Project loaded"))
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

        /// <summary>
        /// Get general settings from QTM Server and saves data in protocol
        ///</summary>
        /// <returns>returns true if settings was retrieved</returns>
        public bool GetGeneralSettings()
        {
            if (SendCommand("GetParameters general"))
            {
                string xmlString = mPacket.GetXMLString();
                mGeneralSettings = ReadGeneralSettings(xmlString);
                if (mGeneralSettings != null)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get 3D settings from QTM Server
        ///</summary>
        /// <returns>returns true if settings was retrieved</returns>
        public bool Get3Dsettings()
        {
            if (SendCommand("GetParameters 3D"))
            {
                string xmlString = mPacket.GetXMLString();
                m3DSettings = Read3DSettings(xmlString);
                if (m3DSettings != null)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get 6DOF settings from QTM Server
        ///</summary>
        /// <returns>returns true if settings was retrieved</returns>
        public bool Get6DSettings()
        {
            if (SendCommand("GetParameters 6D"))
            {
                string xmlString = mPacket.GetXMLString();
                m6DOFSettings = Read6DOFSettings(xmlString);
                if (m6DOFSettings != null)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get Analog settings from QTM Server
        ///</summary>
        /// <returns>returns true if settings was retrieved</returns>
        public bool GetAnalogSettings()
        {
            if (SendCommand("GetParameters Analog"))
            {
                string xmlString = mPacket.GetXMLString();
                mAnalogSettings = ReadAnalogSettings(xmlString);
                if (mAnalogSettings != null)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get Force settings from QTM Server
        ///</summary>
        /// <returns>returns true if settings was retrieved</returns>
        public bool GetForceSettings()
        {
            if (SendCommand("GetParameters force"))
            {
                string xmlString = mPacket.GetXMLString();
                mForceSettings = ReadForceSettings(xmlString);
                if (mForceSettings != null)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get Image settings from QTM Server
        ///</summary>
        /// <returns>returns true if settings was retrieved</returns>
        public bool GetImageSettings()
        {
            if (SendCommand("GetParameters Image"))
            {
                string xmlString = mPacket.GetXMLString();
                mImageSettings = ReadImageSettings(xmlString);
                if (mImageSettings != null)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get Gaze vector settings from QTM Server
        ///</summary>
        /// <returns>returns true if settings was retrieved</returns>
        public bool GetGazeVectorSettings()
        {
            if (SendCommand("GetParameters GazeVector"))
            {
                string xmlString = mPacket.GetXMLString();
                mGazeVectorSettings = ReadGazeVectorSettings(xmlString);
                if (mGazeVectorSettings != null)
                    return true;
            }
            return false;
        }

        #endregion

        #region read settings
        /// <summary>
        /// Read general settings from XML string
        ///</summary>
        /// <param name="xmldata">string with xmldata</param>
        /// <returns>class with general settings from QTM</returns>
        public static SettingsGeneral ReadGeneralSettings(string xmldata)
        {
            xmldata = xmldata.Replace("True", "true").Replace("False", "false").Replace("None", "-1");

            XmlSerializer serializer = new XmlSerializer(typeof(SettingsGeneral));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xmldata));

            XmlReader red = XmlReader.Create(ms);

            red.ReadToDescendant("General");
            SettingsGeneral settings;
            try
            {
                settings = (SettingsGeneral)serializer.Deserialize(red.ReadSubtree());
            }
            catch
            {
                settings = null;
            }
            red.Close();

            return settings;
        }

        /// <summary>
        /// Read 3D settings from XML string
        ///</summary>
        /// <param name="xmldata">string with xmldata</param>
        /// <returns>class with data of 3D settings from QTM</returns>
        public static Settings3D Read3DSettings(string xmldata)
        {
            xmldata = xmldata.Replace("True", "true").Replace("False", "false").Replace("None", "-1");

            XmlSerializer serializer = new XmlSerializer(typeof(Settings3D));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xmldata));

            XmlReader red = XmlReader.Create(ms);

            red.ReadToDescendant("The_3D");
            Settings3D settings;
            try
            {
                settings = (Settings3D)serializer.Deserialize(red.ReadSubtree());
            }
            catch
            {
                settings = null;
            }
            red.Close();

            return settings;
        }

        /// <summary>
        /// Read 6DOFsettings from XML string
        ///</summary>
        /// <param name="xmldata">string with xmldata</param>
        /// <returns>class with data of 6DOF settings from QTM</returns>
        public static Settings6D Read6DOFSettings(string xmldata)
        {
            xmldata = xmldata.Replace("True", "true").Replace("False", "false").Replace("None", "-1");

            XmlSerializer serializer = new XmlSerializer(typeof(Settings6D));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xmldata));

            XmlReader red = XmlReader.Create(ms);

            red.ReadToDescendant("The_6D");
            Settings6D settings;
            try
            {
                settings = (Settings6D)serializer.Deserialize(red.ReadSubtree());
            }
            catch
            {
                settings = null;
            }
            red.Close();

            return settings;
        }

        /// <summary>
        /// Read Analog settings from XML string
        ///</summary>
        /// <param name="xmldata">string with xmldata</param>
        /// <returns>class with data of Analog settings from QTM</returns>
        public static SettingsAnalog ReadAnalogSettings(string xmldata)
        {
            xmldata = xmldata.Replace("True", "true").Replace("False", "false").Replace("None", "-1");

            XmlSerializer serializer = new XmlSerializer(typeof(SettingsAnalog));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xmldata));

            XmlReader red = XmlReader.Create(ms);

            red.ReadToDescendant("Analog");
            SettingsAnalog settings;
            try
            {
                settings = (SettingsAnalog)serializer.Deserialize(red.ReadSubtree());
            }
            catch
            {
                settings = null;
            }
            red.Close();

            return settings;
        }

        /// <summary>
        /// Read Force settings from XML string
        ///</summary>
        /// <param name="xmldata">string with xmldata</param>
        /// <returns>class with data of Force settings from QTM</returns>
        public static SettingsForce ReadForceSettings(string xmldata)
        {
            xmldata = xmldata.Replace("True", "true").Replace("False", "false").Replace("None", "-1");

            XmlSerializer serializer = new XmlSerializer(typeof(SettingsForce));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xmldata));

            XmlReader red = XmlReader.Create(ms);

            red.ReadToDescendant("Force");
            SettingsForce settings;
            try
            {
                settings = (SettingsForce)serializer.Deserialize(red.ReadSubtree());
            }
            catch
            {
                settings = null;
            }
            red.Close();

            return settings;
        }

        /// <summary>
        /// Read Image settings from XML string
        ///</summary>
        /// <param name="xmldata">string with xmldata</param>
        /// <returns>class with data of Image settings from QTM</returns>
        public static SettingsImage ReadImageSettings(string xmldata)
        {
            xmldata = xmldata.Replace("True", "true").Replace("False", "false").Replace("None", "-1");

            XmlSerializer serializer = new XmlSerializer(typeof(SettingsImage));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xmldata));

            XmlReader red = XmlReader.Create(ms);

            red.ReadToDescendant("Image");
            SettingsImage settings;
            try
            {
                settings = (SettingsImage)serializer.Deserialize(red.ReadSubtree());
            }
            catch
            {
                settings = null;
            }
            red.Close();


            return settings;
        }


        /// <summary>
        /// Read Gaze Vector settings from XML string
        ///</summary>
        /// <param name="xmldata">string with xmldata</param>
        /// <returns>class with data of Gaze Vector settings from QTM</returns>
        public static SettingsGazeVector ReadGazeVectorSettings(string xmldata)
        {
            xmldata = xmldata.Replace("True", "true").Replace("False", "false").Replace("None", "-1");

            XmlSerializer serializer = new XmlSerializer(typeof(SettingsGazeVector));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xmldata));

            XmlReader red = XmlReader.Create(ms);

            red.ReadToDescendant("Gaze_Vector");
            SettingsGazeVector settings;
            try
            {
                settings = (SettingsGazeVector)serializer.Deserialize(red.ReadSubtree());
            }
            catch
            {
                settings = null;
            }
            red.Close();

            return settings;
        }
        #endregion

        #region set settings

        /// <summary>
        /// Creates xml string from the general settings to send to QTM
        ///</summary>
        /// <param name="generalSettings">generl settings to generate string from</param>
        /// <param name="setProcessingActions">if string should include processing actions or not</param>
        /// <returns>generated xml string from settings</returns>
        public static string SetGeneralSettings(SettingsGeneral generalSettings, bool setProcessingActions = false)
        {
            StringWriter xmlString = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            XmlWriter xmlWriter = XmlWriter.Create(xmlString, settings);

            xmlWriter.WriteStartElement("QTM_Settings");
            {
                xmlWriter.WriteStartElement("General");
                {
                    xmlWriter.WriteElementString("Frequency", generalSettings.captureFrequency.ToString());
                    xmlWriter.WriteElementString("Capture_Time", generalSettings.captureTime.ToString("0.000"));
                    xmlWriter.WriteElementString("Start_On_ExternalTrigger", generalSettings.startOnExternalTrigger.ToString());
                    if (setProcessingActions)
                    {
                        xmlWriter.WriteStartElement("Processing_Actions");
                        switch (generalSettings.processingActions.TrackingActions)
                        {
                            case SettingsTrackingProcessingActions.ProcessingTracking2D:
                                xmlWriter.WriteElementString("Tracking", "2D");
                                break;
                            case SettingsTrackingProcessingActions.ProcessingTracking3D:
                                xmlWriter.WriteElementString("Tracking", "3D");
                                break;
                            default:
                                xmlWriter.WriteElementString("Tracking", "False");
                                break;
                        }

                        xmlWriter.WriteElementString("TwinSystemMerge", generalSettings.processingActions.TwinSystemMerge.ToString());
                        xmlWriter.WriteElementString("SplineFill", generalSettings.processingActions.SplineFill.ToString());
                        xmlWriter.WriteElementString("AIM", generalSettings.processingActions.Aim.ToString());
                        xmlWriter.WriteElementString("Track6DOF", generalSettings.processingActions.Track6DOF.ToString());
                        xmlWriter.WriteElementString("ForceData", generalSettings.processingActions.ForceData.ToString());
                        xmlWriter.WriteElementString("ExportTSV", generalSettings.processingActions.ExportTSV.ToString());
                        xmlWriter.WriteElementString("ExportC3D", generalSettings.processingActions.ExportC3D.ToString());
                        xmlWriter.WriteElementString("ExportMatlabFile", generalSettings.processingActions.ExportMatlab.ToString());
                        xmlWriter.WriteEndElement();

                    }
                }
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            return xmlString.ToString();
        }

        /// <summary>
        /// Creates xml string from the given settings to send to QTM
        ///</summary>
        /// <param name="sSettingsGeneralExternalTimeBase">time base settings to generate string from</param>
        /// <returns>generated xml string from settings</returns>
        public static string SetGeneralExtTimeBase(SettingsGeneralExternalTimeBase timeBaseSettings)
        {
            StringWriter xmlString = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            XmlWriter xmlWriter = XmlWriter.Create(xmlString, settings);

            xmlWriter.WriteStartElement("QTM_Settings");
            {
                xmlWriter.WriteStartElement("General");
                {
                    xmlWriter.WriteStartElement("External_Time_Base");
                    {
                        xmlWriter.WriteElementString("Enabled", timeBaseSettings.Enabled.ToString());
                        switch (timeBaseSettings.SignalSource)
                        {
                            case SignalSource.SourceControlPort:
                                xmlWriter.WriteElementString("Signal_Source", "Control port");
                                break;
                            case SignalSource.SourceIRReceiver:
                                xmlWriter.WriteElementString("Signal_Source", "IR receiver");
                                break;
                            case SignalSource.SourceSMPTE:
                                xmlWriter.WriteElementString("Signal_Source", "SMPTE");
                                break;
                            case SignalSource.SourceVideoSync:
                                xmlWriter.WriteElementString("Signal_Source", "Video sync");
                                break;
                        }

                        if (timeBaseSettings.SignalMode == SignalMode.Periodic)
                        {
                            xmlWriter.WriteElementString("Signal_Mode", "True");
                        }
                        else
                        {
                            xmlWriter.WriteElementString("Signal_Mode", "False");
                        }

                        xmlWriter.WriteElementString("Frequency_Multiplier", timeBaseSettings.FreqMultiplier.ToString());
                        xmlWriter.WriteElementString("Frequency_Divisor", timeBaseSettings.FreqDivisor.ToString());
                        xmlWriter.WriteElementString("Frequency_Tolerance", timeBaseSettings.FreqTolerance.ToString());


                        if (timeBaseSettings.NominalFrequency > 0)
                        {
                            xmlWriter.WriteElementString("Nominal_Frequency", timeBaseSettings.NominalFrequency.ToString("0.000"));
                        }
                        else
                        {
                            xmlWriter.WriteElementString("Nominal_Frequency", "None");
                        }

                        switch (timeBaseSettings.SignalEdge)
                        {
                            case SignalEdge.Negative:
                                xmlWriter.WriteElementString("Signal_Edge", "Negative");
                                break;
                            case SignalEdge.Positive:
                                xmlWriter.WriteElementString("Signal_Edge", "Positive");
                                break;
                        }

                        xmlWriter.WriteElementString("Signal_Shutter_Delay", timeBaseSettings.SignalShutterDelay.ToString());
                        xmlWriter.WriteElementString("Non_Periodic_Timeout", timeBaseSettings.NominalFrequency.ToString("0.000"));

                    }
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            return xmlString.ToString();
        }

        /// <summary>
        /// Creates xml string from the given settings to send to QTM
        ///</summary>
        /// <param name="cameraSettings">Camera settings to generate string from. if camera ID is < 0, setting will be applied to all cameras</param>
        /// <returns>generated xml string from settings</returns>
        public static string CreateGeneralCameraString(SettingsGeneralCamera cameraSettings)
        {
            StringWriter xmlString = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            XmlWriter xmlWriter = XmlWriter.Create(xmlString, settings);

            xmlWriter.WriteStartElement("QTM_Settings");
            {
                xmlWriter.WriteStartElement("General");
                {
                    xmlWriter.WriteStartElement("Camera");
                    {
                        xmlWriter.WriteElementString("ID", cameraSettings.CameraId.ToString());
                        switch (cameraSettings.Mode)
                        {
                            case CameraMode.ModeMarker:
                                xmlWriter.WriteElementString("Mode", "Marker");
                                break;
                            case CameraMode.ModeMarkerIntensity:
                                xmlWriter.WriteElementString("Mode", "Marker Intensity");
                                break;
                            case CameraMode.ModeVideo:
                                xmlWriter.WriteElementString("Mode", "Video");
                                break;
                        }
                        xmlWriter.WriteElementString("Video_Exposure", cameraSettings.VideoExposure.ToString());
                        xmlWriter.WriteElementString("Video_Flash_Time", cameraSettings.VideoFlashTime.ToString());
                        xmlWriter.WriteElementString("Marker_Exposure", cameraSettings.MarkerExposure.ToString());
                        xmlWriter.WriteElementString("Marker_Threshold", cameraSettings.MarkerThreshold.ToString());
                        xmlWriter.WriteElementString("Orientation", cameraSettings.Orientation.ToString());
                    }
                    xmlWriter.WriteEndElement();

                }
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            return xmlString.ToString();
        }

        /// <summary>
        /// create xml string for sync settings for camera
        ///</summary>
        /// <param name="cameraID">Camera settings to generate string from. if camera ID is < 0, setting will be applied to all cameras</param>
        /// <param name="syncSettings">settings to generate string from</param>
        /// <returns>generated xml string from settings</returns>
        public static string CreateGeneralCameraSyncOutString(int cameraID, Sync syncSettings)
        {
            StringWriter xmlString = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            XmlWriter xmlWriter = XmlWriter.Create(xmlString, settings);

            xmlWriter.WriteStartElement("QTM_Settings");
            {
                xmlWriter.WriteStartElement("General");
                {
                    xmlWriter.WriteStartElement("Camera");
                    {
                        xmlWriter.WriteElementString("ID", cameraID.ToString());

                        xmlWriter.WriteStartElement("Sync_Out");
                        {
                            switch (syncSettings.SyncMode)
                            {
                                case SyncOutFreqMode.ModeShutterOut:
                                    xmlWriter.WriteElementString("Mode", "Shutter out");
                                    break;
                                case SyncOutFreqMode.ModeMultiplier:
                                    xmlWriter.WriteElementString("Mode", "Multiplier");
                                    xmlWriter.WriteElementString("Value", syncSettings.SyncValue.ToString());
                                    break;
                                case SyncOutFreqMode.ModeDivisor:
                                    xmlWriter.WriteElementString("Mode", "Divisor");
                                    xmlWriter.WriteElementString("Value", syncSettings.SyncValue.ToString());
                                    break;
                                case SyncOutFreqMode.ModeActualFreq:
                                    xmlWriter.WriteElementString("Mode", "Camera independent");
                                    break;
                                case SyncOutFreqMode.ModeActualMeasurementTime:
                                    xmlWriter.WriteElementString("Mode", "Measurement time");
                                    xmlWriter.WriteElementString("Value", syncSettings.SyncValue.ToString());
                                    break;
                                case SyncOutFreqMode.ModeFixed100Hz:
                                    xmlWriter.WriteElementString("Mode", "Continuous 100Hz");
                                    break;
                            }
                            if (syncSettings.SyncMode != SyncOutFreqMode.ModeSRAMWireSync || syncSettings.SyncMode != SyncOutFreqMode.ModeFixed100Hz)
                            {
                                switch (syncSettings.SignalPolarity)
                                {
                                    case SignalPolarity.Negative:
                                        xmlWriter.WriteElementString("Signal_Polarity", "Negative");
                                        break;
                                    case SignalPolarity.Positive:
                                        xmlWriter.WriteElementString("Signal_Polarity", "Positive");
                                        break;
                                }

                            }
                        }
                        xmlWriter.WriteEndElement();

                    }
                    xmlWriter.WriteEndElement();

                }
                xmlWriter.WriteEndElement();

            }
            xmlWriter.WriteEndElement();

            return xmlString.ToString();
        }

        /// <summary>
        /// create xml string for image settings
        ///</summary>
        /// <param name="sImageCamera">Image settings to generate string from</param>
        /// <returns>generated xml string from settings</returns>
        public static string CreateSetImageSettingsString(ImageCamera imageSettings)
        {
            StringWriter xmlString = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            XmlWriter xmlWriter = XmlWriter.Create(xmlString, settings);
            xmlWriter.WriteStartElement("QTM_Settings");
            {
                xmlWriter.WriteStartElement("Image");
                {
                    xmlWriter.WriteStartElement("Camera");
                    {
                        xmlWriter.WriteElementString("ID", imageSettings.CameraID.ToString());
                        xmlWriter.WriteElementString("Enabled", imageSettings.Enabled.ToString());

                        switch (imageSettings.ImageFormat)
                        {
                            case ImageFormat.FormatRawGrayScale:
                                xmlWriter.WriteElementString("Mode", "RAWGrayscale");
                                break;
                            case ImageFormat.FormatRawBGR:
                                xmlWriter.WriteElementString("Mode", "RAWBGR");
                                break;
                            case ImageFormat.FormatJPG:
                                xmlWriter.WriteElementString("Mode", "JPG");
                                break;
                            case ImageFormat.FormatPNG:
                                xmlWriter.WriteElementString("Mode", "PNG");
                                break;
                        }

                        xmlWriter.WriteElementString("Format", ((int)imageSettings.ImageFormat).ToString());

                        xmlWriter.WriteElementString("Width", imageSettings.Width.ToString());
                        xmlWriter.WriteElementString("Height", imageSettings.Height.ToString());

                        xmlWriter.WriteElementString("Left_Crop", imageSettings.CropLeft.ToString());
                        xmlWriter.WriteElementString("Top_Crop", imageSettings.CropRight.ToString());
                        xmlWriter.WriteElementString("Right_Crop", imageSettings.CropTop.ToString());
                        xmlWriter.WriteElementString("Bottom_Crop", imageSettings.CropBottom.ToString());

                    }
                    xmlWriter.WriteEndElement();

                }
                xmlWriter.WriteEndElement();

            }
            xmlWriter.WriteEndElement();

            return xmlString.ToString();
        }

        /// <summary>
        /// create xml string for force plate settings
        ///</summary>
        /// <param name="plateSettings">force plate settings to generate string from</param>
        /// <returns>generated xml string from settings</returns>
        public static string CreateForceSettingsString(ForcePlateSettings plateSettings)
        {
            StringWriter xmlString = new StringWriter();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            XmlWriter xmlWriter = XmlWriter.Create(xmlString, settings);
            xmlWriter.WriteStartElement("QTM_Settings");
            {
                xmlWriter.WriteStartElement("Force");
                {
                    xmlWriter.WriteStartElement("Plate");
                    {
                        xmlWriter.WriteElementString("Plate_ID", plateSettings.AnalogDeviceID.ToString());

                        xmlWriter.WriteStartElement("Corner1");
                        {
                            xmlWriter.WriteElementString("X", plateSettings.Location.Corner1.X.ToString());
                            xmlWriter.WriteElementString("Y", plateSettings.Location.Corner1.Y.ToString());
                            xmlWriter.WriteElementString("Z", plateSettings.Location.Corner1.Z.ToString());
                        }
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("Corner2");
                        {
                            xmlWriter.WriteElementString("X", plateSettings.Location.Corner2.X.ToString());
                            xmlWriter.WriteElementString("Y", plateSettings.Location.Corner2.Y.ToString());
                            xmlWriter.WriteElementString("Z", plateSettings.Location.Corner2.Z.ToString());
                        }
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("Corner3");
                        {
                            xmlWriter.WriteElementString("X", plateSettings.Location.Corner3.X.ToString());
                            xmlWriter.WriteElementString("Y", plateSettings.Location.Corner3.Y.ToString());
                            xmlWriter.WriteElementString("Z", plateSettings.Location.Corner3.Z.ToString());
                        }
                        xmlWriter.WriteEndElement();

                        xmlWriter.WriteStartElement("Corner4");
                        {
                            xmlWriter.WriteElementString("X", plateSettings.Location.Corner4.X.ToString());
                            xmlWriter.WriteElementString("Y", plateSettings.Location.Corner4.Y.ToString());
                            xmlWriter.WriteElementString("Z", plateSettings.Location.Corner4.Z.ToString());
                        }
                        xmlWriter.WriteEndElement();

                    }
                    xmlWriter.WriteEndElement();

                }
                xmlWriter.WriteEndElement();

            }
            xmlWriter.WriteEndElement();

            return xmlString.ToString();
        }

        #endregion

        #region generic send functions

        /// <summary>
        /// Send string to QTM server
        ///</summary>
        /// <param name="strtoSend">string with data to send</param>
        /// <param name="packetType">what type of packet it should be sent as</param>
        /// <returns>true if string was sent successfully</returns>
        internal bool SendString(string strtoSend, PacketType packetType)
        {
            if (mNetwork.IsConnected())
            {
                byte[] str = Encoding.ASCII.GetBytes(strtoSend);
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

        /// <summary>
        /// Send command to QTM server that TCP socket is connected to
        ///</summary>
        /// <param name="command">command to send</param>
        /// <returns>true if server doesnt reply with error packet</returns>
        public bool SendCommand(string command)
        {
            bool status = SendString(command, PacketType.PacketCommand);
            if (status)
            {
                Thread.Sleep(20); //avoid missing packets
                PacketType responsePacket;
                ReceiveRTPacket(out responsePacket);
                if (responsePacket != PacketType.PacketError)
                {
                    return true;
                }
                else
                {
                    mErrorString = mPacket.GetErrorString();
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Send XML data to QTM server
        ///</summary>
        /// <param name="xmlString">string with XML data to send</param>
        /// <returns>true if xml was sent successfully</returns>
        public bool SendXML(string xmlString)
        {
            if (SendString(xmlString, PacketType.PacketXML))
            {
                PacketType eType;

                if (ReceiveRTPacket(out eType) > 0)
                {
                    if (eType == PacketType.PacketCommand)
                    {
                        return true;
                    }
                    else
                    {
                        mErrorString = mPacket.GetErrorString();
                    }
                }
            }

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
                    case ComponentType.ComponentForceSingle:
                        command += " ForceSingle";
                        break;
                    case ComponentType.ComponentGazeVector:
                        command += " GazeVector";
                        break;
                }
            }
            return command;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disconnect();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}