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
    class SkeletonBuilder
    {
        private BipedSkeleton skeleton;
        private BipedSkeleton skeletonBuffer;
        private MarkersPreprocessor mp;
        private JointLocalization joints;
        private IKApplier ikApplier;
        public string MarkerPrefix;
        public bool SolveWithIK = true;
        public bool Interpolation = false;
        /// <summary>
        /// Build a skeleton according to the markers
        /// </summary>
        /// <param name="markerData">A list of markers </param>
        /// <returns>A complete skeleton</returns>
        public BipedSkeleton SolveSkeleton(List<LabeledMarker> markerData)
        {
            if (   skeleton == null
                || skeletonBuffer == null
                || mp == null
                || joints == null
                || ikApplier == null)
            {
                skeleton = new BipedSkeleton();
                skeletonBuffer = new BipedSkeleton();
                MarkersNames markersMap;
                mp = new MarkersPreprocessor(markerData, out markersMap, bodyPrefix: MarkerPrefix); ;
                joints = new JointLocalization(markersMap);
                ikApplier = new IKApplier(skeleton);
            }
            Dictionary<string, OpenTK.Vector3> markers;
            mp.ProcessMarkers(markerData, out markers, MarkerPrefix);
            var temp = skeleton;
            skeleton = skeletonBuffer;
            skeletonBuffer = temp;
            joints.GetJointLocation(markers, ref skeleton);
            ikApplier.Interpolation = Interpolation;
            if (SolveWithIK) ikApplier.ApplyIK(ref skeleton);
            return skeleton;
        }
    }
}
