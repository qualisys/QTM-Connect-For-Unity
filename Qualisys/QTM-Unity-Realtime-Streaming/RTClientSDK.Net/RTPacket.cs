// Realtime SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using QTMRealTimeSDK.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace QTMRealTimeSDK.Data
{
    #region Enumerations related to RTPacket

    /// <summary>Type of package sent via RT</summary>
    public enum PacketType
    {
        PacketError = 0,
        PacketCommand,
        PacketXML,
        PacketData,
        PacketNoMoreData,
        PacketC3DFile,
        PacketEvent,
        PacketDiscover,
        PacketQTMFile,
        PacketNone
    }

    /// <summary>Type of component streamed</summary>
    public enum ComponentType
    {
        // Labeled 3d markers (Get3DMarkerData)
        Component3d = 1,
        // Unidentified 3d markers (Get3DMarkerNoLabelsData)
        Component3dNoLabels = 2,
        // Analog data (GetAnalogData)
        ComponentAnalog = 3,
        // Force data (GetForceData)
        ComponentForce = 4,
        // 6D data - position and rotation matrix (Get6DOFData)
        Component6d = 5,
        // 6D data - position and Euler angles (Get6DOFEulerData)
        Component6dEuler = 6,
        // 2D marker data (Get2DMarkerData)
        Component2d = 7,
        // Linearized 2D marker data (Get2DLinearizedMarkerDAta)
        Component2dLinearized = 8,
        // Labeled 3d markers with residual (Get3DMarkerResidualData)
        Component3dResidual = 9,
        // Unidentified 3d markers with residual (Get3DMarkerNoLabelsResidualData)
        Component3dNoLabelsResidual = 10,
        // 6D data - position and rotation matrix with residuals with residual (Get6DOFResidualData)
        Component6dResidual = 11,
        // 6D data - position and Euler angles with residual (Get6DOFEulerResidualData)
        Component6dEulerResidual = 12,
        // Analog data from available analog devices. Only one sample per channel and camera frame. The latest sample is used if more than one sample is available. (GetAnalogSingleData)
        ComponentAnalogSingle = 13,
        // Image frame from a specific camera. Image size and format is set with the XML settings, see Image settings. (GetImageData)
        ComponentImage = 14,
        // Force data from available force plates. Only one sample per plate and camera frame. The latest sample is used if more than one sample is available. (GetForceSingleData)
        ComponentForceSingle = 15,
        // Gaze vector data from eye tracker (GetGazeVectorData)
        ComponentGazeVector = 16,
        // Timecode component
        ComponentTimecode = 17,
        // Skeleton component
        ComponentSkeleton = 18,
        // Nothing
        ComponentNone = 19,
    }

    /// <summary>Events sent from QTM via RT</summary>
    public enum QTMEvent
    {
        EventConnected = 1,
        EventConnectionClosed,
        EventCaptureStarted,
        EventCaptureStopped,
        EventCaptureFetchingFinished,
        EventCalibrationStarted,
        EventCalibrationStopped,
        EventRTFromFileStarted,
        EventRTFromFileStopped,
        EventWaitingForTrigger,
        EventCameraSettingsChanged,
        EventQTMShuttingDown,
        EventCaptureSaved,
        EventReprocessingStarted,
        EventReprocessingStopped,
        EventTrigger,
        EventNone
    }

    // <summary>Timecode types available from QTM</summary>
    public enum TimecodeType
    {
        SMPTE = 0,
        IRIG,
        CameraTime
    }

    #endregion

    #region Data structures related to RTPacket

    /// <summary>Data for cameras, includes 2D marker data</summary>
    public struct Camera
    {
        /// <summary>Number of markers</summary>
        public uint MarkerCount;
        /// <summary>Only first bits is used, too much light enters camera</summary>
        public byte StatusFlags;
        /// <summary>Marker data for this camera</summary>
        public Q2D[] MarkerData2D;
    }

    /// <summary>Struct for xyz coordinates</summary>
    public struct Point
    {
        [XmlElement("X")]
        public float X;
        [XmlElement("Y")]
        public float Y;
        [XmlElement("Z")]
        public float Z;
    }
    /// <summary>Struct for quaternion</summary>
    public struct QuaternionRotation
    {
        [XmlElement("X")]
        public float X;
        [XmlElement("Y")]
        public float Y;
        [XmlElement("Z")]
        public float Z;
        [XmlElement("W")]
        public float W;
    }

    /// <summary>Struct for Euler rotation</summary>
    public struct EulerRotation
    {
        public float First;
        public float Second;
        public float Third;
    }
    /// <summary>Data from a force plate, includes samples</summary>
    public struct ForcePlate
    {
        /// <summary>ID of plate</summary>
        public int PlateId;
        /// <summary>Number of forces in frame</summary>
        public int ForceCount;
        /// <summary>Force number, increased with the force frequency</summary>
        public int ForceNumber;
        /// <summary>Samples collected from plate</summary>
        public ForceSample[] ForceSamples;
    }

    /// <summary>samples for a force plat</summary>
    public struct ForceSample
    {
        /// <summary>Coordinate of the force </summary>
        public Point Force;
        /// <summary>Coordinate of the moment </summary>
        public Point Moment;
        /// <summary>Coordinate of the force application point </summary>
        public Point ApplicationPoint;
    }

    /// <summary>2D Data for markers from cameras. used by both for non- and linearized marker</summary>
    public struct Q2D
    {
        /// <summary>X coordinate of the marker</summary>
        public uint X;
        /// <summary>Y coordinate of the marker</summary>
        public uint Y;
        /// <summary>X diameter of the marker</summary>
        public ushort DiameterX;
        /// <summary>Y diameter of the marker</summary>
        public ushort DiameterY;
    }

    /// <summary>3D data for marker</summary>
    public struct Q3D
    {
        /// <summary>ID of marker, 0 of marker has label</summary>
        public uint Id;
        /// <summary>Position data for marker</summary>
        public Point Position;
        /// <summary>Residual for marker, -1 if residual was not requested</summary>
        public float Residual;
    }

    /// <summary>Data for 6DOF (6 Degrees Of Freedom) Body</summary>
    public struct Q6DOF
    {
        /// <summary>Position data for body</summary>
        public Point Position;
        /// <summary>Rotation matrix for body</summary>
        public float[] Matrix;
        /// <summary>Residual for body, -1 if residual was not requested</summary>
        public float Residual;
    }

    /// <summary>Data for 6DOF (6 Degrees Of Freedom) Body, Euler angles instead of matrix</summary>
    public struct Q6DOFEuler
    {
        /// <summary>Position data for body</summary>
        public Point Position;
        /// <summary>Euler angles for body</summary>
        public EulerRotation Rotation;
        /// <summary>Residual for body, -1 if residual was not requested</summary>
        public float Residual;
    }

    /// <summary>Data from a analog device, includes samples</summary>
    public struct Analog
    {
        /// <summary>Device ID</summary>
        public uint DeviceID;
        /// <summary>Number of channels</summary>
        public uint ChannelCount;
        /// <summary>Samples for all channels</summary>
        public AnalogChannelData[] Channels;
        /// <summary>Start sample number for analog data packet</summary>
        public uint SampleNumber;
    }

    /// <summary>Channel data from analog device</summary>
    public struct AnalogChannelData
    {
        /// <summary>Sample data for channel</summary>
        public float[] Samples;
        /// <summary>Sample number</summary>
        public uint SampleNumber;
    }

    /// <summary>Data for camera image</summary>
    public struct CameraImage
    {
        /// <summary>Id of camera image originates from</summary>
        public uint CameraID;
        /// <summary>Image format</summary>
        public ImageFormat ImageFormat;
        /// <summary>Width of image</summary>
        public uint Width;
        /// <summary>Height of image</summary>
        public uint Height;
        /// <summary>Scaled value of cropping from left</summary>
        public float LeftCrop;
        /// <summary>Scaled value of cropping from top</summary>
        public float TopCrop;
        /// <summary>Scaled value of cropping from right</summary>
        public float RightCrop;
        /// <summary>Scaled value of cropping from bottom</summary>
        public float BottomCrop;
        /// <summary>Size of image data</summary>
        public int ImageSize;
        /// <summary>Actual image data</summary>
        public byte[] ImageData;
    }

    /// <summary>Data for Gaze vector.</summary>
    public struct GazeVector
    {
        /// <summary>Gaze vector</summary>
        public Point Gaze;
        /// <summary>Gaze vector position</summary>
        public Point Position;
        /// <summary>Sample number</summary>
        public uint SampleNumber;
    }

    /// <summary>Data for Timecode.</summary>
    public struct Timecode
    {
        /// <summary>Gaze vector</summary>
        public TimecodeType Type;
        /// <summary>Gaze vector position</summary>
        public uint High;
        /// <summary>Sample number</summary>
        public uint Low;

        public override string ToString()
        {
            return this.FormatTimestamp();
        }
    }

    /// <summary> IRIG timecode struct </summary>
    public struct IRIGTimecode
    {
        public uint Year;
        public uint Day;
        public uint Hour;
        public uint Minute;
        public uint Second;
        public uint Tenth;
    }

    /// <summary> SMPTE timecode struct </summary>
    public struct SMPTETimecode
    {
        public uint Hour;
        public uint Minute;
        public uint Second;
        public uint Frame;
    }

    /// <summary>Data for Skeleton Segment</summary>
    public struct SegmentData
    {
        /// <summary>ID</summary>
        public uint Id;
        /// <summary>Skeleton position</summary>
        public Point Position;
        /// <summary>Skeleton rotation</summary>
        public QuaternionRotation Rotation;
    }
    /// <summary>Data for Skeleton</summary>
    public struct SkeletonData
    {
        // <summary>Segment data</summary>
        public List<SegmentData> SegmentDataList;
        /// <summary>Sample number</summary>
        public uint SampleNumber;
        /// <summary>ID</summary>
        public uint Id;
        /// <summary>Number of Segments</summary>
        public uint SegmentCount;
    }

    #endregion

    public class RTPacket
    {
        /// <summary>return packet with no data but only type set to error packet</summary>
        public static RTPacket ErrorPacket { get { return new RTPacket(PacketType.PacketError); } }

        int mMajorVersion;
        /// <summary>Major protocol version of packet</summary>
        public int MajorVersion { get { return mMajorVersion; } }

        int mMinorVersion;
        /// <summary>Minor protocol version of packet</summary>
        public int MinorVersion { get { return mMinorVersion; } }

        int mPacketSize;
        /// <summary>size of packet in bytes</summary>
        public int PacketSize { get { return mPacketSize; } }

        PacketType mPacketType;
        /// <summary>what type of packet</summary>
        public PacketType PacketType { get { return mPacketType; } }

        long mTimestamp;
        /// <summary>if the packet is a data packet, this will return the timestamp, otherwise -1</summary>
        public long TimeStamp { get { return mTimestamp; } }

        int mFrameNumber;
        /// <summary>if the packet is a data packet, this will return the frame number, otherwise -1</summary>
        public int Frame { get { return mFrameNumber; } }

        uint m2DDropRate;
        /// <summary>Drop rate from cameras</summary>
        public uint DropRate { get { return m2DDropRate; } }
        uint m2DOutOfSyncRate;
        /// <summary>Out of sync rate from cameras</summary>
        public uint OutOfSyncRate { get { return m2DOutOfSyncRate; } }

        /// <summary>Number of cameras</summary>
        public int CameraCount { get { return (m2DMarkerData != null) ? m2DMarkerData.Count : -1; } }
        /// <summary>Number of markers</summary>
        public int MarkerCount { get { return (m3DMarkerData != null) ? m3DMarkerData.Count : -1; } }
        /// <summary>Number of bodies</summary>
        public int BodyCount { get { return (m6DOFData != null) ? m6DOFData.Count : -1; } }

        byte[] mData;
        internal byte[] Data { get { return mData; } }

        List<Camera> m2DMarkerData;
        List<Camera> m2DLinearizedMarkerData;
        List<Q3D> m3DMarkerData;
        List<Q3D> m3DMarkerResidualData;
        List<Q3D> m3DMarkerNoLabelData;
        List<Q3D> m3DMarkerNoLabelResidualData;
        List<Q6DOF> m6DOFData;
        List<Q6DOF> m6DOFResidualData;
        List<Q6DOFEuler> m6DOFEulerData;
        List<Q6DOFEuler> m6DOFEulerResidualData;
        List<Analog> mAnalogData;
        List<Analog> mAnalogSingleData;
        List<ForcePlate> mForcePlateData;
        List<ForcePlate> mForceSinglePlateData;
        List<CameraImage> mImageData;
        List<GazeVector> mGazeVectorData;
        List<Timecode> mTimecodeData;
        List<SkeletonData> mSkeletonData;


        /// <summary>
        /// Private constructor only used for static error packet.
        /// </summary>
        /// <param name="type"></param>
        private RTPacket(PacketType type)
        {
            mPacketType = type;
        }

        /// <summary>
        /// Constructor for packet.
        /// </summary>
        /// <param name="majorVersion">Major version of packet, default is latest version.</param>
        /// <param name="minorVersion">Minor version of packet, default is latest version.</param>
        public RTPacket(int majorVersion = RTProtocol.Constants.MAJOR_VERSION, int minorVersion = RTProtocol.Constants.MINOR_VERSION)
        {
            mMajorVersion = majorVersion;
            mMinorVersion = minorVersion;

            m2DMarkerData = new List<Camera>();
            m2DLinearizedMarkerData = new List<Camera>();

            m3DMarkerData = new List<Q3D>();
            m3DMarkerResidualData = new List<Q3D>();
            m3DMarkerNoLabelData = new List<Q3D>();
            m3DMarkerNoLabelResidualData = new List<Q3D>();

            m6DOFData = new List<Q6DOF>();
            m6DOFResidualData = new List<Q6DOF>();
            m6DOFEulerData = new List<Q6DOFEuler>();
            m6DOFEulerResidualData = new List<Q6DOFEuler>();

            mAnalogData = new List<Analog>();
            mAnalogSingleData = new List<Analog>();

            mForcePlateData = new List<ForcePlate>();
            mForceSinglePlateData = new List<ForcePlate>();

            mImageData = new List<CameraImage>();
            mGazeVectorData = new List<GazeVector>();

            mTimecodeData = new List<Timecode>();
            mSkeletonData = new List<SkeletonData>();

            ClearData();
        }

        /// <summary>
        /// Get version of packet.
        /// </summary>
        /// <param name="majorVersion">Major version of packet.</param>
        /// <param name="minorVersion">Minor version of packet.</param>
        public void GetVersion(ref int majorVersion, ref int minorVersion)
        {
            majorVersion = mMajorVersion;
            minorVersion = mMinorVersion;
        }

        /// <summary>
        /// Clear packet from data.
        /// </summary>
        private void ClearData()
        {
            mData = null;
            mPacketSize = -1;
            mPacketType = PacketType.PacketNone;

            mTimestamp = -1;
            mFrameNumber = -1;

            m2DMarkerData.Clear();
            m2DLinearizedMarkerData.Clear();
            m3DMarkerData.Clear();
            m3DMarkerResidualData.Clear();
            m3DMarkerNoLabelData.Clear();
            m3DMarkerNoLabelResidualData.Clear();
            m6DOFData.Clear();
            m6DOFResidualData.Clear();
            m6DOFEulerData.Clear();
            m6DOFEulerResidualData.Clear();
            mAnalogData.Clear();
            mAnalogSingleData.Clear();
            mForcePlateData.Clear();
            mForceSinglePlateData.Clear();
            mImageData.Clear();
            mGazeVectorData.Clear();
            mTimecodeData.Clear();
            mSkeletonData.Clear();
        }

        private Object packetLock = new Object();

        #region Set packet data function
        /// <summary>
        /// Set the data of packet.
        /// </summary>
        /// <param name="data">byte data recieved from server</param>
        internal void SetData(byte[] data)
        {
            /*  === Data packet setup ===
            *  Packet size - 4 bytes
            *  packet type - 4 bytes
            *  timestamp - 8 bytes
            *  Component count - 4 bytes
            *  [for each component]
            *    Component size - 4 bytes
            *    Component type - 4 bytes
            *    Component Data - [Component size] bytes
            */

            lock (packetLock)
            {
                ClearData();
                mData = data;
                SetPacketHeader(mData);

                if (mPacketType == PacketType.PacketData)
                {
                    mTimestamp = BitConverter.ToInt64(mData, RTProtocol.Constants.PACKET_HEADER_SIZE);
                    mFrameNumber = BitConverter.ToInt32(mData, RTProtocol.Constants.PACKET_HEADER_SIZE + 8);
                    var components = BitConverter.ToInt32(mData, RTProtocol.Constants.PACKET_HEADER_SIZE + 12);
                    int position = RTProtocol.Constants.PACKET_HEADER_SIZE + RTProtocol.Constants.DATA_PACKET_HEADER_SIZE;

                    for (int component = 1; component <= components; component++)
                    {
                        ComponentType componentType = GetComponentType(position);
                        position += RTProtocol.Constants.COMPONENT_HEADER;
                        if (componentType == ComponentType.Component3d)
                        {
                            /* Marker count - 4 bytes
                            * 2D Drop rate - 2 bytes
                            * 2D Out of sync rate - 2 bytes
                            * [Repeated per marker]
                            *   X - 4 bytes
                            *   Y - 4 bytes
                            *   Z - 4 bytes
                            */
                            uint markerCount = BitConvert.GetUInt32(mData, ref position);
                            m2DDropRate = BitConvert.GetUShort(mData, ref position);
                            m2DOutOfSyncRate = BitConvert.GetUShort(mData, ref position);

                            for (int i = 0; i < markerCount; i++)
                            {
                                Q3D marker = new Q3D();
                                marker.Id = 0;
                                marker.Residual = -1;
                                marker.Position = BitConvert.GetPoint(mData, ref position);

                                m3DMarkerData.Add(marker);
                            }
                        }
                        else if (componentType == ComponentType.Component3dNoLabels)
                        {
                            /* Marker count - 4 bytes
                            * 2D Drop rate - 2 bytes
                            * 2D Out of sync rate - 2 bytes
                            * [Repeated per marker]
                            *   X - 4 bytes
                            *   Y - 4 bytes
                            *   Z - 4 bytes
                            *   ID - 4 bytes
                            */

                            int markerCount = BitConvert.GetInt32(mData, ref position);
                            m2DDropRate = BitConvert.GetUShort(mData, ref position);
                            m2DOutOfSyncRate = BitConvert.GetUShort(mData, ref position);

                            for (int i = 0; i < markerCount; i++)
                            {
                                Q3D marker = new Q3D();
                                marker.Residual = -1;
                                marker.Position = BitConvert.GetPoint(mData, ref position);
                                marker.Id = BitConvert.GetUInt32(mData, ref position);
                                m3DMarkerNoLabelData.Add(marker);
                            }

                        }
                        else if (componentType == ComponentType.ComponentAnalog)
                        {
                            /* Analog Device count - 4 bytes
                            * [Repeated per device]
                            *   Device id - 4 bytes
                            *   Channel count - 4 bytes
                            *   Sample count - 4 bytes
                            *   Sample number - 4 bytes
                            *   Analog data - 4 * channelcount * sampleCount
                            */
                            uint deviceCount = BitConvert.GetUInt32(mData, ref position);
                            for (int i = 0; i < deviceCount; i++)
                            {
                                Analog analogDeviceData = new Analog();
                                analogDeviceData.DeviceID = BitConvert.GetUInt32(mData, ref position);
                                analogDeviceData.ChannelCount = BitConvert.GetUInt32(mData, ref position);
                                analogDeviceData.Channels = new AnalogChannelData[analogDeviceData.ChannelCount];

                                uint sampleCount = BitConvert.GetUInt32(mData, ref position);
                                if (sampleCount > 0)
                                {
                                    analogDeviceData.SampleNumber = BitConvert.GetUInt32(mData, ref position);
                                    for (uint j = 0; j < analogDeviceData.ChannelCount; j++)
                                    {
                                        AnalogChannelData sample = new AnalogChannelData();
                                        sample.Samples = new float[sampleCount];
                                        for (uint k = 0; k < sampleCount; k++)
                                        {
                                            sample.SampleNumber = analogDeviceData.SampleNumber + k;
                                            sample.Samples[k] = BitConvert.GetFloat(mData, ref position);
                                        }

                                        analogDeviceData.Channels[j] = sample;
                                    }
                                }
                                mAnalogData.Add(analogDeviceData);
                            }
                        }
                        else if (componentType == ComponentType.ComponentForce)
                        {
                            /* Force plate count - 4 bytes
                            * [Repeated per plate]
                            *   Force plate ID - 4 bytes
                            *   Force count - 4 bytes
                            *   forceNumber - 4 bytes
                            *   Force data - 36 * force count bytes
                            */

                            int forcePlateCount = BitConvert.GetInt32(mData, ref position);
                            for (int i = 0; i < forcePlateCount; i++)
                            {
                                ForcePlate plate = new ForcePlate();
                                plate.PlateId = BitConvert.GetInt32(mData, ref position);
                                plate.ForceCount = BitConvert.GetInt32(mData, ref position);
                                plate.ForceNumber = BitConvert.GetInt32(mData, ref position);
                                plate.ForceSamples = new ForceSample[plate.ForceCount];

                                for (int j = 0; j < plate.ForceCount; j++)
                                {
                                    ForceSample sample;
                                    sample.Force = BitConvert.GetPoint(mData, ref position);
                                    sample.Moment = BitConvert.GetPoint(mData, ref position);
                                    sample.ApplicationPoint = BitConvert.GetPoint(mData, ref position);
                                    plate.ForceSamples[j] = sample;
                                }

                                mForcePlateData.Add(plate);
                            }

                        }
                        else if (componentType == ComponentType.Component6d)
                        {
                            /* Body count - 4 bytes
                            * 2D Drop rate - 2 bytes
                            * 2D Out of sync rate - 2 bytes
                            * [Repeated per body]
                            *   X - 4 bytes
                            *   Y - 4 bytes
                            *   Z - 4 bytes
                            *   rotation matrix - 9*4 bytes
                            */

                            int bodyCount = BitConvert.GetInt32(mData, ref position);
                            m2DDropRate = BitConvert.GetUShort(mData, ref position);
                            m2DOutOfSyncRate = BitConvert.GetUShort(mData, ref position);

                            for (int i = 0; i < bodyCount; i++)
                            {
                                Q6DOF body = new Q6DOF();
                                body.Position = BitConvert.GetPoint(mData, ref position);
                                body.Matrix = new float[9];

                                for (int j = 0; j < 9; j++)
                                {
                                    body.Matrix[j] = BitConvert.GetFloat(mData, ref position);
                                }

                                body.Residual = -1;
                                m6DOFData.Add(body);
                            }
                        }
                        else if (componentType == ComponentType.Component6dEuler)
                        {

                            /* Body count - 4 bytes
                            * 2D Drop rate - 2 bytes
                            * 2D Out of sync rate - 2 bytes
                            * [Repeated per body]
                            *   X - 4 bytes
                            *   Y - 4 bytes
                            *   Z - 4 bytes
                            *   Euler Angles - 3*4 bytes
                            */

                            int bodyCount = BitConvert.GetInt32(mData, ref position);
                            m2DDropRate = BitConvert.GetUShort(mData, ref position);
                            m2DOutOfSyncRate = BitConvert.GetUShort(mData, ref position);

                            for (int i = 0; i < bodyCount; i++)
                            {
                                Q6DOFEuler body = new Q6DOFEuler();
                                body.Position = BitConvert.GetPoint(mData, ref position);
                                body.Rotation = BitConvert.GetEulerRotation(mData, ref position);
                                body.Residual = -1;
                                m6DOFEulerData.Add(body);
                            }

                        }
                        else if (componentType == ComponentType.Component2d || componentType == ComponentType.Component2dLinearized)
                        {
                            /* Camera Count - 4 bytes
                            * 2D Drop rate - 2 bytes
                            * 2D Out of sync rate - 2 bytes
                            * [Repeated per Camera]
                            *   Marker Count - 4 bytes
                            *   Status Flags - 1 byte
                            *   [Repeated per Marker]
                            *     X - 4 Bytes
                            *     Y - 4 Bytes
                            *     Diameter X - 4 bytes
                            *     Diameter Y - 4 bytes
                            */

                            uint cameraCount = BitConvert.GetUInt32(mData, ref position);
                            m2DDropRate = BitConvert.GetUShort(mData, ref position);
                            m2DOutOfSyncRate = BitConvert.GetUShort(mData, ref position);

                            for (int i = 0; i < cameraCount; i++)
                            {
                                Camera camera = new Camera();
                                camera.MarkerCount = BitConvert.GetUInt32(mData, ref position);
                                camera.StatusFlags = mData[position++];
                                camera.MarkerData2D = new Q2D[camera.MarkerCount];
                                for (int j = 0; j < camera.MarkerCount; j++)
                                {
                                    Q2D marker = new Q2D();
                                    marker.X = BitConvert.GetUInt32(mData, ref position);
                                    marker.Y = BitConvert.GetUInt32(mData, ref position);
                                    marker.DiameterX = BitConvert.GetUShort(mData, ref position);
                                    marker.DiameterY = BitConvert.GetUShort(mData, ref position);
                                    camera.MarkerData2D[j] = marker;
                                }
                                if (componentType == ComponentType.Component2d)
                                    m2DMarkerData.Add(camera);
                                else if (componentType == ComponentType.Component2dLinearized)
                                    m2DLinearizedMarkerData.Add(camera);
                            }

                        }
                        else if (componentType == ComponentType.Component3dResidual)
                        {
                            /* Marker count - 4 bytes
                            * 2D Drop rate - 2 bytes
                            * 2D Out of sync rate - 2 bytes
                            * [Repeated per marker]
                            *   X - 4 bytes
                            *   Y - 4 bytes
                            *   Z - 4 bytes
                            *   Residual - 4 bytes
                            */
                            int markerCount = BitConvert.GetInt32(mData, ref position);
                            m2DDropRate = BitConvert.GetUShort(mData, ref position);
                            m2DOutOfSyncRate = BitConvert.GetUShort(mData, ref position);

                            for (int i = 0; i < markerCount; i++)
                            {
                                Q3D marker = new Q3D();
                                marker.Id = 0;
                                marker.Position = BitConvert.GetPoint(mData, ref position);
                                marker.Residual = BitConvert.GetFloat(mData, ref position);

                                m3DMarkerResidualData.Add(marker);
                            }
                        }
                        else if (componentType == ComponentType.Component3dNoLabelsResidual)
                        {
                            /* Marker count - 4 bytes
                                * 2D Drop rate - 2 bytes
                                * 2D Out of sync rate - 2 bytes
                                * [Repeated per marker]
                                *   X - 4 bytes
                                *   Y - 4 bytes
                                *   Z - 4 bytes
                                *   ID - 4 bytes
                                *   Residual - 4 bytes
                            */
                            int markerCount = BitConvert.GetInt32(mData, ref position);
                            m2DDropRate = BitConvert.GetUShort(mData, ref position);
                            m2DOutOfSyncRate = BitConvert.GetUShort(mData, ref position);

                            for (int i = 0; i < markerCount; i++)
                            {
                                Q3D marker = new Q3D();
                                marker.Position = BitConvert.GetPoint(mData, ref position);
                                marker.Id = BitConvert.GetUInt32(mData, ref position);
                                marker.Residual = BitConvert.GetFloat(mData, ref position);

                                m3DMarkerNoLabelResidualData.Add(marker);
                            }

                        }
                        else if (componentType == ComponentType.Component6dResidual)
                        {
                            /* Body count - 4 bytes
                            * 2D Drop rate - 2 bytes
                            * 2D Out of sync rate - 2 bytes
                            * [Repeated per marker]
                            *   X - 4 bytes
                            *   Y - 4 bytes
                            *   Z - 4 bytes
                            *   rotation matrix - 9*4 bytes
                            *   residual - 9*4 bytes
                            */

                            int bodyCount = BitConvert.GetInt32(mData, ref position);
                            m2DDropRate = BitConvert.GetUShort(mData, ref position);
                            m2DOutOfSyncRate = BitConvert.GetUShort(mData, ref position);

                            for (int i = 0; i < bodyCount; i++)
                            {
                                Q6DOF body = new Q6DOF();
                                body.Position = BitConvert.GetPoint(mData, ref position);
                                body.Matrix = new float[9];
                                for (int j = 0; j < 9; j++)
                                    body.Matrix[j] = BitConvert.GetFloat(mData, ref position);
                                body.Residual = BitConvert.GetFloat(mData, ref position); ;
                                m6DOFResidualData.Add(body);
                            }

                        }
                        else if (componentType == ComponentType.Component6dEulerResidual)
                        {

                            /* Body count - 4 bytes
                            * 2D Drop rate - 2 bytes
                            * 2D Out of sync rate - 2 bytes
                            * [Repeated per marker]
                            *   X - 4 bytes
                            *   Y - 4 bytes
                            *   Z - 4 bytes
                            *   Euler Angles - 3*4 bytes
                            *   residual - 9*4 bytes
                            */

                            int bodyCount = BitConvert.GetInt32(mData, ref position);
                            m2DDropRate = BitConvert.GetUShort(mData, ref position);
                            m2DOutOfSyncRate = BitConvert.GetUShort(mData, ref position);

                            for (int i = 0; i < bodyCount; i++)
                            {
                                Q6DOFEuler body = new Q6DOFEuler();
                                body.Position = BitConvert.GetPoint(mData, ref position);
                                body.Rotation = BitConvert.GetEulerRotation(mData, ref position);
                                body.Residual = BitConvert.GetFloat(mData, ref position);
                                m6DOFEulerResidualData.Add(body);
                            }

                        }
                        else if (componentType == ComponentType.ComponentAnalogSingle)
                        {
                            /* Analog Device count - 4 bytes
                            * [Repeated per device]
                            *   Device id - 4 bytes
                            *   Channel count - 4 bytes
                            *   Analog data - 4 * channelcount
                            */

                            int deviceCount = BitConvert.GetInt32(mData, ref position);
                            for (int i = 0; i < deviceCount; i++)
                            {
                                Analog device = new Analog();
                                device.DeviceID = BitConvert.GetUInt32(mData, ref position);
                                device.ChannelCount = BitConvert.GetUInt32(mData, ref position);
                                device.Channels = new AnalogChannelData[device.ChannelCount];
                                for (int j = 0; j < device.ChannelCount; j++)
                                {
                                    AnalogChannelData sample = new AnalogChannelData();
                                    sample.Samples = new float[1];
                                    sample.Samples[0] = BitConvert.GetFloat(mData, ref position);

                                    device.Channels[j] = sample;
                                }

                                mAnalogSingleData.Add(device);
                            }
                        }
                        else if (componentType == ComponentType.ComponentImage)
                        {
                            /* Camera count - 4 bytes
                            * [Repeated per marker]
                            *   Camera ID - 4 bytes
                            *   Image Format - 4 bytes
                            *   Width - 4 bytes
                            *   Height- 4 bytes
                            *   Left crop - 4 bytes
                            *   Top crop - 4 bytes
                            *   Right crop - 4 bytes
                            *   Bottom crop - 4 bytes
                            *   Image size- 4 bytes
                            *   Image data - [Image size bytes]
                            */

                            int cameraCount = BitConvert.GetInt32(mData, ref position);
                            for (int i = 0; i < cameraCount; i++)
                            {
                                CameraImage image = new CameraImage();
                                image.CameraID = BitConvert.GetUInt32(mData, ref position);
                                image.ImageFormat = (ImageFormat)BitConvert.GetUInt32(mData, ref position);
                                image.Width = BitConvert.GetUInt32(mData, ref position);
                                image.Height = BitConvert.GetUInt32(mData, ref position);
                                image.LeftCrop = BitConvert.GetFloat(mData, ref position);
                                image.TopCrop = BitConvert.GetFloat(mData, ref position);
                                image.RightCrop = BitConvert.GetFloat(mData, ref position);
                                image.BottomCrop = BitConvert.GetFloat(mData, ref position);
                                image.ImageSize = BitConvert.GetInt32(mData, ref position);
                                image.ImageData = new byte[image.ImageSize];
                                Array.Copy(mData, position, image.ImageData, 0, image.ImageSize);
                                position += image.ImageSize;

                                mImageData.Add(image);
                            }
                        }
                        else if (componentType == ComponentType.ComponentTimecode)
                        {
                            /* Timecode count - 4 bytes
                            * [Repeated per marker]
                            *   Timecode Type - 4 bytes
                            *   Timecode High - 4 bytes
                            *   Timecode Low - 4 bytes
                            */

                            int timecodeCount = BitConvert.GetInt32(mData, ref position);
                            for (int i = 0; i < timecodeCount; i++)
                            {
                                Timecode timecode = new Timecode();
                                timecode.Type = (TimecodeType)BitConvert.GetUInt32(mData, ref position);
                                timecode.High = BitConvert.GetUInt32(mData, ref position);
                                timecode.Low = BitConvert.GetUInt32(mData, ref position);

                                mTimecodeData.Add(timecode);
                            }
                        }
                        else if (componentType == ComponentType.ComponentForceSingle)
                        {
                            /* Force plate count - 4 bytes
                            * [Repeated per plate]
                            *   Force plate ID - 4 bytes
                            *   Force data - 36 bytes
                            */

                            int forcePlateCount = BitConvert.GetInt32(mData, ref position);
                            for (int i = 0; i < forcePlateCount; i++)
                            {
                                ForcePlate plate = new ForcePlate();
                                plate.PlateId = BitConvert.GetInt32(mData, ref position);
                                plate.ForceCount = 1;
                                plate.ForceNumber = -1;
                                plate.ForceSamples = new ForceSample[plate.ForceCount];
                                plate.ForceSamples[0].Force = BitConvert.GetPoint(mData, ref position);
                                plate.ForceSamples[0].Moment = BitConvert.GetPoint(mData, ref position);
                                plate.ForceSamples[0].ApplicationPoint = BitConvert.GetPoint(mData, ref position);

                                mForceSinglePlateData.Add(plate);
                            }
                        }
                        else if (componentType == ComponentType.ComponentGazeVector)
                        {
                            /* Gaze vector count - 4 bytes
                            * Gaze vector sample count - 4 bytes
                            * Gaze vector sample number - 4 bytes (omitted if sample count is 0)
                            * [Repeated per gaze vector (omitted if sample count is 0)]
                            *   Gaze vector data - 24 bytes
                            */

                            int gazeVectorCount = BitConvert.GetInt32(mData, ref position);
                            for (int i = 0; i < gazeVectorCount; i++)
                            {
                                GazeVector gazeVector = new GazeVector();
                                uint sampleCount = BitConvert.GetUInt32(mData, ref position);
                                if (sampleCount > 0)
                                {
                                    uint sampleNumber = BitConvert.GetUInt32(mData, ref position);
                                    gazeVector.SampleNumber = sampleNumber;
                                    for (var sample = 0; sample < sampleCount; sample++)
                                    {
                                        gazeVector.Gaze = BitConvert.GetPoint(mData, ref position);
                                        gazeVector.Position = BitConvert.GetPoint(mData, ref position);
                                    }
                                }
                                mGazeVectorData.Add(gazeVector);
                            }
                        }
                        else if (componentType == ComponentType.ComponentSkeleton)
                        {
                            int skeletonCount = BitConvert.GetInt32(mData, ref position);
                            for (int i = 0; i < skeletonCount; i++)
                            {
                                SkeletonData skeleton = new SkeletonData();

                                //skeleton.Id = BitConvert.GetUInt32(mData, ref position);
                                skeleton.SegmentCount = BitConvert.GetUInt32(mData, ref position);

                                skeleton.SegmentDataList = new List<SegmentData>();
                                for (int j = 0; j < skeleton.SegmentCount; j++)
                                {
                                    SegmentData segmentData = new SegmentData();
                                    segmentData.Id = BitConvert.GetUInt32(mData, ref position);
                                    segmentData.Position = BitConvert.GetPoint(mData, ref position);
                                    segmentData.Rotation.X = BitConvert.GetFloat(mData, ref position);
                                    segmentData.Rotation.Y = BitConvert.GetFloat(mData, ref position);
                                    segmentData.Rotation.Z = BitConvert.GetFloat(mData, ref position);
                                    segmentData.Rotation.W = BitConvert.GetFloat(mData, ref position);
                                    skeleton.SegmentDataList.Add(segmentData);
                                }
                                mSkeletonData.Add(skeleton);
                            }
                        }
                        else
                        {
                            // Any missing component types?
                        }
                    }
                }
            }
        }
        #endregion

        #region private set functions for packet header data
        /// <summary>
        /// Set this packet's header.
        /// </summary>
        private void SetPacketHeader(byte[] data)
        {
            mPacketSize = GetPacketSize(data);
            if (mPacketSize < 4)
            {
                mPacketType = PacketType.PacketNone;
                return;
            }
            mPacketType = (PacketType)BitConverter.ToInt32(mData, 4);
        }

        #endregion

        #region get functions for packet header data


        /// <summary>
        /// Get number of bytes in packet.
        /// </summary>
        /// <param name="data">bytes from packet.</param>
        /// <returns>Size of packet.</returns>
        internal static int GetPacketSize(byte[] data)
        {
            return BitConverter.ToInt32(data, 0);
        }

        /// <summary>
        /// Get the packet type of packet.
        /// </summary>
        /// <param name="data">bytes from packet.</param>
        /// <returns>packet type</returns>
        internal static PacketType GetPacketType(byte[] data)
        {
            if (data.GetLength(0) < 4)
                return PacketType.PacketNone;

            return (PacketType)BitConverter.ToInt32(data, 4);
        }

        /// <summary>
        /// Get time stamp in a data packet.
        /// </summary>
        /// <param name="data">bytes from packet.</param>
        /// <returns>time stamp from packet</returns>
        internal static long GetTimeStamp(byte[] data)
        {
            if (GetPacketType(data) == PacketType.PacketData)
            {
                return BitConverter.ToInt64(data, RTProtocol.Constants.PACKET_HEADER_SIZE);
            }
            return -1;
        }

        /// <summary>
        /// Get frame number from a data packet.
        /// </summary>
        /// <param name="data">bytes from packet.</param>
        /// <returns>frame number from packet</returns>
        internal static int GetFrameNumber(byte[] data)
        {
            if (GetPacketType(data) == PacketType.PacketData)
            {
                return BitConverter.ToInt32(data, RTProtocol.Constants.PACKET_HEADER_SIZE + 8);
            }
            return -1;
        }

        /// <summary>
        /// Get count of different component types from a datapacket
        /// </summary>
        /// <param name="data">bytes from packet.</param>
        /// <returns>number of component types in packet</returns>
        internal static int GetComponentCount(byte[] data)
        {
            if (GetPacketType(data) == PacketType.PacketData)
            {
                return BitConverter.ToInt32(data, RTProtocol.Constants.PACKET_HEADER_SIZE + 12);
            }
            return -1;
        }

        #endregion

        #region Component related get functions

        /// <summary>
        /// Get component type at position in this packet.
        /// </summary>
        /// <param name="position">position in packet where the component starts</param>
        /// <returns>Component type</returns>
        private ComponentType GetComponentType(int position)
        {
            return (ComponentType)BitConverter.ToInt32(mData, position + 4);
        }

        /// <summary>
        /// Get error string from this packet.
        /// </summary>
        /// <returns>error string, null if the packet is not an error packet.</returns>
        public string GetErrorString()
        {
            if (mPacketType == PacketType.PacketError)
                return System.Text.Encoding.ASCII.GetString(mData, RTProtocol.Constants.PACKET_HEADER_SIZE, GetPacketSize(mData) - RTProtocol.Constants.PACKET_HEADER_SIZE - 1);
            return null;
        }

        /// <summary>
        /// Get command string from this packet.
        /// </summary>
        /// <returns>command string, null if the packet is not a command packet.</returns>
        public string GetCommandString()
        {
            if (mPacketType == PacketType.PacketCommand)
            {
                return System.Text.Encoding.ASCII.GetString(mData, RTProtocol.Constants.PACKET_HEADER_SIZE, GetPacketSize(mData) - RTProtocol.Constants.PACKET_HEADER_SIZE - 1);
            }
            return null;
        }

        /// <summary>
        /// Get XML string from this packet.
        /// </summary>
        /// <returns>XML string, null if the packet is not a XML packet.</returns>
        public string GetXMLString()
        {
            if (mPacketType == PacketType.PacketXML)
            {
                return System.Text.Encoding.ASCII.GetString(mData, RTProtocol.Constants.PACKET_HEADER_SIZE, GetPacketSize(mData) - RTProtocol.Constants.PACKET_HEADER_SIZE - 1);
            }
            return null;
        }

        /// <summary>
        /// Get event type from this packet.
        /// </summary>
        /// <returns>event type</returns>
        public QTMEvent GetEvent()
        {
            if (mPacketType == PacketType.PacketEvent)
            {
                return (QTMEvent)mData[RTProtocol.Constants.PACKET_HEADER_SIZE];
            }
            return QTMEvent.EventNone;
        }

        #endregion

        #region get functions for streamed data

        /// <summary>
        /// Get 2D marker data
        /// </summary>
        /// <returns>List of all 2D marker data</returns>
        public List<Camera> Get2DMarkerData()
        {
            lock (packetLock)
            {
                return m2DMarkerData.ToList();
            }
        }

        /// <summary>
        /// Get 2D marker data at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>2D marker data</returns>
        public Camera Get2DMarkerData(int index)
        {
            lock (packetLock)
            {
                return m2DMarkerData[index];
            }
        }

        /// <summary>
        /// Get linear 2D marker data
        /// </summary>
        /// <returns>List of all linear 2D marker data</returns>
        public List<Camera> Get2DLinearizedMarkerData()
        {
            lock (packetLock)
            {
                return m2DLinearizedMarkerData;
            }
        }

        /// <summary>
        /// Get linear 2D marker data at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>linear 2D marker data</returns>
        public Camera Get2DLinearizedMarkerData(int index)
        {
            lock (packetLock)
            {
                return m2DLinearizedMarkerData[index];
            }
        }

        /// <summary>
        /// Get 3D marker data
        /// </summary>
        /// <returns>List of all 3D marker data</returns>
        public List<Q3D> Get3DMarkerData()
        {
            lock (packetLock)
            {
                return m3DMarkerData;
            }
        }

        /// <summary>
        /// Get 3D marker data at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>3D marker data</returns>
        public Q3D Get3DMarkerResidualData(int index)
        {
            lock (packetLock)
            {
                return m3DMarkerResidualData[index];
            }
        }


        /// <summary>
        /// Get 3D marker data
        /// </summary>
        /// <returns>List of all 3D marker data</returns>
        public List<Q3D> Get3DMarkerResidualData()
        {
            lock (packetLock)
            {
                return m3DMarkerResidualData.ToList();
            }
        }

        /// <summary>
        /// Get 3D marker data at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>3D marker data</returns>
        public Q3D Get3DMarkerData(int index)
        {
            lock (packetLock)
            {
                return m3DMarkerData[index];
            }
        }

        /// <summary>
        /// Get unidentified 3D marker data at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>Unidentified 3D marker data</returns>
        public Q3D Get3DMarkerNoLabelsResidualData(int index)
        {
            lock (packetLock)
            {
                return m3DMarkerNoLabelResidualData[index];
            }
        }

        /// <summary>
        /// Get unidentified 3D marker data
        /// </summary>
        /// <returns>List of all unidentified 3D marker data</returns>
        public List<Q3D> Get3DMarkerNoLabelsResidualData()
        {
            lock (packetLock)
            {
                return m3DMarkerNoLabelResidualData.ToList();
            }
        }


        /// <summary>
        /// Get unidentified 3D marker data at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>Unidentified 3D marker data</returns>
        public Q3D Get3DMarkerNoLabelsData(int index)
        {
            lock (packetLock)
            {
                return m3DMarkerNoLabelData[index];
            }
        }

        /// <summary>
        /// Get unidentified 3D marker data
        /// </summary>
        /// <returns>List of all unidentified 3D marker data</returns>
        public List<Q3D> Get3DMarkerNoLabelsData()
        {
            lock (packetLock)
            {
                return m3DMarkerNoLabelData.ToList();
            }
        }

        /// <summary>
        /// Get 6DOF data
        /// </summary>
        /// <returns>List of all 6DOF body data (orientation described with rotation matrix)</returns>
        public List<Q6DOF> Get6DOFData()
        {
            lock (packetLock)
            {
                return m6DOFData.ToList();
            }
        }

        /// <summary>
        /// Get 6DOF data of body at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>6DOF body data (orientation described with rotation matrix)</returns>
        public Q6DOF Get6DOFData(int index)
        {
            lock (packetLock)
            {
                return m6DOFData[index];
            }
        }

        /// <summary>
        /// Get 6DOF data with residual
        /// </summary>
        /// <returns>List of all 6DOF body data (orientation described with rotation matrix)</returns>
        public List<Q6DOF> Get6DOFResidualData()
        {
            lock (packetLock)
            {
                return m6DOFResidualData.ToList();
            }
        }

        /// <summary>
        /// Get 6DOF data with residual of body at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>6DOF body data (orientation described with rotation matrix)</returns>
        public Q6DOF Get6DOFResidualData(int index)
        {
            lock (packetLock)
            {
                return m6DOFResidualData[index];
            }
        }

        /// <summary>
        /// Get 6DOF data
        /// </summary>
        /// <returns>List of all 6DOF body data (orientation described with Euler angles)</returns>
        public List<Q6DOFEuler> Get6DOFEulerData()
        {
            lock (packetLock)
            {
                return m6DOFEulerData.ToList();
            }
        }

        /// <summary>
        /// Get 6DOF data of body at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>6DOF body data (orientation described with Euler angles)</returns>
        public Q6DOFEuler Get6DOFEulerData(int index)
        {
            lock (packetLock)
            {
                return m6DOFEulerData[index];
            }
        }

        /// <summary>
        /// Get 6DOF euler residual data
        /// </summary>
        /// <returns>List of all 6DOF body data (orientation described with Euler angles)</returns>
        public List<Q6DOFEuler> Get6DOFEulerResidualData()
        {
            lock (packetLock)
            {
                return m6DOFEulerResidualData.ToList();
            }
        }

        /// <summary>
        /// Get 6DOF data of body at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>6DOF body data (orientation described with Euler angles)</returns>
        public Q6DOFEuler Get6DOFEulerResidualData(int index)
        {
            lock (packetLock)
            {
                return m6DOFEulerResidualData[index];
            }
        }

        /// <summary>
        /// Get all samples from all analog devices
        /// </summary>
        /// <returns>List of analog devices containing all samples gathered this frame</returns>
        public List<Analog> GetAnalogData()
        {
            lock (packetLock)
            {
                return mAnalogData.ToList();
            }
        }

        /// <summary>
        /// Get all samples from analog device at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>Analog device containing all samples gathered this frame</returns>
        public Analog GetAnalogData(int index)
        {
            lock (packetLock)
            {
                return mAnalogData[index];
            }
        }

        /// <summary>
        /// Get sample from all analog devices (only one sample per frame)
        /// </summary>
        /// <returns>List of analog devices containing only one sample gathered this frame</returns>
        public List<Analog> GetAnalogSingleData()
        {
            lock (packetLock)
            {
                return mAnalogSingleData.ToList();
            }
        }

        /// <summary>
        /// Get sample from analog device at index (only one sample per frame)
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>Analog device containing one sample gathered this frame</returns>
        public Analog GetAnalogSingleData(int index)
        {
            lock (packetLock)
            {
                return mAnalogSingleData[index];
            }
        }

        /// <summary>
        /// Get samples from all force plates
        /// </summary>
        /// <returns>List of all force plates containing all samples gathered this frame</returns>
        public List<ForcePlate> GetForceData()
        {
            lock (packetLock)
            {
                return mForcePlateData.ToList();
            }
        }

        /// <summary>
        /// Get samples from force plate at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>Force plate containing all samples gathered this frame</returns>
        public ForcePlate GetForceData(int index)
        {
            lock (packetLock)
            {
                return mForcePlateData[index];
            }
        }

        /// <summary>
        /// Get sample from all force plates (only one sample per frame)
        /// </summary>
        /// <returns>List of all force plates containing one sample gathered this frame</returns>
        public List<ForcePlate> GetForceSingleData()
        {
            lock (packetLock)
            {
                return mForceSinglePlateData.ToList();
            }
        }

        /// <summary>
        /// Get samples from force plate at index
        /// </summary>
        /// <param name="index">index to get data from.</param>
        /// <returns>Force plate containing all samples gathered this frame</returns>
        public ForcePlate GetForceSingleData(int index)
        {
            lock (packetLock)
            {
                return mForceSinglePlateData[index];
            }
        }

        /// <summary>
        /// Get images from all cameras
        /// </summary>
        /// <returns>list of all images</returns>
        public List<CameraImage> GetImageData()
        {
            lock (packetLock)
            {
                return mImageData.ToList();
            }
        }

        /// <summary>
        /// Get image from cameras at index
        /// </summary>
        /// <param name="index">index to get data from.(not camera index!)</param>
        /// <returns>Image from index</returns>
        public CameraImage GetImageData(int index)
        {
            lock (packetLock)
            {
                return mImageData[index];
            }
        }

        /// <summary>
        /// Get timecode information from packet
        /// </summary>
        /// <returns>list of all timecodes</returns>
        public List<Timecode> GetTimecodeData()
        {
            lock (packetLock)
            {
                return mTimecodeData.ToList();
            }
        }

        /// <summary>
        /// Get timecode data at index
        /// </summary>
        /// <param name="index">index to get data from.(not camera index!)</param>
        /// <returns>Timecode from index</returns>
        public Timecode GetTimecodeData(int index = 0)
        {
            lock (packetLock)
            {
                return mTimecodeData[index];
            }
        }

        /// <summary>
        /// Get timecode type at index
        /// </summary>
        /// <param name="index">index to get data from.(not camera index!)</param>
        /// <returns>Timecode type from index</returns>
        public TimecodeType GetTimecodeType(int index = 0)
        {
            lock (packetLock)
            {
                return mTimecodeData[index].Type;
            }
        }

        /// <summary>
        /// Get gaze vectors from all cameras
        /// </summary>
        /// <returns>list of all images</returns>
        public List<GazeVector> GetGazeVectorData()
        {
            lock (packetLock)
            {
                return mGazeVectorData.ToList();
            }
        }
        /// <summary>
        /// Get gaze vector data from cameras at index
        /// </summary>
        /// <param name="index">index to get data from.(not camera index!)</param>
        /// <returns>Gaze vector from index</returns>
        public GazeVector GetGazeVectorData(int index)
        {
            lock (packetLock)
            {
                return mGazeVectorData[index];
            }
        }

        /// <summary>
        /// Get skeleton from all cameras
        /// </summary>
        /// <returns>list of all images</returns>
        public List<SkeletonData> GetSkeletonData()
        {
            lock (packetLock)
            {
                return mSkeletonData.ToList();
            }
        }
        /// <summary>
        /// Get skeleton data from cameras at index
        /// </summary>
        /// <param name="index">index to get data from.(not camera index!)</param>
        /// <returns>Gaze vector from index</returns>
        public SkeletonData GetSkeletonData(int index)
        {
            lock (packetLock)
            {
                return mSkeletonData[index];
            }
        }
        #endregion
    }
}
