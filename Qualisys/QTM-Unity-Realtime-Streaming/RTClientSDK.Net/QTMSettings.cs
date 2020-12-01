// Realtime SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using System;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using QTMRealTimeSDK.Data;

namespace QTMRealTimeSDK.Settings
{
    static public class EnumHelper
    {
        // Find [XmlEnum] attribute tag on enum items
        static public string GetXmlAttrNameFromEnumValue<T>(T enumValue)
        {
            var type = enumValue.GetType();
            var info = type.GetField(Enum.GetName(typeof(T), enumValue));
            var attributes = info.GetCustomAttributes(typeof(XmlEnumAttribute), false);
            if (attributes.Length > 0)
                return (attributes[0] as XmlEnumAttribute).Name;
            return null;
        }
        /// Convert from [XmlEnum] string to a Enum object (return defaultValue if not found)
        static public T XmlEnumStringToEnum<T>(string stringValue, T defaultValue)
        {
            foreach (T t in (T[])Enum.GetValues(typeof(T)))
            {
                if (GetXmlAttrNameFromEnumValue(t) == stringValue)
                {
                    return t;
                }
            }
            return defaultValue;
        }
        // Convert from Enum type to [XmlEnum] string (return defaultValue if not found)
        static public string EnumToXmlEnumString<T>(T enumValue)
        {
            var name = GetXmlAttrNameFromEnumValue(enumValue);
            if (name != null)
                return name;
            throw new ArgumentException("There is no XmlEnum attribute for this enum value");
        }
    }

    public class SettingsBase
    {
        [XmlIgnore]
        public string Xml;
    }

    /// <summary>General Settings from QTM</summary>
    [XmlRoot("General")]
    public class SettingsGeneral : SettingsBase
    {
        /// <summary>QTM Capture frequency </summary>
        [XmlElement("Frequency")]
        public int CaptureFrequency;

        /// <summary>length of QTM Capture. Time expressed in seconds</summary>
        [XmlElement("Capture_Time")]
        public float CaptureTime;

        /// <summary>Measurement start on any external trigger (qtm version 2.13 or less, or 2.14 running Oqus systems)</summary>
        [XmlElement("Start_On_External_Trigger")]
        public bool StartOnExternalTrigger;

        /// <summary>Measurement start on MSU Trig NO/Oqus Trig in</summary>
        [XmlElement("Start_On_Trigger_NO")]
        public bool StartOnTrigNO;

        /// <summary>Measurement start on MSU Trig NC</summary>
        [XmlElement("Start_On_Trigger_NC")]
        public bool StartOnTrigNC;

        /// <summary>Measurement start on software trigger (app, keyboard etc)</summary>
        [XmlElement("Start_On_Trigger_Software")]
        public bool StartOnTrigSoftware;

        [XmlElement("External_Time_Base")]
        public SettingsExternalTimeBase ExternalTimebase;

        [XmlElement("External_Timestamp")]
        public SettingsExternalTimestamp ExternalTimestamp;

        [XmlElement("Processing_Actions")]
        public SettingProcessingActions ProcessingActions;

        [XmlElement("RealTime_Processing_Actions")]
        public SettingProcessingActions RealtimeProcessingActions;

        [XmlElement("Reprocessing_Actions")]
        public SettingProcessingActions ReprocessingActions;

        [XmlElement("EulerAngles")]
        public SettingsEulerAngles EulerAngles;

        /// <summary>Camera Settings </summary>
        [XmlElement("Camera")]
        public List<SettingsGeneralCameraSystem> CameraSettings;

        public SettingsGeneral()
        {
        }
    }

    /// <summary>3D Bone Settings from QTM</summary>
    public class SettingsBone : SettingsBase
    {
        /// <summary>name of marker bone starts from </summary>
        [XmlAttribute("From")]
        public string From;

        /// <summary>name of marker bone ends at</summary>
        [XmlAttribute("To")]
        public string To;

        /// <summary>Color of marker bone</summary>
        [XmlAttribute("Color")]
        public int Color;

        SettingsBone()
        {
            Color = 0xEEEEEE;
        }
    }

    /// <summary>3D Settings from QTM</summary>
    [XmlRoot("The_3D")]
    public class Settings3D : SettingsBase
    {
        [XmlIgnore]
        public Axis AxisUpwards;
        [XmlElement("AxisUpwards")]
        public string ModelAsString
        {
            get
            {
                return EnumHelper.EnumToXmlEnumString(AxisUpwards);
            }
            set
            {
                AxisUpwards = EnumHelper.XmlEnumStringToEnum(value, Axis.Unknown);
            }
        }

        [XmlElement("CalibrationTime")]
        public string CalibrationTime;
        [XmlElement("Labels")]
        public int LabelCount;
        [XmlElement("Label")]
        public List<Settings3DLabel> Labels;

        [XmlArray("Bones")]
        [XmlArrayItem("Bone", typeof(SettingsBone))]
        public SettingsBone[] Bones;

        public Settings3D()
        {
        }
    }

    /// <summary>6D Settings from QTM</summary>
    [XmlRoot("The_6D")]
    public class Settings6D_V1 : SettingsBase
    {
        internal static Settings6D ConvertToSettings6DOF(Settings6D_V1 settings)
        {
            return new Settings6D(settings.Xml, settings.BodyCount, settings.Bodies.ConvertAll<Settings6DOF>(Settings6DOF_V1.ConvertToSettings6DOF), settings.EulerNames);
        }

        [XmlElement("Bodies")]
        public int BodyCount;
        [XmlElement("Body")]
        public List<Settings6DOF_V1> Bodies;
        [XmlElement("Euler")]
        public EulerNames EulerNames;
    }

    [XmlRoot("Euler")]
    public class EulerNames
    {
        public EulerNames()
        {
            First = "Roll";
            Second = "Pitch";
            Third = "Yaw";
        }

        [XmlElement("First")]
        public string First;
        [XmlElement("Second")]
        public string Second;
        [XmlElement("Third")]
        public string Third;
    }

    /// <summary>6D Settings from QTM</summary>
    [XmlRoot("The_6D")]
    public class Settings6D_V2 : SettingsBase
    {
        public Settings6D_V2() { }
        internal Settings6D_V2(Settings6D settings)
        {
            Bodies = settings.Bodies.ConvertAll<Settings6DOF_V2>(Settings6DOF.ConvertToSettings6DOF_V2);
        }

