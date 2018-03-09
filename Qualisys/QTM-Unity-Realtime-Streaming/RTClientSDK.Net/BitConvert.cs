// Realtime SDK for Qualisys Track Manager. Copyright 2015 Qualisys AB
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QTMRealTimeSDK.Data;

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
			byte[] intData = new byte[sizeof(int)];
			Array.Copy(data, position, intData, 0, sizeof(int));
			position += sizeof(int);
			return BitConverter.ToInt32(intData, 0);
		}

        /// <summary>
        /// Convert bytes at position to unsigned 32-bit integer
        /// </summary>
        /// <param name="data">Data packet</param>
        /// <param name="position">position to convert, will be increased with the size of uint in bytes</param>
        /// <returns>converted usigned integer</returns>
        public static uint GetUInt32(byte[] data, ref int position)
        {
            byte[] intData = new byte[sizeof(uint)];
            Array.Copy(data, position, intData, 0, sizeof(uint));
            position += sizeof(uint);
            return BitConverter.ToUInt32(intData, 0);
        }

        /// <summary>
        /// Convert bytes at position to 16-bit integer
        /// </summary>
        /// <param name="data">Data packet</param>
        /// <param name="position">position to convert, will be increased with the size of short in bytes</param>
        /// <returns>converted short integer</returns>
        public static short GetShort(byte[] data, ref int position)
		{
			byte[] shortData = new byte[sizeof(short)];
			Array.Copy(data, position, shortData, 0, sizeof(short));
			position += sizeof(short);
			return BitConverter.ToInt16(shortData, 0);
		}

        /// <summary>
        /// Convert bytes at position to unsigned 16-bit integer
        /// </summary>
        /// <param name="data">Data packet</param>
        /// <param name="position">position to convert, will be increased with the size of ushort in bytes</param>
        /// <returns>converted ushort integer</returns>
        public static ushort GetUShort(byte[] data, ref int position)
        {
            byte[] shortData = new byte[sizeof(ushort)];
            Array.Copy(data, position, shortData, 0, sizeof(ushort));
            position += sizeof(ushort);
            return BitConverter.ToUInt16(shortData, 0);
        }

        /// <summary>
        /// Convert bytes at position to 64-bit integer
        /// </summary>
        /// <param name="data">Data packet</param>
        /// <param name="position">position to convert, will be increased with the size of long in bytes</param>
        /// <returns>converted long integer</returns>
        public static long GetLong(byte[] data, ref int position)
		{
            byte[] longData = new byte[sizeof(long)];
            Array.Copy(data, position, longData, 0, sizeof(long));
            position += sizeof(long);
            return BitConverter.ToInt64(longData, 0);
		}

        /// <summary>
        /// Convert bytes at position to unsigned 64-bit integer
        /// </summary>
        /// <param name="data">Data packet</param>
        /// <param name="position">position to convert, will be increased with the size of long in bytes</param>
        /// <returns>converted ulong integer</returns>
        public static ulong GetULong(byte[] data, ref int position)
        {
            byte[] longData = new byte[sizeof(ulong)];
            Array.Copy(data, position, longData, 0, sizeof(ulong));
            position += sizeof(ulong);
            return BitConverter.ToUInt64(longData, 0);
        }

        /// <summary>
        /// Convert bytes at position to 32-bit float
        /// </summary>
        /// <param name="data">Data packet</param>
        /// <param name="position">position to convert, will be increased with the size of float in bytes</param>
        /// <returns>converted float integer</returns>
        public static float GetFloat(byte[] data, ref int position)
		{
            byte[] floatData = new byte[sizeof(float)];
            Array.Copy(data, position, floatData, 0, sizeof(float));
            position += sizeof(float);
            return BitConverter.ToSingle(floatData, 0);
		}

        /// <summary>
        /// Convert bytes at position to 64-bit float
        /// </summary>
        /// <param name="data">Data packet</param>
        /// <param name="position">position to convert, will be increased with the size of double in bytes</param>
        /// <returns>converted double integer</returns>
        public static double GetDouble(byte[] data, ref int position)
		{
            byte[] doubleData = new byte[sizeof(double)];
            Array.Copy(data, position, doubleData, 0, sizeof(double));
            position += sizeof(double);
            return BitConverter.ToInt64(doubleData, 0);
		}

        /// <summary>
        /// Convert bytes at position to a sPoint (3 float values)
        /// </summary>
        /// <param name="data">Data packet</param>
        /// <param name="position">position to convert, will be increased with the size of three floats in bytes</param>
        /// <returns>struct of sPoint with 3 float values for x,y,z</returns>
		public static Point GetPoint(byte [] data, ref int position)
		{
			Point point;
			byte[] pointData = new byte[sizeof(float) * 3];
			Array.Copy(data, position, pointData, 0, sizeof(float) * 3);
			point.X = BitConverter.ToSingle(pointData, 0);
			point.Y = BitConverter.ToSingle(pointData, 4);
			point.Z = BitConverter.ToSingle(pointData, 8);
			position += sizeof(float) * 3;
			return point;
		}
    }
}
