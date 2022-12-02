// Unity SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using QTMRealTimeSDK.Data;
using QTMRealTimeSDK.Settings;
using UnityEngine;

namespace QualisysRealTime.Unity
{
    public static class QtmRhsToUnityLhsExtensions 
    {
        public static Vector3 ToVector3(this Point point)
        { 
            return new Vector3(point.X, point.Y, point.Z);
        }

        public static Vector3 QtmRhsToUnityLhs(this Point position)
        {
            return new Vector3(-position.X / 1000f, position.Y / 1000f, position.Z / 1000f);
        }

        public static Vector3 QtmRhsToUnityLhs(this Position position)
        {
            return new Vector3(-position.X / 1000f, position.Y / 1000f, position.Z / 1000f);
        }

        public static Vector3 QtmRhsToUnityLhsNormalizedDirection(this Point position, Quaternion coordinateSystemRotation)
        {
            var p = new Vector3(-position.X, position.Y, position.Z);
            return coordinateSystemRotation.Rotate(p);
        }

        public static Vector3 QtmRhsToUnityLhs(this Point position, Quaternion coordinateSystemRotation)
        {
            var p = new Vector3(-position.X / 1000f, position.Y / 1000f, position.Z / 1000f);
            return coordinateSystemRotation.Rotate(p);
        }

        public static Vector3 QtmRhsToUnityLhs(this Position position, Quaternion coordinateSystemRotation)
        {
            var p = new Vector3(-position.X / 1000f, position.Y / 1000f, position.Z / 1000f);
            return coordinateSystemRotation.Rotate(p);
        }

        public static Quaternion QtmRhsToUnityLhs(this QTMRealTimeSDK.Settings.Rotation src)
        {
            var q = new Quaternion(-src.X, src.Y, src.Z, -src.W);
            return q;
        }
        public static Quaternion QtmRhsToUnityLhs(this QuatRotation src)
        {
            var q = new Quaternion(-src.X, src.Y, src.Z, -src.W);
            return q;
        }
        public static Quaternion QtmRhsToUnityLhs(this Quaternion src)
        {
            var q = new Quaternion(-src.x, src.y, src.z, -src.w);
            return q;
        }



        public static Quaternion QtmRhsToUnityLhs(this QTMRealTimeSDK.Settings.Rotation src, Quaternion coordinateSystemRotation)
        {
            var q = new Quaternion(-src.X, src.Y, src.Z, -src.W);
            return coordinateSystemRotation * q;
        }
        public static Quaternion QtmRhsToUnityLhs(this QuatRotation src, Quaternion coordinateSystemRotation)
        {
            var q = new Quaternion(-src.X, src.Y, src.Z, -src.W);
            return coordinateSystemRotation * q;
        }

        public static Quaternion QtmRhsToUnityLhs(this Quaternion src, Quaternion coordinateSystemRotation)
        {
            var q = new Quaternion(-src.x, src.y, src.z, -src.w);
            return coordinateSystemRotation * q;
        }
    }
}