        internal static Settings6D ConvertToSettings6DOF(Settings6D_V2 settings)
        {
            return new Settings6D(settings.Xml, settings.Bodies.Count, settings.Bodies.ConvertAll<Settings6DOF>(Settings6DOF_V2.ConvertToSettings6DOF), new EulerNames());
        }
        [XmlElement("Body")]
        public List<Settings6DOF_V2> Bodies;
    }

    public class Settings6D : SettingsBase
    {
        public Settings6D()
        {
#pragma warning disable CS0618 // Type or member is obsolete 
            EulerNames = new EulerNames();
#pragma warning restore CS0618 // Type or member is obsolete 
        }
        public Settings6D(string xml, int bodyCount, List<Settings6DOF> bodies, EulerNames eulerNames)
        {
            Xml = xml;
            BodyCount = bodyCount;
            Bodies = bodies;
#pragma warning disable CS0618 // Type or member is obsolete 
            EulerNames = eulerNames;
#pragma warning restore CS0618 // Type or member is obsolete 
        }

        public int BodyCount;
        public List<Settings6DOF> Bodies;
        [Obsolete("EulerNames is moved to general settings from protocol version 1.21.", false)]
        public EulerNames EulerNames;
    }

    /// <summary>Analog Settings from QTM</summary>
    [XmlRoot("Analog")]
    public class SettingsAnalog : SettingsBase
    {
        [XmlElement("Device")]
        public List<AnalogDevice> Devices;

        public SettingsAnalog()
        {

        }
    }

    /// <summary>Force Settings from QTM</summary>
    [XmlRoot("Force")]
    public class SettingsForce : SettingsBase
    {
        [XmlElement("Unit_Length")]
        public string UnitLength;
        [XmlElement("Unit_Force")]
        public string UnitForce;
        [XmlElement("Plate")]
        public List<ForcePlateSettings> Plates;

        public SettingsForce() { }
    }

    /// <summary>Image Settings from QTM</summary>
    [XmlRoot("Image")]
    public class SettingsImage : SettingsBase
    {
        [XmlElement("Camera")]
        public List<ImageCamera> Cameras;
    }

    /// <summary>Gaze vector</summary>
    public class SettingGazeVector
    {
        [XmlElement("Name")]
        public string Name;
        [XmlElement("Frequency")]
        public float Frequency;
    }

    /// <summary>Gaze vector Settings from QTM</summary>
    [XmlRoot("Gaze_Vector")]
    public class SettingsGazeVectors : SettingsBase
    {
        [XmlElement("Vector")]
        public List<SettingGazeVector> GazeVectors;
    }

    /// <summary>Eye tracker</summary>
    public class SettingEyeTracker
    {
        [XmlElement("Name")]
        public string Name;
        [XmlElement("Frequency")]
        public float Frequency;
    }

    /// <summary>Eye tracker Settings from QTM</summary>
    [XmlRoot("Eye_Tracker")]
    public class SettingsEyeTrackers : SettingsBase
    {
        [XmlElement("Device")]
        public List<SettingEyeTracker> EyeTrackers;
    }

    /// <summary>Position</summary>
    public class Position
    {
        [XmlAttribute("X")]
        public float X;
        [XmlAttribute("Y")]
        public float Y;
        [XmlAttribute("Z")]
        public float Z;
    }

    /// <summary>Rotation</summary>
    public class Rotation
    {
        [XmlAttribute("X")]
        public float X;
        [XmlAttribute("Y")]
        public float Y;
        [XmlAttribute("Z")]
        public float Z;
        [XmlAttribute("W")]
        public float W;
    }

    /// <summary>Transform</summary>
    public class Transform
    {
        [XmlElement("Position")]
        public Position Position;
        [XmlElement("Rotation")]
        public Rotation Rotation;
    }

    /// <summary>DefaultTransform</summary>
    public class DefaultTransform
    {
        [XmlElement("Position")]
        public Position Position;
        [XmlElement("Rotation")]
        public Rotation Rotation;
    }

    /// <summary>Boundary</summary>
    public class Boundary
    {
        [XmlAttribute("LowerBound")]
        public float LowerBound;
        [XmlAttribute("UpperBound")]
        public float UpperBound;
    }

    /// <summary>DegreesOfFreedom</summary>
    public class DegreesOfFreedom
    {
        [XmlElement("RotationX")]
        public Boundary RotationX;
        [XmlElement("RotationY")]
        public Boundary RotationY;
        [XmlElement("RotationZ")]
        public Boundary RotationZ;
        [XmlElement("TranslationX")]
        public Boundary TranslationX;
        [XmlElement("TranslationY")]
        public Boundary TranslationY;
        [XmlElement("TranslationZ")]
        public Boundary TranslationZ;
    }

    /// <summary>Marker</summary>
    public class Marker
    {
        [XmlAttribute("Name")]
        public string Name;
        [XmlElement("Position")]
        public Position Position;
        [XmlElement("Weight")]
        public float Weight;
    }

    /// <summary>Markers</summary>
    public class Markers
    {
        [XmlElement("Marker")]
        public List<Marker> MarkerList;
    }

    /// <summary>RigidBody</summary>
    public class RigidBody
    {
        [XmlAttribute("Name")]
        public string Name;
        [XmlElement("Transform")]
        public Transform Transform;
        [XmlElement("Weight")]
        public float Weight;
    }

    /// <summary>RigidBodies</summary>
    public class RigidBodies
    {
        [XmlElement("RigidBody")]
        public List<RigidBody> RigidBodyList;
    }

    /// <summary>Skeleton segment</summary>
    public class SettingSkeletonSegmentHierarchical
    {
        [XmlAttribute("Name")]
        public string Name;
        [XmlAttribute("ID")]
        public uint Id;
        [XmlElement("Transform")]
        public Transform Transform;
        [XmlElement("DefaultTransform")]
        public DefaultTransform DefaultTransform;
        [XmlElement("DegreesOfFreedom")]
        public DegreesOfFreedom DegreesOfFreedom;
        [XmlElement("Endpoint")]
        public Position Endpoint;
        [XmlElement("Markers")]
        public Markers Markers;
        [XmlElement("RigidBodies")]
        public RigidBodies RigidBodies;
        [XmlElement("Segment")]
        public List<SettingSkeletonSegmentHierarchical> Segments;
    }

    /// <summary>Skeleton segments</summary>
    public class SegmentsHierarchical
    {
        [XmlElement("Segment")]
        public List<SettingSkeletonSegmentHierarchical> Segments;
    }


