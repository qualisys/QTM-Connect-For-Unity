// Realtime SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System;

namespace QTMRealTimeSDK.Network
{
    internal class RTNetwork : IDisposable
    {
        private UdpClient mUDPClient = null;
        private UdpClient mUDPBroadcastClient = null;
        private TcpClient mTCPClient = null;

        string mErrorString;
        private SocketError mSocketError = SocketError.NotConnected;

        /// <summary>
        /// Default constructor
        /// </summary>
        internal RTNetwork()
        {
        }

        ~RTNetwork()
        {
            Dispose(false);
        }

        /// <summary>
        /// Connect TCP socket to a server.
        /// </summary>
        /// <param name="serverAddr">IP or hostname of server.</param>
        /// <param name="port">Port that TCP should use.</param>
        /// <returns>True if connection is successful otherwise false</returns>
        internal bool Connect(string serverAddr, int port)
        {
            try
            {
                IPAddress[] serverIP = Dns.GetHostAddresses(serverAddr);
                if (serverIP.Length <= 0)
                {
                    mErrorString = "Error looking up host name";
                    return false;
                }

                mTCPClient = new TcpClient();
                // Adding timeout to connection, otherwise system sometimes
                // hangs when attempting to connect to an invalid host; programs won't
                // continue after mTCPClient.Connect() until a new request is made 
                mTCPClient.SendTimeout = 500;
                mTCPClient.Connect(serverIP[0], port);
                // Disable Nagle's algorithm
                mTCPClient.NoDelay = true;
            }
            catch (SocketException e)
            {
                mErrorString = e.Message;
                mSocketError = e.SocketErrorCode;

                if (mTCPClient != null)
                {
                    mTCPClient.Close();
                }

                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Closes all sockets(command(TCP), stream(UDP) and broadcast(UDP)).
        /// </summary>
        internal void Disconnect()
        {
            if (mTCPClient != null)
            {
                mTCPClient.Close();
                mTCPClient = null;
            }

            if (mUDPClient != null)
            {
                mUDPClient.Close();
                mUDPClient = null;
            }

            if (mUDPBroadcastClient != null)
            {
                mUDPBroadcastClient.Close();
                mUDPBroadcastClient = null;
            }
        }

        /// <summary>
        /// Check if TCP socket is connected.
        /// </summary>
        /// <returns>true if TCP socket is connected to a server</returns>
        internal bool IsConnected()
        {
            return (mTCPClient != null && mTCPClient.Connected);
        }

        /// <summary>
        /// Creates an UDP socket for streaming or to send broadcast packet for server discovery
        /// </summary>
        /// <param name="udpPort">Port to use for socket. set to 0 to get a free port automatically</param>
        /// <param name="broadcast">Should socket be used for sending broadcast? Default is false</param>
        /// <returns>True if socket creation was successful</returns>
        internal bool CreateUDPSocket(ref ushort udpPort, bool broadcast = false)
        {
            if (udpPort == 0 || udpPort > 1023)
            {
                IPEndPoint e = new IPEndPoint(IPAddress.Any, udpPort);
                UdpClient tempSocket = new UdpClient(e);
                tempSocket.Client.Blocking = false;
                udpPort = (ushort)((IPEndPoint)tempSocket.Client.LocalEndPoint).Port;

                if (broadcast)
                {
                    tempSocket.Client.EnableBroadcast = true;
                    mUDPBroadcastClient = tempSocket;
                }
                else
                {
                    mUDPClient = tempSocket;
                }

                return true;
                
            }
            else
            {
                mErrorString = "Please use port outside of system port range (1024 or greater)";
                return false;
            }
        }

        internal int ReceiveBroadcast(ref byte[] receivebuffer, int bufferSize, ref EndPoint remoteEP, int timeout)
        {
            if (mUDPBroadcastClient == null)
                return -1;

            try
            {
                List<Socket> receiveList = new List<Socket>();
                List<Socket> errorList = new List<Socket>();

                if (mUDPBroadcastClient != null)
                {
                    receiveList.Add(mUDPBroadcastClient.Client);
                    errorList.Add(mUDPBroadcastClient.Client);
                }

                Socket.Select(receiveList, null, errorList, timeout);

                if (mUDPBroadcastClient != null && errorList.Contains(mUDPBroadcastClient.Client))
                {
                    // Error from broadcast socket
                    mErrorString = "Error reading from Broadcast UDP socket";
                }
                else if (mUDPBroadcastClient != null && receiveList.Contains(mUDPBroadcastClient.Client))
                {
                    // Receive data from broadcast socket
                    return mUDPBroadcastClient.Client.ReceiveFrom(receivebuffer, bufferSize, SocketFlags.None, ref remoteEP);
                }
            }
            catch (SocketException exception)
            {
                // Ignore and return
                mErrorString = exception.Message;
            }
            return -1;
        }
        internal int Receive(ref byte[] receivebuffer, int offset, int bufferSize, bool header, int timeout)
        {
            try
            {
                List<Socket> receiveList = new List<Socket>();
                List<Socket> errorList = new List<Socket>();

                if (mTCPClient != null)
                {
                    receiveList.Add(mTCPClient.Client);
                    errorList.Add(mTCPClient.Client);
                }

                if (mUDPClient != null)
                {
                    receiveList.Add(mUDPClient.Client);
                    errorList.Add(mUDPClient.Client);
                }

                if (receiveList.Count == 0)
                {
                    receivebuffer = null;
                    return 0;
                }

                Socket.Select(receiveList, null, errorList, timeout);

                if (mTCPClient != null && errorList.Contains(mTCPClient.Client))
                {
                    // Error from TCP socket
                    mErrorString = "Error reading from TCP socket";
                }
                else if (mTCPClient != null && receiveList.Contains(mTCPClient.Client))
                {
                    // Receive data from TCP socket
                    return mTCPClient.Client.Receive(receivebuffer, offset, header ? RTProtocol.Constants.PACKET_HEADER_SIZE : bufferSize, SocketFlags.None);
                }
                else if (mUDPClient != null && errorList.Contains(mUDPClient.Client))
                {
                    // Error from UDP socket
                    mErrorString = "Error reading from UDP socket";
                }
                else if (mUDPClient != null && receiveList.Contains(mUDPClient.Client))
                {
                    // Receive data from UDP socket
                    return mUDPClient.Client.Receive(receivebuffer, offset, bufferSize, SocketFlags.None);
                }
            }
            catch (SocketException exception)
            {
                // Ignore and return
                mErrorString = exception.Message;
            }
            return -1;
        }

        /// <summary>
        /// Send data from TCP socket.
        /// </summary>
        /// <param name="sendBuffer">data to send.</param>
        /// <param name="bufferSize">size of data to send.</param>
        /// <returns>true if data was sent successfully otherwise false</returns>
        internal bool Send(byte[] sendBuffer, int bufferSize)
        {
            int sentData = 0;

            try
            {
                sentData += mTCPClient.Client.Send(sendBuffer);
            }
            catch(SocketException e)
            {
                mErrorString = e.Message;
                mSocketError = e.SocketErrorCode;
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Try and get all the local ip adresses
        /// </summary>
        /// <returns></returns>
        private List<IPAddress> GetLocalIPAddresses()
        {
            try
            {
                List<IPAddress> localIPs = new List<IPAddress>();

                var hostName = Dns.GetHostName();
                var host = Dns.GetHostEntry(hostName);
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIPs.Add(ip);
                    }
                }
                return localIPs;
            }
            catch (Exception)
            {
                // Ignore exception
            }
            return null;
        }

        /// <summary>
        /// Send data over UDP via broadcast IP
        /// </summary>
        /// <param name="sendBuffer"> Buffer to send over UDP. </param>
        /// <param name="bufferSize"> Size of buffer(should be 10)</param>
        /// <param name="discoverPort"> Port for server to respond on. </param>
        /// <returns></returns>
        internal bool SendUDPBroadcast(byte[] sendBuffer, int bufferSize, int discoverPort = RTProtocol.Constants.STANDARD_BROADCAST_PORT)
        {
            if (mUDPBroadcastClient == null)
                return false;

            try
            {
                var nics = NetworkInterface.GetAllNetworkInterfaces();
                if (nics.Length > 0)
                {
                    foreach (NetworkInterface nic in nics)
                    {
                        try
                        {
                            if (nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet &&
                                nic.NetworkInterfaceType != NetworkInterfaceType.Wireless80211)
                                continue;
                            foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                            {
                                if (ip.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
                                    continue;

                                IPAddress ipv4Mask;
                                try
                                {
                                    ipv4Mask = ip.IPv4Mask;
                                }
                                catch (Exception)
                                {
                                    ipv4Mask = IPAddress.Parse("255.255.255.0");
                                }
                                var broadcastAddress = ip.Address.GetBroadcastAddress(ipv4Mask);
                                if (broadcastAddress != null)
                                {
                                    IPEndPoint e = new IPEndPoint(broadcastAddress, discoverPort);
                                    mUDPBroadcastClient.Client.SendTo(sendBuffer, bufferSize, 0, e);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            // Ignore broadcast failure, since we might have more ips to send to
                        }

                    }
                }
                else
                {
                    var localIPs = GetLocalIPAddresses();
                    foreach (var ip in localIPs)
                    {
                        try
                        {
                            var ipv4Mask = IPAddress.Parse("255.255.255.0");
                            var broadcastAddress = ip.GetBroadcastAddress(ipv4Mask);
                            if (broadcastAddress != null)
                            {
                                IPEndPoint e = new IPEndPoint(broadcastAddress, discoverPort);
                                mUDPBroadcastClient.Client.SendTo(sendBuffer, bufferSize, 0, e);
                            }
                        }
                        catch (Exception)
                        {
                            // Ignore broadcast failure, since we might have more ips to send to
                        }
                    }
                }
            }
            catch (SocketException e)
            {
                mSocketError = e.SocketErrorCode;
                mErrorString = e.Message;
                return false;
            }
            catch (Exception e)
            {
                mErrorString = e.Message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Error string related to errors that could have occurred during execution of commands
        /// </summary>
        /// <returns>string with error description</returns>
        internal string GetErrorString()
        {
            return mErrorString;
        }

        /// <summary>
        /// More specific error related to socket error occurred during execution of commands.
        /// </summary>
        /// <returns>Socket error</returns>
        internal SocketError GetError()
        {
            return mSocketError;
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

    internal static class IPAddressExtensions
    {
        internal static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
        {
            if (address == null || subnetMask == null)
                return null;

            byte[] ipAddressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAddressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAddressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAddressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }

        internal static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAddressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAddressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAddressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAddressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPAddress(broadcastAddress);
        }

        internal static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            IPAddress network1 = address.GetNetworkAddress(subnetMask);
            IPAddress network2 = address2.GetNetworkAddress(subnetMask);

            return network1.Equals(network2);
        }
    }
}
