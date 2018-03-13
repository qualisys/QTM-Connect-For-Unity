// Realtime SDK for Qualisys Track Manager. Copyright 2015-2018 Qualisys AB
//
using QTMRealTimeSDK.Data;
using System;

namespace QTMRealTimeSDK
{
    /// <summary>
    /// Converts bytes to different data types
    /// </summary>
    static class BitConvert
    {
        /// <summary>
        /// Convert bytes at position to 32-bit integer
        /// </summary>
        /// <param name="data">Data packet</param>
        /// <param name="position">position to convert, will be increased with the size of int in bytes</param>
        /// <returns>converted integer</returns>
        public static int GetInt32(byte[] data, ref int position)
        {
            var value = BitConverter.ToInt32(data, position);
            position += sizeof(int);
            return value;
        }

        /// <summary>
        /// Convert bytes at position to unsigned 32-bit integer
        /// </summary>
        /// <param name="data">Data packet</param>
        /// <param name="position">position to convert, will be increased with the size of uint in bytes</param>
        /// <returns>converted usigned integer</returns>
        public static uint GetUInt32(byte[] data, ref int position)
        {
            var value = BitConverter.ToUInt32(data, position);
            position += sizeof(uint);
            return value;
        }

        /// <summary>
        /// Convert bytes at position to unsigned 16-bit integer
        /// </summary>
        /// <param name="data">Data packet</param>
        /// <param name="position">position to convert, will be increased with the size of ushort in bytes</param>
        /// <returns>converted ushort integer</returns>
        public static ushort GetUShort(byte[] data, ref int position)
        {
            var value = BitConverter.ToUInt16(data, position);
            position += sizeof(ushort);
            return value;
        }

        /// <summary>
        /// Convert bytes at position to 32-bit float
        /// </summary>
        /// <param name="data">Data packet</param>
        /// <param name="position">position to convert, will be increased with the size of float in bytes</param>
        /// <returns>converted float integer</returns>
        public static float GetFloat(byte[] data, ref int position)
        {
            var value = BitConverter.ToSingle(data, position);
            position += sizeof(float);
            return value;
        }

        /// <summary>
        /// Convert bytes at position to a Point (3 float values)
        /// </summary>
        /// <param name="data">Data packet</param>
        /// <param name="position">position to convert, will be increased with the size of three floats in bytes</param>
        /// <returns>struct of Point with 3 float values for x,y,z</returns>
		public static Point GetPoint(byte[] data, ref int position)
        {
            Point point;
            point.X = BitConverter.ToSingle(data, position + 0);
            point.Y = BitConverter.ToSingle(data, position + 4);
            point.Z = BitConverter.ToSingle(data, position + 8);
            position += sizeof(float) * 3;
            return point;
        }

        /// <summary>
        /// Convert bytes at position to a Point (3 float values)
        /// </summary>
        /// <param name="data">Data packet</param>
        /// <param name="rotation">rotation to convert, will be increased with the size of three floats in bytes</param>
        /// <returns>struct of Rotation with 3 float values for x,y,z</returns>
		public static EulerRotation GetEulerRotation(byte[] data, ref int position)
        {
            EulerRotation rotation;
            rotation.First = BitConverter.ToSingle(data, position + 0);
            rotation.Second = BitConverter.ToSingle(data, position + 4);
            rotation.Third = BitConverter.ToSingle(data, position + 8);
            position += sizeof(float) * 3;
            return rotation;
        }
    }
}