    /// <summary>Skeleton</summary>
    public class SettingSkeletonHierarchical
    {
        [XmlAttribute("Name")]
        public string Name;
        [XmlElement("Solver")]
        public string Solver;
        [XmlElement("Scale")]
        public string Scale;
        [XmlElement("Segments")]
        public SegmentsHierarchical Segments;
    }

    /// <summary>
    /// Skeleton Settings from QTM.
    /// The skeleton is stored hierarchicaly.
    /// </summary>
    [XmlRoot("Skeletons")]
    public class SettingsSkeletonsHierarchical : SettingsBase
    {
        [XmlElement("Skeleton")]
        public List<SettingSkeletonHierarchical> Skeletons;
    }

    /// <summary>Skeleton segment</summary>
    public class SettingSkeletonSegment
    {
        [XmlAttribute("Name")]
        public string Name;
        [XmlAttribute("ID")]
        public uint Id;
        [XmlAttribute("Parent_ID")]
        public uint ParentId;
        [XmlElement("Position")]
        public Position Position;
        [XmlElement("Rotation")]
        public Rotation Rotation;
    }

    /// <summary>Skeleton</summary>
    public class SettingSkeleton
    {
        [XmlAttribute("Name")]
        public string Name;
        [XmlElement("Segment")]
        public List<SettingSkeletonSegment> Segments;
    }

    /// <summary>
    /// Skeleton Settings from QTM.
    /// The skeleton is stored in a vector.
    /// </summary>
    [XmlRoot("Skeletons")]
    public class SettingsSkeletons : SettingsBase
    {
        [XmlElement("Skeleton")]
        public List<SettingSkeleton> Skeletons;
    }

    /// <summary>General settings for Camera System</summary>
    public struct SettingsGeneralCameraSystem
    {
        /// <summary>ID of camera</summary>
        [XmlElement("ID")]
        public int CameraId;

        /// <summary>Model of camera</summary>
        [XmlIgnore]
        public CameraModel Model;
        [XmlElement("Model")]
        public string ModelAsString
        {
            get
            {
                return EnumHelper.EnumToXmlEnumString(Model);
            }
            set
            {
                Model = EnumHelper.XmlEnumStringToEnum(value, CameraModel.Unknown);
            }
        }

        /// <summary>If the camera is an underwater camera</summary>
        [XmlElement("UnderWater")]
        public bool UnderWater;
        /// <summary>If the camera supports hardware sync (like Oqus and Miqus Sync Units)</summary>
        [XmlElement("Supports_HW_Sync")]
        public bool SupportsHardwareSync;
        /// <summary>Serial number of the selected camera</summary>
        [XmlElement("Serial")]
        public int Serial;

        /// <summary>Camera mode the camera is set to</summary>
        [XmlIgnore]
        public CameraMode Mode;
        [XmlElement("Mode")]
        public string ModeAsString
        {
            get
            {
                return EnumHelper.EnumToXmlEnumString(Mode);
            }
            set
            {
                Mode = EnumHelper.XmlEnumStringToEnum(value, CameraMode.Unknown);
            }
        }


        /// <summary>Values for camera video mode, current, min and max</summary>
        [XmlElement("Video_Frequency")]
        public int VideoFrequency;
        /// <summary>Values for camera video exposure, current, min and max</summary>
        [XmlElement("Video_Exposure")]
        public CameraSetting VideoExposure;
        /// <summary>Values for camera video flash time, current, min and max</summary>
        [XmlElement("Video_Flash_Time")]
        public CameraSetting VideoFlashTime;
        /// <summary>Values for camera marker exposure, current, min and max</summary>
        [XmlElement("Marker_Exposure")]
        public CameraSetting MarkerExposure;
        /// <summary>Values for camera marker threshold, current, min and max</summary>
        [XmlElement("Marker_Threshold")]
        public CameraSetting MarkerThreshold;
        /// <summary>Position of camera</summary>
        [XmlElement("Position")]
        public CameraPosition Position;
        /// <summary>Orientation of camera</summary>
        [XmlElement("Orientation")]
        public int Orientation;
        /// <summary>Marker resolution of camera, width and height</summary>
        [XmlElement("Marker_Res")]
        public FieldOfViewSize MarkerFieldOfViewSize;
        /// <summary>Video resolution of camera, width and height</summary>
        [XmlElement("Video_Res")]
        public FieldOfViewSize VideoFieldOfViewSize;
        /// <summary>Marker Field Of View, left, top, right and bottom coordinates</summary>
        [XmlElement("Marker_FOV")]
        public FieldOfView MarkerFOV;
        /// <summary>Video Field Of View, left, top, right and bottom coordinates</summary>
        [XmlElement("Video_FOV")]
        public FieldOfView VideoFOV;
        /// <summary>Sync out settings for Oqus sync out or Sync Unit Out1</summary>
        [XmlElement("Sync_Out")]
        public SettingsSyncOut SyncOut;
        /// <summary>Sync out settings for Sync Unit Out2</summary>
        [XmlElement("Sync_Out2")]
        public SettingsSyncOut SyncOut2;
        /// <summary>Sync out settings for Sync Unit Measurement Time (MT)</summary>
        [XmlElement("Sync_Out_MT")]
        public SettingsSyncOut SyncOutMT;
        /// <summary>Lens Control settings for camera equipped with motorized lens</summary>
        [XmlElement("LensControl")]
        public SettingsLensControl LensControl;
        /// <summary>Auto exposure settings for video camera</summary>
        [XmlElement("AutoExposure")]
        public SettingsAutoExposure AutoExposure;

        /// <summary>Video resolution for non-marker cameras</summary>
        [XmlIgnore]
        public SettingsVideoResolution VideoResolution;
        [XmlElement("Video_Resolution")]
        public string VideoResolutionAsString
        {
            get
            {
                return EnumHelper.EnumToXmlEnumString(VideoResolution);
            }
            set
            {
                VideoResolution = EnumHelper.XmlEnumStringToEnum(value, SettingsVideoResolution.Unknown);
            }
        }

        /// <summary>Video aspect ratio for non-marker cameras</summary>
        [XmlIgnore]
        public SettingsVideoAspectRatio VideoAspectRatio;
        [XmlElement("Video_Aspect_Ratio")]
        public string VideoAspectRatioAsString
        {
            get
            {
                return EnumHelper.EnumToXmlEnumString(VideoAspectRatio);
            }
            set
            {
                VideoAspectRatio = EnumHelper.XmlEnumStringToEnum(value, SettingsVideoAspectRatio.Unknown);
            }
        }
    }

