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
    class CCD : IKSolver
    {
        private int degreeStep = 10;
        /// <summary>
        /// Given a array of bones, and a target bone, solves the chain so that the last bone in the chain is at at the same position as the target
        /// </summary>
        /// <param name="bones">An array of bones, the chain to be solved by IK</param>
        /// <param name="target">The target for the chain</param>
        /// <param name="grandparent">the parent of the first bone in the bones chain, is used to ensure constraints</param>
        /// <returns>True if target was reached, false if maximum iteration was reached first   </returns>
        override public bool SolveBoneChain(Bone[] bones, Bone target, Bone grandparent)
        {

            if (!IsReachable(bones,target))
            {
                TargetUnreachable(bones, target.Pos, grandparent);
                bones[bones.Length - 1].Orientation = new Quaternion(target.Orientation.Xyz, target.Orientation.W);
                return true;
            }

            int numberOfBones = bones.Length;
            int iter = 0;
            int degrees = degreeStep;
            bool toggle = false;
            bool doneOneLapAroundYAxis = false;
            int maxdegrees = 120;
            float lastDistanceToTarget = float.MaxValue;
            float distanceToTarget = (bones[bones.Length - 1].Pos - target.Pos).Length;
            // main loop
            while (distanceToTarget > threshold && MaxIterations > ++iter)
            {
                // if CCD is stuck becouse of constraints, we twist the chain
                if (distanceToTarget >= lastDistanceToTarget)
                {
                    if (!doneOneLapAroundYAxis && degrees > maxdegrees)
                    {
                        doneOneLapAroundYAxis = true;
                        degrees = degreeStep;
                    }
                    else if (doneOneLapAroundYAxis && degrees > maxdegrees)
                    {
                        break;
                    }
                    Quaternion q = doneOneLapAroundYAxis ?
                        QuaternionHelper2.RotationX(MathHelper.DegreesToRadians(toggle ? degrees : -degrees))
                      :
                       QuaternionHelper2.RotationY(MathHelper.DegreesToRadians(toggle ? degrees : -degrees));
                    ForwardKinematics(ref bones, q);
                    if (toggle) degrees += degreeStep;
                    toggle = !toggle;
                }

                // for each bone, starting with the one closest to the end effector 
                // (but not the end effector itself)
                Vector3 a, b;
                Quaternion rotation;
                for (int i = numberOfBones - 2; i >= 0; i--)
                {
                    // Get the vectors between the points
                    a = bones[numberOfBones - 1].Pos - bones[i].Pos;
                    b = target.Pos - bones[i].Pos;
                    // Make a rotation quaternion and rotate 
                    // - first the endEffector
                    // - then the rest of the affected joints
                    rotation = (a.LengthFast == 0 || b.LengthFast == 0) ? Quaternion.Identity
                        : QuaternionHelper2.GetRotationBetween(a, b, bones[i].Stiffness);

                    if (bones[i].HasConstraints)
                    {
                        Vector3 res;
                        Quaternion rot;
                        if (constraints.CheckRotationalConstraints(
                            bones[i],
                            ((i > 0) ? bones[i - 1] : grandparent).Orientation, //Reference
                            bones[i].Pos + Vector3.Transform(bones[i + 1].Pos - bones[i].Pos, rotation), // Target
                            out res, out rot))
                        {
                            rotation = rot * rotation;
                        }
                    }
                    // Move the chain
                    ForwardKinematics(ref bones, rotation, i);
                    // Check for twist constraints
                    if (bones[i].HasTwistConstraints)
                    {
                        Quaternion rotation2;
                        if (constraints.CheckOrientationalConstraint(bones[i], (i > 0) ? bones[i - 1] : grandparent, out rotation2))
                        {
                            ForwardKinematics(ref bones, rotation2, i);
                        }
                    }
                }
                lastDistanceToTarget = distanceToTarget;
                distanceToTarget = (bones[bones.Length - 1].Pos - target.Pos).LengthFast;
            }
            // Copy the targets rotation so that rotation is consistant
            bones[bones.Length - 1].Orientation = new Quaternion (target.Orientation.Xyz,target.Orientation.W);
            return (distanceToTarget <= threshold);
        }
    }
}
