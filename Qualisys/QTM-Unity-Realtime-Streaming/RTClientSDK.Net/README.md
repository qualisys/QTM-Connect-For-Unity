# [Qualisys](http://www.qualisys.com) Realtime SDK for .Net

This is C# package with SDK and examples that can help Qualisys users connect and stream data from [Qualisys Track Manager](http://www.qualisys.com/products/software/qtm) in realtime.
This handles all kind of data that QTM can capture. This includes marker data, 6dof objects, user bones, analog, force and gaze vector settings and data.

* Use RTProtocol::DiscoverRTServers to find out which QTM servers that you can connect to on a network.
* Use RTProtocol::IsConnected method to check if a connection to QTM already exists.
* Use RTProtocol::Connect method to connect to QTM.
* Use RTProtocol::StreamFrames to setup streaming from QTM (this lets you decide what to stream).
* Use RTProtocol::ReceiveRTPacket to get data/event/error packets.
* Use RTProtocol::StreamFramesStop to stop streaming data.
* Use RTProtocol::Disconnect to disconnect.

Below is a c# console application example on how to discover all QTM servers on the network. Connect to localhost, stream 6DOF position+euler+residual data.

```csharp
using System;
using System.Collections.Generic;
using System.Threading;
using QTMRealTimeSDK;
using QTMRealTimeSDK.Data;

namespace RTSDKExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Example example = new Example();
            example.DiscoverQTMServers(4547);
            Console.WriteLine("Press key to continue");
            Console.ReadKey();
            while (true)
            {
                example.HandleStreaming("127.0.0.1");
                if (Console.KeyAvailable)
                {
                    if (Console.ReadKey(false).Key == ConsoleKey.Escape)
                        break;
                }
            }
        }
    }

    class Example
    {
        RTProtocol rtProtocol = new RTProtocol();

        public List<DiscoveryResponse> DiscoverQTMServers(ushort discoveryPort)
        {
            if (rtProtocol.DiscoverRTServers(discoveryPort))
            {
                var discoveryResponses = rtProtocol.DiscoveryResponses;
                foreach (var discoveryResponse in discoveryResponses)
                {
                    Console.WriteLine("Host:{0,20}\tIP Address:{1,15}\tInfo Text:{2,20}\tCamera count:{3,3}", discoveryResponse.HostName, discoveryResponse.IpAddress, discoveryResponse.InfoText, discoveryResponse.CameraCount);
                }

            }
            return null;
        }

        ~Example()
        {
            if (rtProtocol.IsConnected())
            {
                rtProtocol.StreamFramesStop();
                rtProtocol.Disconnect();
            }
        }

        public void HandleStreaming(string ipAddress)
        {
            // Check if connection to QTM is possible
            if (!rtProtocol.IsConnected())
            {
                if (!rtProtocol.Connect(ipAddress))
                {
                    Console.WriteLine("QTM: Trying to connect");
                    Thread.Sleep(1000);
                    return;
                }
                Console.WriteLine("QTM: Connected");
            }

            // Check for available 6DOF data in the stream
            if (rtProtocol.Settings6DOF == null)
            {
                if (!rtProtocol.Get6dSettings())
                {
                    Console.WriteLine("QTM: Trying to get 6DOF settings");
                    Thread.Sleep(500);
                    return;
                }
                Console.WriteLine("QTM: 6DOF data available");

                rtProtocol.StreamAllFrames(QTMRealTimeSDK.Data.ComponentType.Component6dEulerResidual);
                Console.WriteLine("QTM: Starting to stream 6DOF data");
                Thread.Sleep(500);
            }

            // Get RTPacket from stream
            PacketType packetType;
            rtProtocol.ReceiveRTPacket(out packetType, false);

            // Handle data packet
            if (packetType == PacketType.PacketData)
            {
                var sixDofData = rtProtocol.GetRTPacket().Get6DOFEulerResidualData();
                if (sixDofData != null)
                {
                    // Print out the available 6DOF data.
                    for (int body = 0; body < sixDofData.Count; body++)
                    {
                        var sixDofBody = sixDofData[body];
                        var bodySetting = rtProtocol.Settings6DOF.Bodies[body];
                        Console.WriteLine("Frame:{0:D5} Body:{1,20} X:{2,7:F1} Y:{3,7:F1} Z:{4,7:F1} First Angle:{5,7:F1} Second Angle:{6,7:F1} Third Angle:{7,7:F1} Residual:{8,7:F1}",
                            rtProtocol.GetRTPacket().Frame,
                            bodySetting.Name,
                            sixDofBody.Position.X, sixDofBody.Position.Y, sixDofBody.Position.Z,
                            sixDofBody.Rotation.First, sixDofBody.Rotation.Second, sixDofBody.Rotation.Third,
                            sixDofBody.Residual);
                    }
                }
            }

            // Handle event packet
            if (packetType == PacketType.PacketEvent)
            {
                // If an event comes from QTM then print it out
                var qtmEvent = rtProtocol.GetRTPacket().GetEvent();
                Console.WriteLine("{0}", qtmEvent);
            }
        }
    }
}
```