    /// <summary>Settings regarding Lens Control for camera equipped with motorized lens</summary>
    public struct SettingsLensControl
    {
        /// <summary>Camera focus lens control</summary>
        [XmlElement("Focus")]
        public SettingsLensControlValues Focus;
        /// <summary>Camera aperture lens control</summary>
        [XmlElement("Aperture")]
        public SettingsLensControlValues Aperture;
    }

    /// <summary>Settings regarding camera auto exposure</summary>
    public struct SettingsAutoExposure
    {
        [XmlAttribute("Enabled")]
        public bool Enabled;
        [XmlAttribute("Compensation")]
        public float Compensation;
    }

    /// <summary>Settings for Lens Control Focus</summary>
    public struct SettingsLensControlValues
    {
        [XmlAttribute("Value")]
        public float Value;
        [XmlAttribute("Min")]
        public float Min;
        [XmlAttribute("Max")]
        public float Max;
    }

    /// <summary>Settings regarding sync for Camera</summary>
    public struct SettingsSyncOut
    {
        /// <summary>Sync mode for camera</summary>
        [XmlIgnore]
        public SyncOutFrequencyMode SyncMode;
        [XmlElement("Mode")]
        public string SyncModeAsString
        {
            get
            {
                return EnumHelper.EnumToXmlEnumString(SyncMode);
            }
            set
            {
                SyncMode = EnumHelper.XmlEnumStringToEnum(value, SyncOutFrequencyMode.Unknown);
            }
        }

        /// <summary>Sync value, depending on mode</summary>
        [XmlElement("Value")]
        public int SyncValue;
        /// <summary>Output duty cycle in percent</summary>
        [XmlElement("Duty_Cycle")]
        public float DutyCycle;

        /// <summary>TTL signal polarity. no used in SRAM or 100Hz mode</summary>
        [XmlIgnore]
        public SignalPolarity SignalPolarity;
        [XmlElement("Signal_Polarity")]
        public string SignalPolarityAsString
        {
            get
            {
                return EnumHelper.EnumToXmlEnumString(SignalPolarity);
            }
            set
            {
                SignalPolarity = EnumHelper.XmlEnumStringToEnum(value, SignalPolarity.Unknown);
            }
        }
    }

    /// <summary>Position for a camera</summary>
    public struct CameraPosition
    {
        /// <summary>X position</summary>
        [XmlElement("X")]
        public float X;
        /// <summary>Y position</summary>
        [XmlElement("Y")]
        public float Y;
        /// <summary>Z position</summary>
        [XmlElement("Z")]
        public float Z;
        /// <summary>Rotation matrix - [1,1] value</summary>
        [XmlElement("Rot_1_1")]
        public float Rot11;
        /// <summary>Rotation matrix - [2,1] value</summary>
        [XmlElement("Rot_2_1")]
        public float Rot21;
        /// <summary>Rotation matrix - [3,1] value</summary>
        [XmlElement("Rot_3_1")]
        public float Rot31;
        /// <summary>Rotation matrix - [1,2] value</summary>
        [XmlElement("Rot_1_2")]
        public float Rot12;
        /// <summary>Rotation matrix - [2,2] value</summary>
        [XmlElement("Rot_2_2")]
        public float Rot22;
        /// <summary>Rotation matrix - [3,2] value</summary>
        [XmlElement("Rot_3_2")]
        public float Rot32;
        /// <summary>Rotation matrix - [1,3] value</summary>
        [XmlElement("Rot_1_3")]
        public float Rot13;
        /// <summary>Rotation matrix - [2,3] value</summary>
        [XmlElement("Rot_2_3")]
        public float Rot23;
        /// <summary>Rotation matrix - [3,3] value</summary>
        [XmlElement("Rot_3_3")]
        public float Rot33;
    }

    /// <summary>Resolution (width/height)</summary>
    public struct FieldOfViewSize
    {
        /// <summary>Width</summary>
        [XmlElement("Width")]
        public int Width;
        /// <summary>Height</summary>
        [XmlElement("Height")]
        public int Height;
    }

    /// <summary>Field of View</summary>
    public struct FieldOfView
    {
        /// <summary>Left</summary>
        [XmlElement("Left")]
        public int Left;
        /// <summary>Top</summary>
        [XmlElement("Top")]
        public int Top;
        /// <summary>Right</summary>
        [XmlElement("Right")]
        public int Right;
        /// <summary>Bottom</summary>
        [XmlElement("Bottom")]
        public int Bottom;
    }

    /// <summary>Settings for Camera values (min,max and current)</summary>
    public struct CameraSetting
    {
        /// <summary>Current value</summary>
        [XmlElement("Current")]
        public int Current;
        /// <summary>Minimum value</summary>
        [XmlElement("Min")]
        public int Min;
        /// <summary>Maximum value</summary>
        [XmlElement("Max")]
        public int Max;
    }

    /// <summary>Settings regarding post processing actions</summary>
    public struct SettingProcessingActions
    {
        /// <summary>Preprocessing 2d data</summary>
        [XmlElement("PreProcessing2D")]
        public bool PreProcessing2D;

        /// <summary>Tracking processing</summary>
        [XmlIgnore]
        public SettingsTrackingProcessingAction TrackingAction;
        [XmlElement("Tracking")]
        public string TrackingActionsAsString
        {
            get
            {
                return EnumHelper.EnumToXmlEnumString(TrackingAction);
            }
            set
            {
                TrackingAction = EnumHelper.XmlEnumStringToEnum(value, SettingsTrackingProcessingAction.Unknown);
            }
        }

