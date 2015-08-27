#region --- LICENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015 Qualisys AB

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using OpenTK;

namespace QualisysRealTime.Unity.Skeleton
{
    public static class QuaternionHelper2
    {
        public static float precision = 0.9999f;
        /// <summary>
        /// Defines the zero quaternion.
        /// </summary>
        public static Quaternion Zero = new Quaternion(0, 0, 0, 0);

        /// <summary>
        /// Check if any element in quaternion is NaN
        /// </summary>
        /// <param name="quaternion"> Quaternion to be checked </param>
        /// <returns>True if any of x, y, z, w is NaN</returns>
        public static bool IsNaN(this Quaternion q)
        {
            return q.Xyz.IsNaN() || float.IsNaN(q.W);
        }

        /// <summary>
        /// Rotation around X axis
        /// </summary>
        /// <param name="radians">rotation amount</param>   
        /// <returns>Quaternion with rotation</returns>
        public static Quaternion RotationX(float radians)
        {
            float s, c, angle;
            angle = (radians * 0.5f);
            s = Mathf.Sin(angle);
            c = Mathf.Cos(angle);
            return new Quaternion(s, 0.0f, 0.0f, c);
        }

        /// <summary>
        /// Rotation around Y axis
        /// </summary>
        /// <param name="radians">rotation amount</param>
        /// <returns>Quaternion with rotation</returns>
        public static Quaternion RotationY(float radians)
        {
            float s, c, angle;
            angle = (radians * 0.5f);
            s = Mathf.Sin(angle);
            c = Mathf.Cos(angle);
            return new Quaternion(0.0f, s, 0.0f, c);
        }

        /// <summary>
        /// Rotation around Z axis
        /// </summary>
        /// <param name="radians">rotation amount</param>
        /// <returns>Quaternion with rotation</returns>
        public static Quaternion RotationZ(float radians)
        {
            float s, c, angle;
            angle = (radians * 0.5f);
            s = Mathf.Sin(angle);
            c = Mathf.Cos(angle);
            return new Quaternion(0.0f, 0.0f, s, c);
        }

        /// <summary>
        /// Quaternion from Matrix4 matrix
        /// </summary>
        /// <param name="matrix">Matrix4 matrix</param>
        /// <returns>Quaternion</returns>
        public static Quaternion FromMatrix(Matrix4 matrix)
        {
            float[] matrixArray = new float[9];
            matrixArray[0] = matrix.M11;
            matrixArray[1] = matrix.M21;
            matrixArray[2] = matrix.M31;
            matrixArray[3] = matrix.M12;
            matrixArray[4] = matrix.M22;
            matrixArray[5] = matrix.M32;
            matrixArray[6] = matrix.M13;
            matrixArray[7] = matrix.M23;
            matrixArray[8] = matrix.M33;
            return FromMatrix(matrixArray);
        }
        /// <summary>
        /// Quaternion from matrix array
        /// </summary>
        /// <param name="array float">size nine array rep of a rotation matrix</param>
        /// <returns>The resulting Quaternion</returns>
        public static Quaternion FromMatrix(float[] matrix)
        {
            float trace, radicand, scale, xx, yx, zx, xy, yy, zy, xz, yz, zz, tmpx, tmpy, tmpz, tmpw, qx, qy, qz, qw;
            bool negTrace, ZgtX, ZgtY, YgtX;
            bool largestXorY, largestYorZ, largestZorX;

            xx = matrix[0];
            yx = matrix[1];
            zx = matrix[2];
            xy = matrix[3];
            yy = matrix[4];
            zy = matrix[5];
            xz = matrix[6];
            yz = matrix[7];
            zz = matrix[8];

            trace = ((xx + yy) + zz);

            negTrace = (trace < 0.0);
            ZgtX = zz > xx;
            ZgtY = zz > yy;
            YgtX = yy > xx;
            largestXorY = (!ZgtX || !ZgtY) && negTrace;
            largestYorZ = (YgtX || ZgtX) && negTrace;
            largestZorX = (ZgtY || !YgtX) && negTrace;

            if (largestXorY)
            {
                zz = -zz;
                xy = -xy;
            }
            if (largestYorZ)
            {
                xx = -xx;
                yz = -yz;
            }
            if (largestZorX)
            {
                yy = -yy;
                zx = -zx;
            }

            radicand = (((xx + yy) + zz) + 1.0f);
            scale = (0.5f * (1.0f / Mathf.Sqrt(radicand)));

            tmpx = ((zy - yz) * scale);
            tmpy = ((xz - zx) * scale);
            tmpz = ((yx - xy) * scale);
            tmpw = (radicand * scale);
            qx = tmpx;
            qy = tmpy;
            qz = tmpz;
            qw = tmpw;

            if (largestXorY)
            {
                qx = tmpw;
                qy = tmpz;
                qz = tmpy;
                qw = tmpx;
            }
            if (largestYorZ)
            {
                tmpx = qx;
                tmpz = qz;
                qx = qy;
                qy = tmpx;
                qz = qw;
                qw = tmpz;
            }

            return new Quaternion(qx, qy, qz, qw);
        }

