#region --- LICENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015 Qualisys AB

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using OpenTK;

namespace QualisysRealTime.Unity.Skeleton
{
    static class Vector3Helper
    {
        /// <summary>
        /// Defines a NaN Vector3.
        /// </summary>
        public static Vector3 NaN = new Vector3(float.NaN, float.NaN, float.NaN);

        /// <summary>
        /// Check if any element in vector is NaN
        /// </summary>
        /// <param name="vector"> Vector to be checked </param>
        /// <returns>True if any of x, y, z is NaN</returns>
        public static bool IsNaN(this Vector3 v)
        {
            return float.IsNaN(v.X) || float.IsNaN(v.Y) || float.IsNaN(v.Z);
        }

        /// <summary>
        /// The vector at the center or in of two vectors
        /// </summary>
        /// <param name="left vector">the first vector</param>
        /// <param name="right vector">the secound vector</param>
        /// <returns>Vector3 at the center the vectors</returns>
        public static Vector3 MidPoint(this Vector3 leftVect, Vector3 rightVect)
        {
            return PointBetween(leftVect, rightVect, 0.5f);
        }
        /// <summary>
        /// Gives the vector at a distance from one vector towards the other
        /// </summary>
        /// <param name="left vector">The first vector</param>
        /// <param name="right vector">The secound vector</param>
        /// <param name="distance">Distans from the first vector in precent towards the second</param>
        /// <returns>Vector3 in between the points at a given precent distance</returns>
        public static Vector3 PointBetween(this Vector3 leftVect, Vector3 rightVect, float dist)
        {
            return (leftVect - rightVect) * dist + rightVect;
        }

        /// <summary>
        /// Applies Gram-Schmitt Ortho-normalization to the given two input Vectro3 objects. 
        /// </summary>
        /// <param name="vec1">The first Vector3 objects to be ortho-normalized</param>
        /// <param name="vec2">The secound Vector3 objects to be ortho-normalized</param>
        public static void OrthoNormalize(ref Vector3 vec1, ref Vector3 vec2)
        {
            vec1.NormalizeFast();
            vec2 = Vector3.Subtract(vec2, ProjectAndCreate(vec2, vec1));
            vec2.NormalizeFast();
        }
        /// <summary>
        /// Applies Gram-Schmitt Ortho-normalization to the given set of input Vectro3 objects.
        /// </summary>
        /// <param name="vector array">Array of Vector3 objects to be ortho-normalized</param>
        public static void OrthoNormalize(ref Vector3[] vecs)
        {
            for (int i = 0; i < vecs.Length; ++i)
            {
                Vector3 accum = Vector3.Zero;

                for (int j = 0; j < i; ++j)
                {
                    accum += ProjectAndCreate(vecs[i], vecs[j]);
                }

                vecs[i] = Vector3.Subtract(vecs[i], accum);
                vecs[i].Normalize();
            }
        }
        /// <summary>
        /// Projects Vector3 v1 onto Vector3 v2 and creates a new Vector3 for the result.
        /// </summary>
        /// <param name="vector 1"> Vector3 to be projected.</param>
        /// <param name="vector2">v2 Vector3 the Vector3 to be projected on.</param>
        /// <returns>The result of the projection.</returns>
        public static Vector3 ProjectAndCreate(Vector3 v1, Vector3 v2)
        {
            double d = Vector3.Dot(v1,v2);
            double d_div = d / v2.Length;
            return new Vector3 (v2 * (float)d_div);
        }
        public static Vector3 Project(Vector3 a, Vector3 b)
        {
            return (Vector3.Dot(a, b) / Vector3.Dot(b, b)) * b;
        }

        /// <summary>
        /// Checks if the two given vectors are parallel 
        /// </summary>
        /// <param name="a">The first vector</param>
        /// <param name="b">The secound vector</param>
        /// <param name="precision">Precison to be tollerated </param>
        /// <returns>True if the two vectors are parallel or any of the vecor contains NaN, false other</returns>
        public static bool Parallel(Vector3 a, Vector3 b, float precision)
        {
            if (a.IsNaN() || b.IsNaN()) return true; // what?
            return Math.Abs((a.X / b.X) - (a.Y / b.Y)) < precision
                && Math.Abs((a.X / b.X) - (a.Z / b.Z)) < precision;
        }
    }
}
