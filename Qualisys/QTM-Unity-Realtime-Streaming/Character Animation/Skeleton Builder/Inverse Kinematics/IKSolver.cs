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

namespace QualisysRealTime.Unity.Skeleton
{
    /// <summary>
    /// An abstract class containing the functions for a Inverse kinematic solver
    /// </summary>
    abstract class IKSolver
    {
        abstract public bool SolveSegmentChain(Segment[] segments, Segment target, Segment parent);
        protected float distanceThreshold = 0.01f;
        public ConstraintsFunctions constraints = new ConstraintsFunctions();
        public int MaxIterations = 140;
        /// <summary>
        /// Checks whether the target is reachable for the segment chain or not
        /// </summary>
        /// <param name="segments">The chain of segment, the IK Chain</param>
        /// <param name="target">The target</param>
        /// <returns>True if target is reachable, false otherwise</returns>
        protected bool IsReachable(Segment[] segments, Segment target)
        {
            float acc = 0;
            for (int i = 0; i < segments.Length - 1; i++)
            {
                acc += (segments[i].Pos - segments[i + 1].Pos).Length;
            }
            float dist = System.Math.Abs((segments[0].Pos - target.Pos).Length);
            if (float.IsNaN(dist) || float.IsNaN(acc))
                return false;
            return dist < acc;
        }
        /// <summary>
        /// Gets the total length from the root of each segment in the kinematic chain (the segment chain)
        /// </summary>
        /// <param name="distances"></param>
        /// <param name="segments"></param>
        protected float[] GetDistances(ref Segment[] segments)
        {
            float[] distances = new float[segments.Length - 1];
            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = (segments[i].Pos - segments[i + 1].Pos).Length;
            }
            return distances;
        }
        /// <summary>
        /// Stretches the chain to a maximum towards the target, assuming it is unreachable
        /// </summary>
        /// <param name="segments">The kimematic chain, the segments</param>
        /// <param name="target">The target to be reach towards</param>
        /// <param name="grandparent">The parent of the first segment in the kinematic chain, used for constraints</param>
        /// <returns></returns>
        protected Segment[] TargetUnreachable(Segment[] segments, Vector3 target, Segment grandparent)
        {
            float[] distances = GetDistances(ref segments);
            
            for (int i = 0; i < distances.Length; i++)
            {
                // Position
                float r = (target - segments[i].Pos).Length;
                float l = distances[i] / r;
                Vector3 newPos = ((1 - l) * segments[i].Pos) + (l * target);
                segments[i + 1].Pos = newPos;
                // Orientation
                segments[i].RotateTowards(segments[i + 1].Pos - segments[i].Pos);
                if (segments[i].HasTwistConstraints)
                {
                    Quaternion rotation2;
                    if (constraints.CheckOrientationalConstraint(segments[i], (i > 0) ? segments[i - 1] : grandparent, out rotation2))
                    {
                        ForwardKinematics(ref segments, rotation2, i);
                    }
                }
            }
            return segments;
        }
        protected void ForwardKinematics(ref Segment[] segments, Quaternion rotation, int i = 0)
        {
            ForwardKinematics(ref segments, rotation, i, segments.Length-1);
        }
        /// <summary>
        /// Forward kinematic function
        /// </summary>
        /// <param name="segments">The kinamatic chain, array of segments</param>
        /// <param name="rotation">The rotation to be alplied to the chain</param>
        /// <param name="i">An index of where in the chain the rotation should starts</param>
        /// <param name="length">The amount of segments in the chain the kinamatics should be applied to</param>
        protected void ForwardKinematics(ref Segment[] segments, Quaternion rotation, int i, int length)
        {
            for (int j = length; j >= i; j--)
            {
                if (j > i)
                {
                    segments[j].Pos = segments[i].Pos + Vector3.Transform((segments[j].Pos - segments[i].Pos), rotation);
                }
                // rotate orientation
                segments[j].Rotate(rotation);
            }
        }
    }
}
