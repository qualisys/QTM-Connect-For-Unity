// Realtime SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using QTMRealTimeSDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QTMRealTimeSDK.Data
{

    public static class TimecodeExtensions
    {
        public static bool IsIRIG(this Timecode timecode) { return timecode.Type == TimecodeType.IRIG; }
        public static bool IsSMPTE(this Timecode timecode) { return timecode.Type == TimecodeType.SMPTE; }
        public static bool IsCameraTime(this Timecode timecode) { return timecode.Type == TimecodeType.CameraTime; }

        /// <summary>
        /// Get timecode converted to IRIG timestamp
        /// </summary>
        public static void GetAsIRIG(this Timecode timecode, out IRIGTimecode irig)
        {
            irig = new IRIGTimecode();
            irig.Year = 0x7f & timecode.High;
            irig.Day = 0x1FF & (timecode.High >> 7);
            irig.Hour = 0x1f & timecode.Low;
            irig.Minute = 0x3F & (timecode.Low >> 5);
            irig.Second = 0x3F & (timecode.Low >> 11);
            irig.Tenth = 0xF & (timecode.Low >> 17);
        }

        /// <summary>
        /// Get timecode converted to SMPTE timestamp
        /// </summary>
        public static void GetAsSMPTE(this Timecode timecode, out SMPTETimecode smpte)
        {
            smpte = new SMPTETimecode();
            smpte.Hour = 0x1f & timecode.Low;
            smpte.Minute = 0x3F & (timecode.Low >> 5);
            smpte.Second = 0x3F & (timecode.Low >> 11);
            smpte.Frame = 0x1F & (timecode.Low >> 17);
        }

        /// <summary>
        /// Get timecode as camera timestamp
        /// </summary>
        public static void GetAsCameraTime(this Timecode timecode, out UInt64 cameratime)
        {
            cameratime = ((UInt64)(timecode.High) << 32) | timecode.Low;
        }

        /// <summary>
        /// Format timestamp depending on type
        /// </summary>
        public static string FormatTimestamp(this Timecode timecode)
        {
            string output = string.Empty;
            switch (timecode.Type)
            {
                case TimecodeType.SMPTE:
                    SMPTETimecode smpte;
                    timecode.GetAsSMPTE(out smpte);
                    output = string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D2}", smpte.Hour, smpte.Minute, smpte.Second, smpte.Frame);
                    break;
                case TimecodeType.IRIG:
                    IRIGTimecode irig;
                    timecode.GetAsIRIG(out irig);
                    output = string.Format("{0:D2}:{1:D3}:{2:D2}:{3:D2}:{4:D2}.{5:D}", irig.Year, irig.Day, irig.Hour, irig.Minute, irig.Second, irig.Tenth);
                    break;
                case TimecodeType.CameraTime:
                    UInt64 cameraTime;
                    timecode.GetAsCameraTime(out cameraTime);
                    const UInt64 cTicksPerSecond = 10000000;
                    UInt64 cSeconds = (cameraTime / cTicksPerSecond);
                    UInt64 cNanoseconds = ((cameraTime % cTicksPerSecond) * (1000000000 / cTicksPerSecond));
                    output = string.Format("{0}.{1:D9}", cSeconds, cNanoseconds);
                    break;
                default:
                    throw new ArgumentException("Timecode Type invalid");
            }
            return output;
        }
    }
}