        /// <summary>Twin system merge processing</summary>
        [XmlElement("TwinSystemMerge")]
        public bool TwinSystemMerge;
        /// <summary>Gapfill processing</summary>
        [XmlElement("SplineFill")]
        public bool SplineFill;
        /// <summary>AIM processing</summary>
        [XmlElement("AIM")]
        public bool Aim;
        /// <summary>6DOF tracking processing</summary>
        [XmlElement("Track6DOF")]
        public bool Track6DOF;
        /// <summary>Force data</summary>
        [XmlElement("ForceData")]
        public bool ForceData;
        /// <summary>GazeVector</summary>
        [XmlElement("GazeVector")]
        public bool GazeVector;
        /// <summary>Export to TSV</summary>
        [XmlElement("ExportTSV")]
        public bool ExportTSV;
        /// <summary>Export to C3D</summary>
        [XmlElement("ExportC3D")]
        public bool ExportC3D;
        /// <summary>Export to Matlab file</summary>
        [XmlElement("ExportMatlabFile")]
        public bool ExportMatlab;
        /// <summary>Export to AVI</summary>
        [XmlElement("ExportAviFile")]
        public bool ExportAviFile;
        /// <summary>Export to FBX</summary>
        [XmlElement("ExportFbx")]
        public bool ExportFbx;
        /// <summary>Start Program</summary>
        [XmlElement("StartProgram")]
        public bool StartProgram;
        /// <summary>Solve skeletons</summary>
        [XmlElement("SkeletonSolve")]
        public bool SkeletonSolve;
    }

    /// <summary>Settings regarding external Time Base</summary>
    public struct SettingsExternalTimeBase
    {
        [XmlElement("Enabled")]
        public bool Enabled;

        [XmlIgnore]
        public SignalSource SignalSource;
        [XmlElement("Signal_Source")]
        public string SignalSourceAsString
        {
            get
            {
                return EnumHelper.EnumToXmlEnumString(SignalSource);
            }
            set
            {
                SignalSource = EnumHelper.XmlEnumStringToEnum(value, SignalSource.Unknown);
            }
        }

        [XmlIgnore]
        public SignalMode SignalMode;
        [XmlElement("Signal_Mode")]
        public string SignalModeAsString
        {
            get
            {
                return EnumHelper.EnumToXmlEnumString(SignalMode);
            }
            set
            {
                SignalMode = EnumHelper.XmlEnumStringToEnum(value, SignalMode.Unknown);
            }
        }

        [XmlElement("Frequency_Multiplier")]
        public int FreqMultiplier;
        [XmlElement("Frequency_Divisor")]
        public int FreqDivisor;
        [XmlElement("Frequency_Tolerance")]
        public int FreqTolerance;
        [XmlElement("Nominal_Frequency")]
        public float NominalFrequency;

        [XmlIgnore]
        public SignalEdge SignalEdge;
        [XmlElement("Signal_Edge")]
        public string SignalEdgeAsString
        {
            get
            {
                return EnumHelper.EnumToXmlEnumString(SignalEdge);
            }
            set
            {
                SignalEdge = EnumHelper.XmlEnumStringToEnum(value, SignalEdge.Unknown);
            }
        }

        [XmlElement("Signal_Shutter_Delay")]
        public int SignalShutterDelay;
        [XmlElement("Non_Periodic_Timeout")]
        public float NonPeriodicTimeout;
    }
  
    public struct SettingsEulerAngles
    {
        [XmlAttribute("First")]
        public string First;
        [XmlAttribute("Second")]
        public string Second;
        [XmlAttribute("Third")]
        public string Third;
    }
  
    /// <summary>Settings regarding external time stamp</summary>
    public struct SettingsExternalTimestamp
    {
        [XmlElement("Enabled")]
        public bool Enabled;

        [XmlIgnore]
        public TimestampType Type;
        [XmlElement("Type")]
        public string TimestampTypeAsString
        {
            get
            {
                return EnumHelper.EnumToXmlEnumString(Type);
            }
            set
            {
                Type = EnumHelper.XmlEnumStringToEnum(value, TimestampType.Unknown);
            }
        }

        [XmlElement("Frequency")]
        public int Frequency;
    }

    /// <summary>Struct for 6dof point information</summary>
    public struct Settings6DOFPoint_V1
    {
        internal static Settings6DOFPoint ConvertToSettingsPoint(Settings6DOFPoint_V1 settingsPoint)
        {
            return new Settings6DOFPoint("", settingsPoint.X, settingsPoint.Y, settingsPoint.Z, settingsPoint.Virtual, settingsPoint.PhysicalId);
        }
        [XmlElement("X")]
        public float X;
        [XmlElement("Y")]
        public float Y;
        [XmlElement("Z")]
        public float Z;
        [XmlElement("PhysicalId")]
        public int PhysicalId;
        [XmlElement("Virtual")]
        public bool Virtual;
    }

    /// <summary>Settings for 6DOF bodies</summary>
    public struct Settings6DOF_V1
    {
        internal static Settings6DOF ConvertToSettings6DOF(Settings6DOF_V1 settings6DOF)
        {
            return new Settings6DOF(settings6DOF.Name, settings6DOF.ColorRGB, 0, 0, 0, new Settings6DOFFilter(), new Settings6DOFMesh(),
                settings6DOF.Points.ConvertAll<Settings6DOFPoint>(Settings6DOFPoint_V1.ConvertToSettingsPoint), new Settings6DOFDataOrigin(), new Settings6DOFDataOrientation());
        }
        /// <summary>Name of 6DOF body</summary>
        [XmlElement("Name")]
        public string Name;
        /// <summary>Color of 6DOF body</summary>
        [XmlElement("RGBColor")]
        public int ColorRGB;
        /// <summary>List of points in 6DOF body</summary>
        [XmlElement("Point")]
        public List<Settings6DOFPoint_V1> Points;
    }

    /// <summary>Struct for 6dof filter</summary>
    public struct Settings6DOFColor_V2
    {
        internal Settings6DOFColor_V2(int ColorRGB)
        {
            R = (ColorRGB & 0xff);
            G = ((ColorRGB >> 8) & 0xff);
            B = ((ColorRGB >> 16) & 0xff);
        }
        [XmlAttribute("R")]
        public int R;
        [XmlAttribute("G")]
        public int G;
        [XmlAttribute("B")]
        public int B;
    }

    /// <summary>Struct for 6dof filter</summary>
    public struct Settings6DOFFilter
    {
        [XmlAttribute("Preset")]
        public string Preset;
    }

    /// <summary>Struct for 6dof mesh position</summary>
    public struct Settings6DOFMeshPosition
    {
        [XmlAttribute("X")]
        public float X;
        [XmlAttribute("Y")]
        public float Y;
        [XmlAttribute("Z")]
        public float Z;
    }

    /// <summary>Struct for 6dof mesh rotation</summary>
    public struct Settings6DOFMeshRotation
    {
        [XmlAttribute("X")]
        public float X;
        [XmlAttribute("Y")]
        public float Y;
        [XmlAttribute("Z")]
        public float Z;
    }

