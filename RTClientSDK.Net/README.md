# [Qualisys](http://www.qualisys.com) Realtime SDK for .Net

This is C# package with SDK and examples that can help Qualisys users connect and stream data from [Qualisys Track Manager](http://www.qualisys.com/products/software/qtm) in realtime.
This handles all kind of data that QTM can capture. This includes marker data, 6dof objects, user bones, analog, force and gaze vector settings and data.

* Use RTProtocol::Connect method to connect to QTM.
* Use RTProtocol::DiscoverRTServers to find out which QTM applications that you can connect to on a network.
* Use RTProtocol::StreamFrames to setup streaming from QTM (this lets you decide what to stream).
* Setup the delegate RTProtocol::RealTimeDataCallback to get a call whenever there is a new realtime frame.
* Setup the delegate RTProtocol::EventDataCallback for QTM event notifications.
* Use RTProtocol::ListenToStream to start start getting streamed data.
* Do your own stuff with the streamed data.
* Use RTProtocol::StreamFramesStop to stop streaming
* Use RTProtocol::StopStreamListen to stop listening on stream
* Use RTProtocol::Disconnect to disconnect

Here is an c# console application example on how to discover all QTM servers on the network. Connect to localhost, get labeled 3d trajectory names and stream 3d data.

```csharp
using System;
using System.Collections.Generic;
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
            if (example.ConnectAndStartStreaming("127.0.0.1"))
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(100);
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
                    Console.WriteLine("Host={0,20}\tIP Adress={1,15}\tInfo Text={2,15}\tCamera count={3,3}", discoveryResponse.HostName, discoveryResponse.IpAddress, discoveryResponse.InfoText, discoveryResponse.CameraCount);
                }

            }
            return null;
        }

        public bool ConnectAndStartStreaming(string host)
        {
            try
            {
                if (rtProtocol.Connect(host))
                {
                    rtProtocol.Get3Dsettings();
                    foreach (var label in rtProtocol.Settings3D.labels3D)
                    {
                        Console.WriteLine(label.Name);
                    }
                    Console.WriteLine("Press key to continue");
                    Console.ReadKey();
                    if (rtProtocol.StreamFrames(StreamRate.RateAllFrames, -1, false, new List<ComponentType>() { ComponentType.Component3dResidual }))
                    {
                        rtProtocol.RealTimeDataCallback += DataCallback;
                        rtProtocol.EventDataCallback += EventCallback;
                        rtProtocol.ListenToStream();
                    }
                    return true;
                }
                else
                {
                    Console.WriteLine(rtProtocol.GetErrorString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return true;
        }

        private void DataCallback(RTPacket packet)
        {
            var markerData = packet.Get3DMarkerResidualData();
            foreach (var marker in markerData)
            {
                Console.WriteLine("Id={0}\tX={1,8:F2}\tY={2,8:F2}\tZ={3,8:F2}\tResidual={4,8:F2}", marker.Id, marker.Position.X, marker.Position.Y, marker.Position.Z, marker.Residual);
            }
        }

        private void EventCallback(RTPacket packet)
        {
            var qtmEvent = packet.GetEvent();
            switch (qtmEvent)
            {
                case QTMEvent.EventConnected:
                    break;
                case QTMEvent.EventConnectionClosed:
                    break;
                case QTMEvent.EventCaptureStarted:
                    break;
                case QTMEvent.EventCaptureStopped:
                    break;
                case QTMEvent.EventCaptureFetchingFinished:
                    break;
                case QTMEvent.EventCalibrationStarted:
                    break;
                case QTMEvent.EventCalibrationStopped:
                    break;
                case QTMEvent.EventRTFromFileStarted:
                    break;
                case QTMEvent.EventRTFromFileStopped:
                    break;
                case QTMEvent.EventWaitingForTrigger:
                    break;
                case QTMEvent.EventCameraSettingsChanged:
                    break;
                case QTMEvent.EventQTMShuttingDown:
                    break;
                case QTMEvent.EventCaptureSaved:
                    break;
            }
        }
    }
}
```