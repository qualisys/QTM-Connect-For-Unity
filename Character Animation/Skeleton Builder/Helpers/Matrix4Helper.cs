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
    static class Matrix4Helper
    {
        /// <summary>
        /// Create matrix from position and three vectors
        /// </summary>
        /// <param name="position">position</param>
        /// <param name="front">forward vector</param>
        /// <param name="up">up vector</param>
        /// <param name="right">right vector</param>
        /// <returns>matrix with cooridnate system based on vectors</returns>
        public static Matrix4 GetMatrix(Vector3 position, Vector3 front, Vector3 up, Vector3 right)
        {
            return new Matrix4(
                    new Vector4(right),
                    new Vector4(up),
                    new Vector4(front),
                    new Vector4(position.X, position.Y, position.Z, 1)
                );
        }

        /// <summary>
        /// Returns a rotation matrix with position <0,0,0> according to vector x and z
        /// </summary>
        /// <param name="x"> the x vector</param>
        /// <param name="z">the z vector</param>
        /// <returns>Rotation matrix</returns>
        public static Matrix4 GetOrientationMatrix(Vector3 x, Vector3 z)
        {
            x.NormalizeFast();
            z.NormalizeFast();
            Vector3 y = Vector3.Cross(z, x);
            return GetMatrix(Vector3.Zero, z, y, Vector3.Cross(y, z));
        }
    }

}