    /// <summary>Struct for 6dof mesh</summary>
    public struct Settings6DOFMesh
    {
        [XmlElement("Name")]
        public string Name;
        [XmlElement("Position")]
        public Settings6DOFMeshPosition Position;
        [XmlElement("Rotation")]
        public Settings6DOFMeshRotation Rotation;
        [XmlElement("Scale")]
        public float Scale;
        [XmlElement("Opacity")]
        public float Opacity;
    }

    /// <summary>Struct for 6dof data origin</summary>
    public struct Settings6DOFDataOrigin
    {
        [XmlText]
        public string Type;
        [XmlElement("X")]
        public float X;
        [XmlElement("Y")]
        public float Y;
        [XmlElement("Z")]
        public float Z;
        [XmlElement("RelativeBody")]
        public int RelativeBody;
    }

    /// <summary>Struct for 6dof data orientation</summary>
    public struct Settings6DOFDataOrientation
    {
        [XmlText]
        public string Type;
        [XmlElement("R11")]
        public float R11;
        [XmlElement("R12")]
        public float R12;
        [XmlElement("R13")]
        public float R13;
        [XmlElement("R21")]
        public float R21;
        [XmlElement("R22")]
        public float R22;
        [XmlElement("R23")]
        public float R23;
        [XmlElement("R31")]
        public float R31;
        [XmlElement("R32")]
        public float R32;
        [XmlElement("R33")]
        public float R33;
    }

    public struct Settings6DOF_V2
    {
        internal Settings6DOF_V2(Settings6DOF settings)
        {
            Name = settings.Name;
            Color = new Settings6DOFColor_V2(settings.ColorRGB);
            MaximumResidual = settings.MaximumResidual;
            MinimumMarkersInBody = settings.MinimumMarkersInBody;
            BoneLengthTolerance = settings.BoneLengthTolerance;
            Filter = settings.Filter;
            Mesh = settings.Mesh;
            Points = settings.Points;
            DataOrigin = settings.DataOrigin;
            DataOrientation = settings.DataOrientation;
        }
        internal static Settings6DOF ConvertToSettings6DOF(Settings6DOF_V2 settings6DOF)
        {
            int colorRGB = (settings6DOF.Color.R & 0xff) | ((settings6DOF.Color.G << 8) & 0xff00) | ((settings6DOF.Color.B << 16) & 0xff0000);
            return new Settings6DOF(settings6DOF.Name, colorRGB, settings6DOF.MaximumResidual, settings6DOF.MinimumMarkersInBody, settings6DOF.BoneLengthTolerance,
                settings6DOF.Filter, settings6DOF.Mesh, settings6DOF.Points, settings6DOF.DataOrigin, settings6DOF.DataOrientation);
        }
        /// <summary>Name of 6DOF body</summary>
        [XmlElement("Name")]
        public string Name;
        /// <summary>Color of 6DOF body</summary>
        [XmlElement("Color")]
        public Settings6DOFColor_V2 Color;
        /// <summary>Maximum residual of 6DOF body</summary>
        [XmlElement("MaximumResidual")]
        public float MaximumResidual;
        /// <summary>Minimum markers in 6DOF body</summary>
        [XmlElement("MinimumMarkersInBody")]
        public int MinimumMarkersInBody;
        /// <summary>Bone length tolerance of 6DOF body</summary>
        [XmlElement("BoneLengthTolerance")]
        public float BoneLengthTolerance;
        /// <summary>Filter of 6DOF body</summary>
        [XmlElement("Filter")]
        public Settings6DOFFilter Filter;
        /// <summary>Mesh of 6DOF body</summary>
        [XmlElement("Mesh")]
        public Settings6DOFMesh Mesh;
        /// <summary>List of points in 6DOF body</summary>
        [XmlArray("Points")]
        [XmlArrayItem("Point")]
        public List<Settings6DOFPoint> Points;
        /// <summary>Data origin of 6DOF body</summary>
        [XmlElement("Data_origin")]
        public Settings6DOFDataOrigin DataOrigin;
        /// <summary>Data orientation of 6DOF body</summary>
        [XmlElement("Data_orientation")]
        public Settings6DOFDataOrientation DataOrientation;
    }

    public struct Settings6DOFPoint
    {
        public Settings6DOFPoint(string name, float x, float y, float z, bool _virtual, int physicalId)
        {
            Name = name;
            X = x;
            Y = y;
            Z = z;
            Virtual = _virtual;
            PhysicalId = physicalId;
        }
        [XmlAttribute("Name")]
        public string Name;
        [XmlAttribute("X")]
        public float X;
        [XmlAttribute("Y")]
        public float Y;
        [XmlAttribute("Z")]
        public float Z;
        [XmlAttribute("Virtual")]
        public bool Virtual;
        [XmlAttribute("PhysicalId")]
        public int PhysicalId;
    }

    public struct Settings6DOF
    {
        internal static Settings6DOF_V2 ConvertToSettings6DOF_V2(Settings6DOF settings)
        {
            return new Settings6DOF_V2(settings);
        }
        public Settings6DOF(string name, int colorRGB, float maxResidual, int minimumMarkersInBody, float boneLengthTolerance, Settings6DOFFilter filter, Settings6DOFMesh mesh,
            List<Settings6DOFPoint> points, Settings6DOFDataOrigin dataOrigin, Settings6DOFDataOrientation dataOrientation)
        {
            Name = name;
            ColorRGB = colorRGB;
            MaximumResidual = maxResidual;
            MinimumMarkersInBody = minimumMarkersInBody;
            BoneLengthTolerance = boneLengthTolerance;
            Filter = filter;
            Mesh = mesh;
            Points = points;
            DataOrigin = dataOrigin;
            DataOrientation = dataOrientation;
        }
        /// <summary>Name of 6DOF body</summary>
        public string Name;
        /// <summary>Color of 6DOF body</summary>
        public int ColorRGB;
        /// <summary>Maximum residual of 6DOF body</summary>
        public float MaximumResidual;
        /// <summary>Minimum markers in 6DOF body</summary>
        public int MinimumMarkersInBody;
        /// <summary>Bone length tolerance of 6DOF body</summary>
        public float BoneLengthTolerance;
        /// <summary>Filter of 6DOF body</summary>
        public Settings6DOFFilter Filter;
        /// <summary>Mesh of 6DOF body</summary>
        public Settings6DOFMesh Mesh;
        /// <summary>List of points in 6DOF body</summary>
        public List<Settings6DOFPoint> Points;
        /// <summary>Data origin of 6DOF body</summary>
        public Settings6DOFDataOrigin DataOrigin;
        /// <summary>Data orientation of 6DOF body</summary>
        public Settings6DOFDataOrientation DataOrientation;
    }