        /// <summary>
        /// Calculates the difference in rotation between two Quaternions
        /// if result is 0, there is no diffrence between the Quaternions
        /// if the results is 1, the diffrence is 180 degrees diffrence 
        /// </summary>
        /// <param name="a">The first quaternion</param>
        /// <param name="b">The secound quaternion</param>
        /// <returns>float between 0 and 1, where 0 the Quaternions are the same, and 1 they are at a 180 degrees diffrences</returns>
        public static float DiffrenceBetween(Quaternion right, Quaternion left)
        {
            float dot = left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W;
            return 1f - Mathf.Sqrt(dot);
        }
 
        /// <summary>
        /// Returns a quaternion representing the rotation from vector a to b
        /// Robust, does not return NaN
        /// </summary>
        /// <param name="a">The first vector</param>
        /// <param name="b">The secound vector</param>
        /// <param name="stiffness">Stiffness value</param>
        /// <returns>The rotation Quaternion from a to b in stiffness amont </returns>
        public static Quaternion GetRotationBetween(Vector3 a, Vector3 b, float stiffness = 1f)
        {
            return RotationBetween(a, b);
        }

        /// <summary>
        /// Returns a quaternion representing the rotation between vectors
        /// Not robust, but faster
        /// </summary>
        /// <param name="from">The vetor the rotation start from</param>
        /// <param name="to">The vector the quaternion should result in if from vector was transformed by the resulting Quaternion</param>
        /// <returns>The quaternion that when from vector is transformed with should result in this vector</returns>
        public static Quaternion RotationBetween(Vector3 from, Vector3 to)
        {
            return Quaternion.FromAxisAngle(Vector3.Cross(from, to), Vector3.CalculateAngle(from, to));
        }

        /// <summary>
        /// Get orientation of three points, where on point defines forward
        /// </summary>
        /// <param name="back">position vector of back marker</param>
        /// <param name="left">position vector of left marker</param>
        /// <param name="right">position vector of right marker</param>
        /// <returns>Quaternion rotation</returns>
        public static Quaternion GetOrientation(Vector3 forwardPoint, Vector3 leftPoint, Vector3 rightPoint)
        {
            Vector3 x = rightPoint - leftPoint;
            Vector3 z = forwardPoint - Vector3Helper.MidPoint(leftPoint, rightPoint);
            return GetOrientationFromZY(z, Vector3.Cross(z, x));
        }

        /// <summary>
        /// Get quaternion with rotation as Y axis towards target as close as z parameter as possible
        /// </summary>
        /// <param name="source">The source position vector to look from</param>
        /// <param name="leftHip">The target position vector the source should point the Y axus to look at</param>
        /// <param name="rightHip">The definition of direction Z axis</param>
        /// <returns>Quaternion with rotation to target</returns>
        public static Quaternion LookAtUp(Vector3 source, Vector3 target, Vector3 z)
        {
            Vector3 y = target - source;
            return GetOrientationFromZY(z, y);
        }
        /// <summary>
        /// Get quaternion with rotation as Y axis from source towards target and X close to right parameter
        /// </summary>
        /// <param name="source">Position vector to look from</param>
        /// <param name="target">Position vector to look at</param>
        /// <param name="X axis">Direction vector of defenition of x axis</param>
        /// <returns>Quaternion with rotation to target</returns>
        public static Quaternion LookAtRight(Vector3 source, Vector3 target, Vector3 x)
        {
            Vector3 y = target - source;
            return GetOrientationFromYX(y, x);
        }
        /// <summary>
        /// Get quaternion with front and right vector
        /// </summary>
        /// <param name="source">Front vector</param>
        /// <param name="target">Right vector</param>
        /// <returns>Quaternion with rotation</returns>
        public static Quaternion GetOrientationFromYX(Vector3 y, Vector3 x)
        {
            Vector3 z = Vector3.Cross(y, x);
            return GetOrientationFromZY(z, y);
        }
        /// <summary>
        /// Get quaternion with front and right vector
        /// </summary>
        /// <param name="source">Front vector</param>
        /// <param name="target">Right vector</param>
        /// <returns>Quaternion with rotation</returns>
        public static Quaternion GetOrientationFromZY(Vector3 z, Vector3 y)
        {
            Vector3Helper.OrthoNormalize(ref y, ref z);
            Quaternion zRot = Quaternion.FromAxisAngle(Vector3.Cross(Vector3.UnitZ, z), Vector3.CalculateAngle(Vector3.UnitZ, z));
            Vector3 t = Vector3.Transform(Vector3.UnitY, zRot);
            return Quaternion.FromAxisAngle(Vector3.Cross(t, y), Vector3.CalculateAngle(t, y)) * zRot;
        }
    }
}
