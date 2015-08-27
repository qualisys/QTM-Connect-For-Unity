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
using System;
using System.Linq;

namespace QualisysRealTime.Unity.Skeleton
{
    /// <summary>
    /// From Aristidou, Andreas, and Joan Lasenby. "FABRIK: a fast, iterative solver for the inverse kinematics problem." Graphical Models 73, no. 5 (2011): 243-260.
    /// </summary>
    class FABRIK : IKSolver
    {
        override public bool SolveBoneChain(Bone[] bones, Bone target, Bone parent)
        {
            // Calculate distances 
            float[] distances = GetDistances(ref bones);

            double dist = Math.Abs((bones[0].Pos - target.Pos).Length);
            if (dist > distances.Sum()) // the target is unreachable
            {
                TargetUnreachable(bones, target.Pos, parent);
                return true;
            }

            // The target is reachable
            int numberOfBones = bones.Length;
            bones[numberOfBones - 1].Orientation = target.Orientation;
            Vector3 root = bones[0].Pos;
            int iterations = 0;
            float lastDistToTarget = float.MaxValue;

            float distToTarget = (bones[bones.Length - 1].Pos - target.Pos).Length;
            while (distToTarget > threshold && iterations++ < MaxIterations && distToTarget < lastDistToTarget)
            {
                // Forward reaching
                ForwardReaching(ref bones, ref distances, target);
                // Backward reaching
                BackwardReaching(ref bones, ref distances, root, parent);

                lastDistToTarget = distToTarget;
                distToTarget = (bones[bones.Length - 1].Pos - target.Pos).Length;
            }
            bones[bones.Length - 1].Orientation = target.Orientation;

            return (distToTarget <= threshold);
        }


        /// <summary>
        /// The forward reaching phase
        /// </summary>
        /// <param name="bones"></param>
        /// <param name="distances"></param>
        /// <param name="target"></param>
        private void ForwardReaching(ref Bone[] bones, ref float[] distances, Bone target)
        {
            
            bones[bones.Length - 1].Pos = target.Pos;
            bones[bones.Length - 1].Orientation = target.Orientation; //TODO if bone is endeffector, we should not look at rot constraints
            for (int i = bones.Length - 2; i >= 0; i--)
            {
                SamePosCheck(ref bones, i);

                // Position
                Vector3 newPos;
                float r = (bones[i + 1].Pos - bones[i].Pos).Length;
                float l = distances[i] / r;

                newPos = (1 - l) * bones[i + 1].Pos + l * bones[i].Pos;

                bones[i].Pos = newPos;

                // Orientation
                bones[i].RotateTowards(bones[i + 1].Pos - bones[i].Pos);
            }
        }
        /// <summary>
        /// The backwards reaching phase
        /// </summary>
        /// <param name="bones"></param>
        /// <param name="distances"></param>
        /// <param name="root"></param>
        /// <param name="parent"></param>
        private void BackwardReaching(ref Bone[] bones, ref float[] distances, Vector3 root, Bone parent)
        {

            bones[0].Pos = root;

            for (int i = 0; i < bones.Length - 1; i++)
            {
                SamePosCheck(ref bones, i);
                Vector3 newPos;
                // Position
                float r = (bones[i + 1].Pos - bones[i].Pos).Length;
                float l = distances[i] / r;

                newPos = (1 - l) * bones[i].Pos + l * bones[i + 1].Pos;

                Bone prevBone = (i > 0) ? bones[i - 1] : parent;
                bones[i + 1].Pos = newPos;
                // Orientation
                bones[i].RotateTowards(bones[i + 1].Pos - bones[i].Pos,bones[i].Stiffness);
                if (bones[i].HasConstraints)
                {
                    Quaternion rot;
                    if (constraints.CheckOrientationalConstraint(bones[i], prevBone, out rot))
                    {
                        bones[i].Rotate(rot);
                    }

                }
            }
        }
        /// <summary>
        /// Checks whether two bones has the same position or not, then moves the bone a small amount
        /// </summary>
        /// <param name="bones">The bones to be checked</param>
        /// <param name="i">A inded of where in the array of bones we should start looking</param>
        private void SamePosCheck(ref Bone[] bones, int i) {
            if (bones[i+1].Pos == bones[i].Pos)
            {
                float small = 0.001f;
                // move one of them a small distance along the chain
                if (i+2 < bones.Length)
                {
                    Vector3 pushed = Vector3.Normalize(bones[i + 2].Pos - bones[i + 1].Pos) * small;
                        bones[i + 1].Pos += 
                            !pushed.IsNaN() ? 
                            pushed : 
                            new Vector3(small, small, small); ;
                }
                else if (i - 1 >= 0)
                {
                    Vector3 pushed = bones[i - 1].Pos +
                        Vector3.Normalize(bones[i - 1].Pos - bones[i].Pos) * small;
                    bones[i].Pos += !pushed.IsNaN() ? pushed : new Vector3(small, small, small); ;
                }
            }
        }
    }
}