    /// <summary>General settings for Analog devices</summary>
    public struct AnalogDevice
    {
        /// <summary>Analog device ID</summary>
        [XmlElement("Device_ID")]
        public int DeviceID;
        /// <summary>Analog device name</summary>
        [XmlElement("Device_Name")]
        public string DeviceName;
        /// <summary>Number of channels in device</summary>
        [XmlElement("Channels")]
        public int ChannelCount;
        /// <summary>Frequency of channels</summary>
        [XmlElement("Frequency")]
        public int Frequency;
        /// <summary>Range of channels</summary>
        [XmlElement("Range")]
        public AnalogRange ChannelRange;
        /// <summary>Information of channels</summary>
        [XmlElement("Channel")]
        public List<AnalogChannelInformation> ChannelInformation;
    }

    /// <summary>Analog range and channels</summary>
    public struct AnalogRange
    {
        /// <summary>Minimum value</summary>
        [XmlElement("Min")]
        public float Min;
        /// <summary>Maximum value</summary>
        [XmlElement("Max")]
        public float Max;
    }

    /// <summary>Settings for Analog channel</summary>
    public struct AnalogChannelInformation
    {
        /// <summary>Channel label</summary>
        [XmlElement("Label")]
        public string Name;
        /// <summary>Unit used by channel</summary>
        [XmlElement("Unit")]
        public string Unit;
    }

    /// <summary>Settings for Force plate</summary>
    public struct ForcePlateSettings
    {
        /// <summary>Force plate index number</summary>
        [XmlElement("Force_Plate_Index")]
        public int ForcePlateIndex;
        /// <summary>ID of force plate</summary>
        [XmlElement("Plate_ID")]
        public int PlateID;
        /// <summary>ID of analog device connected to force plate. 0 = no analog device associated with force plate</summary>
        [XmlElement("Analog_Device_ID")]
        public int AnalogDeviceID;
        /// <summary>Measurement frequency of analog device connected to force plate</summary>
        [XmlElement("Frequency")]
        public float Frequency;
        /// <summary>Force plate type</summary>
        [XmlElement("Type")]
        public string Type;
        /// <summary>Name of force plate</summary>
        [XmlElement("Name")]
        public string Name;
        /// <summary>Force plate length</summary>
        [XmlElement("Length")]
        public float Length;
        /// <summary>Force plate width</summary>
        [XmlElement("Width")]
        public float Width;
        /// <summary>four blocks with the corners of the force plate</summary>
        [XmlElement("Location")]
        public Location Location;
        /// <summary>Force plate origin</summary>
        [XmlElement("Origin")]
        public Point Origin;
        /// <summary>Analog channels connected to force plate</summary>
        [XmlArray("Channels")]
        public List<ForceChannel> Channels;
        /// <summary>Calibration of the force plate</summary>
        [XmlElement("Calibration_Matrix")]
        public CalibrationMatrix CalibrationMatrix;

    }

    /// <summary>Struct with calibration matrix for force plate</summary>
    public struct CalibrationMatrix
    {
        [XmlElement("Row1")]
        public CalibrationRow Row1;
        [XmlElement("Row2")]
        public CalibrationRow Row2;
        [XmlElement("Row3")]
        public CalibrationRow Row3;
        [XmlElement("Row4")]
        public CalibrationRow Row4;
        [XmlElement("Row5")]
        public CalibrationRow Row5;
        [XmlElement("Row6")]
        public CalibrationRow Row6;
        [XmlElement("Row7")]
        public CalibrationRow Row7;
        [XmlElement("Row8")]
        public CalibrationRow Row8;
        [XmlElement("Row9")]
        public CalibrationRow Row9;
        [XmlElement("Row10")]
        public CalibrationRow Row10;
        [XmlElement("Row11")]
        public CalibrationRow Row11;
        [XmlElement("Row12")]
        public CalibrationRow Row12;

    }

    /// <summary>row for calibration matrix of force plates</summary>
    public struct CalibrationRow
    {
        [XmlElement("Col1")]
        public float Col1;
        [XmlElement("Col2")]
        public float Col2;
        [XmlElement("Col3")]
        public float Col3;
        [XmlElement("Col4")]
        public float Col4;
        [XmlElement("Col5")]
        public float Col5;
        [XmlElement("Col6")]
        public float Col6;
        [XmlElement("Col7")]
        public float Col7;
        [XmlElement("Col8")]
        public float Col8;
        [XmlElement("Col9")]
        public float Col9;
        [XmlElement("Col10")]
        public float Col10;
        [XmlElement("Col11")]
        public float Col11;
        [XmlElement("Col12")]
        public float Col12;
    }

    /// <summary>Settings for channel</summary>
    [XmlType("Channel")]
    public struct ForceChannel
    {
        /// <summary>Channel number</summary>
        [XmlElement("Channel_No")]
        public int ChannelNumber;
        /// <summary>Conversion factor of channel</summary>
        [XmlElement("ConversionFactor")]
        public float ConversionFactor;
    }

    /// <summary>Location for force plate</summary>
    public struct Location
    {
        /// <summary>First corner</summary>
        [XmlElement("Corner1")]
        public Point Corner1;
        /// <summary>Second corner</summary>
        [XmlElement("Corner2")]
        public Point Corner2;
        /// <summary>Third corner</summary>
        [XmlElement("Corner3")]
        public Point Corner3;
        /// <summary>Fourth corner</summary>
        [XmlElement("Corner4")]
        public Point Corner4;
    }

    /// <summary>Settings for image from camera</summary>
    public struct ImageCamera
    {
        /// <summary>ID of camera</summary>
        [XmlElement("ID")]
        public int CameraID;
        /// <summary>Image streaming on or off</summary>
        [XmlElement("Enabled")]
        public bool Enabled;

        /// <summary>Format of image</summary>
        [XmlIgnore]
        public ImageFormat ImageFormat;
        [XmlElement("Format")]
        public string ImageFormatAsString
        {
            get
            {
                return EnumHelper.EnumToXmlEnumString(ImageFormat);
            }
            set
            {
                ImageFormat = EnumHelper.XmlEnumStringToEnum(value, ImageFormat.Unknown);
            }
        }

