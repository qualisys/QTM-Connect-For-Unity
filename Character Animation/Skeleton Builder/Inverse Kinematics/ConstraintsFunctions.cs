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
    /// <summary>
    /// This class contains the functions for ensuring constraints of a bone
    /// </summary>
    class ConstraintsFunctions
    {
        // An orientational constraint is the twist of the bone around its own direction vector
        // with respect to its parent
        // It is defined as an allowed range betwen angles [start,end]
        // where start != end && 0 < start, end <= 360
        // If both start and end is 0 no twist constraint exist
        /// <summary>
        /// Checks if the bone has a legal rotation in regards to its parent, returns true if legal, false otherwise.
        /// The out rotation gives the rotation the bone should be applied to to be inside the twist constraints
        /// </summary>
        /// <param name="b">The bone under consideration</param>
        /// <param name="refBone">The parent of the bone, to check whether the child has legal rotation</param>
        /// <param name="rotation">The rotation bone b should applie to be inside the constraints</param>
        /// <returns></returns>
        public bool CheckOrientationalConstraint(Bone b, Bone refBone, out Quaternion rotation)
        {
            if (b.Orientation.Xyz.IsNaN() || refBone.Orientation.Xyz.IsNaN())
            {
                rotation = Quaternion.Identity;
                return false;
            }
            Vector3 thisY = b.GetYAxis();
            Quaternion referenceRotation = refBone.Orientation * b.ParentPointer;
            Vector3 reference = Vector3.Transform(
                    Vector3.Transform(Vector3.UnitZ, referenceRotation),
                    QuaternionHelper2.GetRotationBetween(
                            Vector3.Transform(Vector3.UnitY, referenceRotation),
                            thisY));

            float twistAngle = MathHelper.RadiansToDegrees(Vector3.CalculateAngle(reference, b.GetZAxis()));

            if (Vector3.CalculateAngle(reference, b.GetXAxis()) > Mathf.PI / 2) // b is twisted left with respect to parent
                twistAngle = 360 - twistAngle;

            float leftLimit = b.StartTwistLimit;
            float rightLimit = b.EndTwistLimit;

            float precision = 0.5f;
            bool inside = (leftLimit >= rightLimit) ? // The allowed range is on both sides of the reference vector
                    inside = twistAngle - leftLimit >= precision || twistAngle - rightLimit <= precision :
                    inside = twistAngle - leftLimit >= precision && twistAngle - rightLimit <= precision;

            if (!inside) // not inside constraints 
            {
                // Create a rotation to the closest limit
                float toLeft = Math.Min(360 - Math.Abs(twistAngle - leftLimit), Math.Abs(twistAngle - leftLimit));
                float toRight = Math.Min(360 - Math.Abs(twistAngle - rightLimit), Math.Abs(twistAngle - rightLimit));
                if (toLeft < toRight)
                {
                    // Anti-clockwise rotation to left limit
                    rotation = Quaternion.FromAxisAngle(thisY, -MathHelper.DegreesToRadians(toLeft));
                    return true;
                }
                else
                {
                    // Clockwise to right limit
                    rotation = Quaternion.FromAxisAngle(thisY, MathHelper.DegreesToRadians(toRight));
                    return true;
                }
            }
            rotation = Quaternion.Identity;
            return false;
        }
        /// <summary>
        /// What quadrant the bone is in regarding
        /// </summary>
        private enum Quadrant { q1, q2, q3, q4 };
        private float precision = 0.01f;
        /// <summary>
        /// Check the positional constraints of the bone, if the bone is inside the legal cone, returns true, otherwise false
        /// Originally modeled from Andreas Aristidou and Joan Lasenby FABRIK: A fast, iterative solver for the Inverse Kinematics problem
        /// </summary>
        /// <param name="joint">The bone to be checked if its has a legal position</param>
        /// <param name="parentsRots">The rotation if the parent</param>
        /// <param name="target">The position of the joint</param>
        /// <param name="res">The resulting position, the legal position of the bone if its legal, the same as target otherwise</param>
        /// <param name="rot">The rotation that should be applied to the bone if it has a illegal rotation, Identity otherwise</param>
        /// <returns></returns>
        public bool CheckRotationalConstraints(Bone joint, Quaternion parentsRots, Vector3 target, out Vector3 res, out Quaternion rot)
        {
            Quaternion referenceRotation = parentsRots * joint.ParentPointer;
            Vector3 L1 = Vector3.Normalize(Vector3.Transform(Vector3.UnitY, referenceRotation));

            Vector3 jointPos = joint.Pos;
            Vector4 constraints = joint.Constraints;
            Vector3 targetPos = new Vector3(target.X, target.Y, target.Z);
            Vector3 joint2Target = (targetPos - jointPos);

            bool behind = false;
            bool reverseCone = false;
            bool sideCone = false;
            //3.1 Find the line equation L1
            //3.2 Find the projection O of the target t on line L1
            Vector3 O = Vector3Helper.Project(joint2Target, L1);
            if (Math.Abs(Vector3.Dot(L1, joint2Target)) < precision) // target is ortogonal with L1
            {
                O = Vector3.NormalizeFast(L1) * precision;
            }
            else if (Math.Abs(Vector3.Dot(O, L1) - O.LengthFast * L1.LengthFast) >= precision) // O not same direction as L1
            {
                behind = true;
            }
            //3.3 Find the distance between the point O and the joint position
            float S = O.Length;

            //3.4 Map the target (rotate and translate) in such a
            //way that O is now located at the axis origin and oriented
            //according to the x and y-axis ) Now it is a 2D simplified problem
            Quaternion rotation = Quaternion.Invert(referenceRotation);//Quaternion.Invert(parent.Orientation);
            Vector3 TRotated = Vector3.Transform(joint2Target, rotation); // align joint2target vector to  y axis get x z offset
            Vector2 target2D = new Vector2(TRotated.X, TRotated.Z); //only intrested in the X Z cordinates
            //3.5 Find in which quadrant the target belongs 
            // Locate target in a particular quadrant
            //3.6 Find what conic section describes the allowed
            //range of motion
            Vector2 radius;
            Quadrant q;
            #region find Quadrant
            if (target2D.X >= 0 && target2D.Y >= 0)
            {
                radius = new Vector2(constraints.X, constraints.Y);
                q = Quadrant.q1;
            }
            else if (target2D.X >= 0 && target2D.Y < 0)
            {
                q = Quadrant.q2;
                radius = new Vector2(constraints.X, constraints.W);
            }
            else if (target2D.X < 0 && target2D.Y < 0)
            {
                q = Quadrant.q3;
                radius = new Vector2(constraints.Z, constraints.W);
            }
            else //if (target.X > 0 && target.Y < 0)
            {
                q = Quadrant.q4;
                radius = new Vector2(constraints.Z, constraints.Y);
            }

            #endregion
            #region check cone
            if (radius.X > 90 && radius.Y > 90) // cone is reversed if  both angles are larget then 90 degrees
            {
                reverseCone = true;
                radius.X = 90 - (radius.X - 90);
                radius.Y = 90 - (radius.Y - 90);
            }
            else if (behind && radius.X <= 90 && radius.Y <= 90) // target behind and cone i front
            {
                O = -Vector3.NormalizeFast(O) * precision;
                S = O.Length;
            }
            else if (radius.X > 90 || radius.Y > 90) // has one angle > 90, other not, very speciall case
            {
                Vector3 L2 = GetNewL(rotation, q, radius);
                if (!behind) L2 = (L2 - L1) / 2 + L1;
                float angle = Vector3.CalculateAngle(L2, L1);
                Vector3 axis = Vector3.Cross(L2, L1);
                rotation = rotation * Quaternion.FromAxisAngle(axis, angle);
                TRotated = Vector3.Transform(joint2Target, rotation);
                target2D = new Vector2(TRotated.X, TRotated.Z);
                O = Vector3Helper.Project(joint2Target, L2);
                if (Math.Abs(Vector3.Dot(L2, joint2Target)) < precision) // target is ortogonal with L2
                {
                    O = Vector3.Normalize(L2) * precision;
                }
                S = behind ? O.Length : O.Length * 1.4f; //magic number
                if (behind)
                {
                    sideCone = true;
                    if (radius.X > 90)
                    {
                        radius.X = (radius.X - 90);
                    }
                    else
                    {
                        radius.Y = (radius.Y - 90);
                    }
                }
            }
            #endregion

            radius.X = Mathf.Clamp(radius.X, precision, 90 - precision);  // clamp it so if <=0 -> 0.01, >=90 -> 89.99
            radius.Y = Mathf.Clamp(radius.Y, precision, 90 - precision);

            //3.7 Find the conic section which is associated with
            //that quadrant using the distances qj = Stanhj, where
            //j = 1,..,4
            float radiusX = S * Mathf.Tan(MathHelper.DegreesToRadians(radius.X));
            float radiusY = S * Mathf.Tan(MathHelper.DegreesToRadians(radius.Y));

            //3.8 Check whether the target is within the conic section or not
            bool inside = (target2D.X * target2D.X) / (radiusX * radiusX) +
                (target2D.Y * target2D.Y) / (radiusY * radiusY) <= 1 + precision;
            //3.9 if within the conic section then         
            if ((inside && !behind)
                || (!inside && reverseCone)
                || (inside && sideCone)
               )
            {
                //3.10 use the true target position t
                res = target;
                rot = Quaternion.Identity;
                return false;
            }
            //3.11 else
            else
            {
                //3.12 Find the nearest point on that conic section from the target
                Vector2 newPoint = NearestPoint(radiusX, radiusY, target2D);
                Vector3 newPointV3 = new Vector3(newPoint.X, 0.0f, newPoint.Y);

                //3.13 Map (rotate and translate) that point on the
                //conic section via reverse of 3.4 and use that point as
                //the new target position
                rotation = Quaternion.Invert(rotation);
                Vector3 vectorToMoveTo = Vector3.Transform(newPointV3, rotation) + O;
                Vector3 axis = Vector3.Cross(joint2Target, vectorToMoveTo);
                float angle = Vector3.CalculateAngle(joint2Target, vectorToMoveTo);
                rot = Quaternion.FromAxisAngle(axis, angle);
                res = Vector3.Transform(joint2Target, rot) + jointPos;
                return true;
            }
            //3.14 end
        }
        /// <summary>
        /// The new line on which the cone should be modeled around
        /// </summary>
        /// <param name="rotation">The rotation of the cone</param>
        /// <param name="q">The quadrant the bone is inside</param>
        /// <param name="radius">The x,y radius of the cone</param>
        /// <returns>The new Line on which the cone should be modeled</returns>
        private Vector3 GetNewL(Quaternion rotation, Quadrant q, Vector2 radius)
        {
            Quaternion inverRot = Quaternion.Invert(rotation);
            Vector3 right = Vector3.Transform(Vector3.UnitX, inverRot);
            Vector3 forward = Vector3.Transform(Vector3.UnitZ, inverRot);
            Vector3 L2;
            switch (q)
            {
                case Quadrant.q1:
                    if (radius.X > 90) L2 = right;
                    else L2 = forward;
                    break;
                case Quadrant.q2:
                    if (radius.X > 90) L2 = right;
                    else L2 = -forward;
                    break;
                case Quadrant.q3:
                    if (radius.X > 90) L2 = -right;
                    else L2 = -forward;
                    break;
                case Quadrant.q4:
                    if (radius.X > 90) { L2 = -right; }
                    else L2 = forward;
                    break;
                default:
                    L2 = right;
                    break;
            }
            L2.NormalizeFast();
            return L2;
        }
        private Vector2 NearestPoint(float radiusX, float radiusY, Vector2 target2D)
        {
            Vector2 newPoint;
            float xRad, yRad, pX, pY;
            if (radiusX >= radiusY)
            {
                xRad = Math.Abs(radiusX);
                yRad = Math.Abs(radiusY);
                pX = Math.Abs(target2D.X);
                pY = Math.Abs(target2D.Y);
                newPoint =
                    Mathf.FindNearestPointOnEllipse
                    (xRad, yRad, new Vector2(pX, pY));
            }
            else
            {
                xRad = Math.Abs(radiusY);
                yRad = Math.Abs(radiusX);
                pX = Math.Abs(target2D.Y);
                pY = Math.Abs(target2D.X);
                newPoint =
                    Mathf.FindNearestPointOnEllipse
                    (xRad, yRad, new Vector2(pX, pY));
                MathHelper.Swap(ref newPoint.X, ref newPoint.Y);
            }
            if (target2D.X < 0) newPoint.X = -newPoint.X;
            if (target2D.Y < 0) newPoint.Y = -newPoint.Y;
            return newPoint;
        }
    }

}
