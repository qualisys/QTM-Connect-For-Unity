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
using System.Collections.Generic;

namespace QualisysRealTime.Unity.Skeleton
{
    /// <summary>
    /// This class keeps the data collected from the markers such as subject length, chest depth and width, and an approximation of the weight.
    /// </summary>
    class BodyData
    {
        #region Read only data for body proportions
        public readonly static float MarkerCentreToSkinSurface = 0.009f;
        public readonly static float MarkerToSpineDist = 0.08f; // m
        public readonly static float MidHeadToHeadJoint = 0.04f; // m
        public readonly static float SpineLength = 0.0236f; // m
        public readonly static float BMI = 24;
        #endregion
        #region The collected body data
        public float Height { get; private set; }
        public float Mass { get; private set; }
        public Vector3 NeckToChestVector { get; private set; }
        public float ShoulderWidth { get; private set; }
        public float ChestDepth
        {
            get
            {
                if (NeckToChestVector != Vector3.Zero)
                {
                    return NeckToChestVector.Length * 1000;
                }
                else
                {
                    return 170f;
                }
            }
        }
        #endregion
        #region private frameCounters
        private uint chestsFrames = 0;
        private uint shoulderFrames = 0;
        private uint heightFrames = 0;
        #endregion
        private MarkersNames m;
        public BodyData(MarkersNames m)
        {
            this.m = m;
            Height = 175; // cm
            Mass = 75; //kg
            ShoulderWidth = 400; //mm
        }
        /// <summary>
        /// Using a markerset, calculates the needed data
        /// </summary>
        /// <param name="markers">The set of the markers</param>
        /// <param name="chestOrientation">An quaternion representing the orientation of the chest</param>
        public void CalculateBodyData(Dictionary<string, Vector3> markers, Quaternion chestOrientation)
        {
            // set chest depth
            var currentNeckToChestVector = (markers[m.chest] - markers[m.neck]);
            if (!currentNeckToChestVector.IsNaN() && !chestOrientation.IsNaN())
            {
                NeckToChestVector = 
                    (NeckToChestVector * chestsFrames
                    + Vector3.Transform(currentNeckToChestVector, Quaternion.Invert(chestOrientation))) 
                    / (++chestsFrames);
            }

            // set shoulder width
            float tmp = (markers[m.leftShoulder] - markers[m.rightShoulder]).Length * 500; // to mm half the width
            if (!float.IsNaN(tmp))// && tmp < 500)
            {
                ShoulderWidth = (ShoulderWidth * shoulderFrames + tmp) / (++shoulderFrames);
            }
            // height and mass
            tmp = ( (markers[m.rightOuterAnkle] - markers[m.rightOuterKnee]).LengthFast +
                    (markers[m.rightOuterKnee] - markers[m.rightHip]).LengthFast +
                    (markers[m.bodyBase] - markers[m.neck]).LengthFast +
                    (markers[m.neck] - markers[m.head]).LengthFast
                  ) * 100; // cm
            if (!float.IsNaN(tmp) && tmp < 250)
            {
                Height = (Height * heightFrames + tmp) / (++heightFrames);
                Mass = (Height / 100) * (Height / 100) * BMI; // BMI
            }
        }
    }
}
