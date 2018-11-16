#region --- LICENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015-2018 Qualisys AB

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using OpenTK;
using System;
using System.Linq;

namespace QualisysRealTime.Unity.Skeleton
{
    /// <summary>
    /// From Aristidou, Andreas, and Joan Lasenby. "FABRIK: a fast, iterative solver for the inverse kinematics problem." Graphical Models 73, no. 5 (2011): 243-260.
    /// </summary>
    class FABRIK : IKSolver
    {
        override public bool SolveSegmentChain(Segment[] segments, Segment target, Segment parent)
        {
            // Calculate distances 
            float[] distances = GetDistances(ref segments);

            double dist = Math.Abs((segments[0].Pos - target.Pos).Length);
            if (dist > distances.Sum()) // the target is unreachable
            {
                TargetUnreachable(segments, target.Pos, parent);
                return true;
            }

            // The target is reachable
            int numberOfSegments = segments.Length;
            segments[numberOfSegments - 1].Orientation = target.Orientation;
            Vector3 root = segments[0].Pos;
            int iterations = 0;
            float lastDistToTarget = float.MaxValue;

            float distToTarget = (segments[segments.Length - 1].Pos - target.Pos).Length;
            while (distToTarget > distanceThreshold && iterations++ < MaxIterations && distToTarget < lastDistToTarget)
            {
                // Forward reaching
                ForwardReaching(ref segments, ref distances, target);
                // Backward reaching
                BackwardReaching(ref segments, ref distances, root, parent);

                lastDistToTarget = distToTarget;
                distToTarget = (segments[segments.Length - 1].Pos - target.Pos).Length;
            }
            segments[segments.Length - 1].Orientation = target.Orientation;

            return (distToTarget <= distanceThreshold);
        }


        /// <summary>
        /// The forward reaching phase
        /// </summary>
        /// <param name="segments"></param>
        /// <param name="distances"></param>
        /// <param name="target"></param>
        private void ForwardReaching(ref Segment[] segments, ref float[] distances, Segment target)
        {
            
            segments[segments.Length - 1].Pos = target.Pos;
            segments[segments.Length - 1].Orientation = target.Orientation; //TODO if segment is endeffector, we should not look at rot constraints
            for (int i = segments.Length - 2; i >= 0; i--)
            {
                SamePosCheck(ref segments, i);

                // Position
                Vector3 newPos;
                float r = (segments[i + 1].Pos - segments[i].Pos).Length;
                float l = distances[i] / r;

                newPos = (1 - l) * segments[i + 1].Pos + l * segments[i].Pos;

                segments[i].Pos = newPos;

                // Orientation
                segments[i].RotateTowards(segments[i + 1].Pos - segments[i].Pos);
            }
        }
        /// <summary>
        /// The backwards reaching phase
        /// </summary>
        /// <param name="segments"></param>
        /// <param name="distances"></param>
        /// <param name="root"></param>
        /// <param name="parent"></param>
        private void BackwardReaching(ref Segment[] segments, ref float[] distances, Vector3 root, Segment parent)
        {

            segments[0].Pos = root;

            for (int i = 0; i < segments.Length - 1; i++)
            {
                SamePosCheck(ref segments, i);
                Vector3 newPos;
                // Position
                float r = (segments[i + 1].Pos - segments[i].Pos).Length;
                float l = distances[i] / r;

                newPos = (1 - l) * segments[i].Pos + l * segments[i + 1].Pos;

                Segment prevSegments = (i > 0) ? segments[i - 1] : parent;
                segments[i + 1].Pos = newPos;
                // Orientation
                segments[i].RotateTowards(segments[i + 1].Pos - segments[i].Pos,segments[i].Stiffness);
                if (segments[i].HasConstraints)
                {
                    Quaternion rot;
                    if (constraints.CheckOrientationalConstraint(segments[i], prevSegments, out rot))
                    {
                        segments[i].Rotate(rot);
                    }

                }
            }
        }
        /// <summary>
        /// Checks whether two segments has the same position or not, then moves the segment a small amount
        /// </summary>
        /// <param name="segments">The segments to be checked</param>
        /// <param name="i">A inded of where in the array of segments we should start looking</param>
        private void SamePosCheck(ref Segment[] segments, int i) {
            if (segments[i+1].Pos == segments[i].Pos)
            {
                float small = 0.001f;
                // move one of them a small distance along the chain
                if (i+2 < segments.Length)
                {
                    Vector3 pushed = Vector3.Normalize(segments[i + 2].Pos - segments[i + 1].Pos) * small;
                        segments[i + 1].Pos += 
                            !pushed.IsNaN() ? 
                            pushed : 
                            new Vector3(small, small, small); ;
                }
                else if (i - 1 >= 0)
                {
                    Vector3 pushed = segments[i - 1].Pos +
                        Vector3.Normalize(segments[i - 1].Pos - segments[i].Pos) * small;
                    segments[i].Pos += !pushed.IsNaN() ? pushed : new Vector3(small, small, small); ;
                }
            }
        }
    }
}