        /// <summary>Image width</summary>
        [XmlElement("Width")]
        public int Width;
        /// <summary>Image height</summary>
        [XmlElement("Height")]
        public int Height;
        /// <summary>Left edge relative to original image</summary>
        [XmlElement("Left_Crop")]
        public float CropLeft;
        /// <summary>Top edge relative to original image</summary>
        [XmlElement("Top_Crop")]
        public float CropTop;
        /// <summary>Right edge relative to original image</summary>
        [XmlElement("Right_Crop")]
        public float CropRight;
        /// <summary>Bottom edge relative to original image</summary>
        [XmlElement("Bottom_Crop")]
        public float CropBottom;
    }

    /// <summary>Settings for labeled marker</summary>
    public struct Settings3DLabel
    {
        /// <summary>Name of marker</summary>
        [XmlElement("Name")]
        public string Name;
        /// <summary>Color of marker</summary>
        [XmlElement("RGBColor")]
        public int ColorRGB;
    }

    /// <summary>Tracking processing actions</summary>
    public enum SettingsTrackingProcessingAction
    {
        [XmlEnum("Unknown SettingsTrackingProcessingAction")]
        Unknown = -1,
        [XmlEnum("false")]
        ProcessingNone = 0,
        [XmlEnum("2D")]
        ProcessingTracking2D,
        [XmlEnum("3D")]
        ProcessingTracking3D
    }

    /// <summary>Camera models</summary>
    public enum CameraModel
    {
        [XmlEnum("Unknown CameraModel")]
        Unknown = -1,
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
        [XmlEnum("Oqus 100")]
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
        [XmlEnum("Oqus 600 Plus")]
        ModelQqus600Plus,
        [XmlEnum("Miqus M1")]
        ModelMiqusM1,
        [XmlEnum("Miqus M3")]
        ModelMiqusM3,
        [XmlEnum("Miqus M5")]
        ModelMiqusM5,
        [XmlEnum("Miqus Sync Unit")]
        ModelMiqusSU,
        [XmlEnum("Miqus Video")]
        ModelMiqusVideo,
        [XmlEnum("Miqus Video Color")]
        ModelMiqusVideoColor,
        [XmlEnum("Miqus Hybrid")]
        ModelMiqusHybrid,
        [XmlEnum("Arqus A5")]
        ModelArqusA5,
        [XmlEnum("Arqus A9")]
        ModelArqusA9,
        [XmlEnum("Arqus A12")]
        ModelArqusA12,
        [XmlEnum("Arqus A26")]
        ModelArqusA26,
    }

    /// <summary>Camera modes</summary>
    public enum CameraMode
    {
        [XmlEnum("Unknown CameraMode")]
        Unknown = -1,
        [XmlEnum("Marker")]
        ModeMarker = 0,
        [XmlEnum("Marker Intensity")]
        ModeMarkerIntensity,
        [XmlEnum("Video")]
        ModeVideo
    }

    /// <summary>Sync out modes</summary>
    public enum SyncOutFrequencyMode
    {
        [XmlEnum("Unknown SyncOutFrequencyMode")]
        Unknown = -1,
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
        [XmlEnum("Continuous 100Hz")]
        ModeFixed100Hz
    }

    /// <summary>Signal sources</summary>
    public enum SignalSource
    {
        [XmlEnum("Unknown SignalSource")]
        Unknown = -1,
        [XmlEnum("Control port")]
        SourceControlPort = 0,
        [XmlEnum("IR receiver")]
        SourceIRReceiver,
        [XmlEnum("SMPTE")]
        SourceSMPTE,
        [XmlEnum("Video sync")]
        SourceVideoSync,
        [XmlEnum("IRIG")]
        SourceIRIGSync
    }

    /// <summary>Signal modes</summary>
    public enum SignalMode
    {
        [XmlEnum("Unknown SignalMode")]
        Unknown = -1,
        [XmlEnum("Periodic")]
        Periodic = 0,
        [XmlEnum("Non-periodic")]
        NonPeriodic
    }

    /// <summary>Axises</summary>
    public enum Axis
    {
        [XmlEnum("Unknown Axis")]
        Unknown = -1,
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
        [XmlEnum("Unknown SignalEdge")]
        Unknown = -1,
        [XmlEnum("Negative")]
        Negative = 0,
        [XmlEnum("Positive")]
        Positive
    }

    /// <summary>Signal Polarity</summary>
    public enum SignalPolarity
    {
        [XmlEnum("Unknown SignalPolarity")]
        Unknown = -1,
        [XmlEnum("Negative")]
        Negative = 0,
        [XmlEnum("Positive")]
        Positive
    }

    /// <summary>Image formats Available</summary>
    public enum ImageFormat
    {
        [XmlEnum("Unknown ImageFormat")]
        Unknown = -1,
        [XmlEnum("RAWGrayscale")]
        FormatRawGrayScale = 0,
        [XmlEnum("RAWBGR")]
        FormatRawBGR,
        [XmlEnum("JPG")]
        FormatJPG,
        [XmlEnum("PNG")]
        FormatPNG
    }
    /// <summary>Video resolution settings for video cameras</summary>
    public enum SettingsVideoResolution
    {
        [XmlEnum("Unknown SettingsVideoResolution")]
        Unknown = -1,
        [XmlEnum("1080p")]
        VideoResolution_1080p = 0,
        [XmlEnum("720p")]
        VideoResolution_720p,
        [XmlEnum("540p")]
        VideoResolution_540p,
        [XmlEnum("480p")]
        VideoResolution_480p,
    }
    /// <summary>Video aspect ratio settings for video cameras</summary>
    public enum SettingsVideoAspectRatio
    {
        [XmlEnum("Unknown SettingsVideoAspectRatio")]
        Unknown = -1,
        [XmlEnum("16x9")]
        AspectRatio_16x9,
        [XmlEnum("4x3")]
        AspectRatio_4x3,
        [XmlEnum("1x1")]
        AspectRatio_1x1,
    }
    /// <summary>Timestamp type</summary>
    public enum TimestampType
    {
        [XmlEnum("Unknown TimestampType")]
        Unknown = -1,
        [XmlEnum("SMPTE")]
        SMPTE = 0,
        [XmlEnum("IRIG")]
        IRIG,
        [XmlEnum("CameraTime")]
        CameraTime,
    }
}