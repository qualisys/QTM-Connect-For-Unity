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
    /// <summary>
    /// An abstract class containing the functions for a Inverse kinematic solver
    /// </summary>
    abstract class IKSolver
    {
        abstract public bool SolveBoneChain(Bone[] bones, Bone target, Bone parent);
        protected float threshold = 0.01f;
        public ConstraintsFunctions constraints = new ConstraintsFunctions();
        public int MaxIterations = 140;
        /// <summary>
        /// Checks wheter the target is reacheble for the bone chain or not
        /// </summary>
        /// <param name="bones">The chain of bone, the IK Chain</param>
        /// <param name="target">The target</param>
        /// <returns>True if target is reachable, false otherwise</returns>
        protected bool IsReachable(Bone[] bones, Bone target)
        {
            float acc = 0;
            for (int i = 0; i < bones.Length - 1; i++)
            {
                acc += (bones[i].Pos - bones[i + 1].Pos).Length;
            }
            float dist = System.Math.Abs((bones[0].Pos - target.Pos).Length);
            return dist < acc; // the target is unreachable
        }
        /// <summary>
        /// Gets the total length from the root of each bone in the kinematic chain (the bone chain)
        /// </summary>
        /// <param name="distances"></param>
        /// <param name="bones"></param>
        protected float[] GetDistances(ref Bone[] bones)
        {
            float[] distances = new float[bones.Length - 1];
            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = (bones[i].Pos - bones[i + 1].Pos).Length;
            }
            return distances;
        }
        /// <summary>
        /// Streches the chain to a maximum towards the target, assuming it is unreachabel
        /// </summary>
        /// <param name="bones">The kimematic chain, the bones</param>
        /// <param name="target">The target to be reach towards</param>
        /// <param name="grandparent">The parent of the first bone in the kinematic chain, used for constraints</param>
        /// <returns></returns>
        protected Bone[] TargetUnreachable(Bone[] bones, Vector3 target, Bone grandparent)
        {
            float[] distances = GetDistances(ref bones);
            
            for (int i = 0; i < distances.Length; i++)
            {
                // Position
                float r = (target - bones[i].Pos).Length;
                float l = distances[i] / r;
                Vector3 newPos = ((1 - l) * bones[i].Pos) + (l * target);
                bones[i + 1].Pos = newPos;
                // Orientation
                bones[i].RotateTowards(bones[i + 1].Pos - bones[i].Pos);
                if (bones[i].HasTwistConstraints)
                {
                    Quaternion rotation2;
                    if (constraints.CheckOrientationalConstraint(bones[i], (i > 0) ? bones[i - 1] : grandparent, out rotation2))
                    {
                        ForwardKinematics(ref bones, rotation2, i);
                    }
                }
            }
            return bones;
        }
        protected void ForwardKinematics(ref Bone[] bones, Quaternion rotation, int i = 0)
        {
            ForwardKinematics(ref bones, rotation, i, bones.Length-1);
        }
        /// <summary>
        /// Forward kinamatic function
        /// </summary>
        /// <param name="bones">The kinamatic chain, array of bones</param>
        /// <param name="rotation">The rotation to be alplied to the chain</param>
        /// <param name="i">An index of where in the chain the rotation should starts</param>
        /// <param name="length">The amount of bones in the chain the kinamatics should be applied to</param>
        protected void ForwardKinematics(ref Bone[] bones, Quaternion rotation, int i, int length)
        {
            for (int j = length; j >= i; j--)
            {
                if (j > i)
                {
                    bones[j].Pos = bones[i].Pos +
                        Vector3.Transform((bones[j].Pos - bones[i].Pos), rotation);
                }
                // rotate orientation
                bones[j].Rotate(rotation);
            }
        }
    }
}
