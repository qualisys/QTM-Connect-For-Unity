#region --- LICENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015 Qualisys AB

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System.Collections.Generic;

namespace QualisysRealTime.Unity.Skeleton
{
    /// <summary>
    /// Class responsible for Building a Skeleton from markers
    /// </summary>
    public class SkeletonBuilder
    {
        private BipedSkeleton skeleton;
        private BipedSkeleton skeletonBuffer;
        private MarkersPreprocessor mp;
        private JointLocalization joints;
        private IKApplier ikApplier;

        private ŚegmentTracking st;
        
		public string MarkerPrefix;

        public int bodyHeight = 0;
        public int bodyMass = 0;

        public bool SolveWithIK = true;
        public bool Interpolation = false;

        public bool UseTrackingMarkers = true;

        /// <summary>
        /// Build a skeleton according to the markers
        /// </summary>
        /// <param name="markerData">A list of markers </param>
        /// <returns>A complete skeleton</returns>
        public BipedSkeleton SolveSkeleton(List<Marker> markerData)
        {
            if (skeleton == null
                || skeletonBuffer == null
                || mp == null
                || joints == null
                || ikApplier == null)
            {
                skeleton = new BipedSkeleton();
                skeletonBuffer = new BipedSkeleton();
                MarkersNames markersMap;
                mp = new MarkersPreprocessor(markerData, out markersMap, bodyPrefix: MarkerPrefix);
                joints = new JointLocalization(markersMap);
                ikApplier = new IKApplier(skeleton);

                // Set segment tracking markers for virtual marker construction
                st = new ŚegmentTracking(skeleton, markersMap, markerData);
            }

            var temp = skeleton;
            skeleton = skeletonBuffer;
            skeletonBuffer = temp;

            Dictionary<string, OpenTK.Vector3> markers;
            mp.UpdateMarkerList(markerData, out markers);
            mp.ProcessMarkers(out markers);

            joints.BodyData.Height = bodyHeight;
            joints.BodyData.Mass = bodyMass;
            joints.GetJointLocations(markers, ref skeleton);

            // Try to reconstruct virtual markers
            if (UseTrackingMarkers)
            {
                if (st.ProcessMarkers(skeleton, markerData, ref markers, MarkerPrefix))
                {
                    mp.ProcessMarkers(out markers);
                    joints.GetJointLocations(markers, ref skeleton);
                }
            }

            ikApplier.Interpolation = Interpolation;
            if (SolveWithIK) ikApplier.ApplyIK(ref skeleton);
            
			return skeleton;
        }

        /// <summary>
        /// E.Wolf, 2016-03-16
        /// </summary>
        /// <param name="height"></param>
        /// <param name="mass"></param>
        public void SetBodyData(int height, int mass)
        {
            if (height > 0)
                bodyHeight = height;
            if (mass > 0)
              bodyMass = mass;
        }
    }
}
