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
    [System.Serializable]
    public class Bone : IEquatable<Bone>
    {
        #region Name, pos, rot and constraints getters and setters
        public bool Exists
        {
            get { return !pos.IsNaN() && pos != Vector3.Zero; }
        }
        public bool HasNaN
        {
            get { return pos.IsNaN() || orientation.IsNaN(); }
        }
        private Joint name;
        public Joint Name
        {
            get { return name; }
        }

        private Vector3 pos;
        public Vector3 Pos
        {
            get { return pos; }
            set
            {
                pos = new Vector3(value);
            }
        }
        private Quaternion orientation;
        public Quaternion Orientation
        {
            get { return orientation; }
            set { orientation = value != QuaternionHelper2.Zero ?  Quaternion.Normalize(value) : value; }
        }
        #region constraints getters and setters

        // An orientational constraint is the twist of the bone around its own direction vector
        // with respect to its parent
        // It is defined as an allowed range betwen angles [start,end]
        // where start != end && 0 < start, end <= 360
        // If both start and end is 0 no twist constraint exist
        private Vector2 twistLimit = new Vector2(-1, -1);

        public Vector2 TwistLimit
        {
            get { return twistLimit; }
            set { twistLimit = new Vector2(value.X,value.Y); }
        }
        public bool HasTwistConstraints
        {
            get { return (twistLimit.X >= 0 && twistLimit.Y >= 0); }
        }
        public float StartTwistLimit
        {
            get { return twistLimit.X; }
        }
        public float EndTwistLimit
        {
            get { return twistLimit.Y; }
        }

        private Vector4 constraints = Vector4.Zero;
        public Vector4 Constraints
        {
            get { return new Vector4(constraints); }
            set { constraints = new Vector4(value); }
        }
        public bool HasConstraints
        {
            get { return (constraints != Vector4.Zero); }
        }
        private Quaternion parentPointer = Quaternion.Identity;
        public Quaternion ParentPointer
        {
            get {return parentPointer;}
            set {parentPointer = value;}
        }
        private float stiffness = 1;
        public float Stiffness
        {
            get { return stiffness; }
            set { stiffness = value; }
        }
        #endregion
        #endregion

        #region Constructors
        public Bone(Joint name)
        {
            this.name = name;
        }

        public Bone(Joint name, Vector3 position)
            : this(name)
        {
            pos = position;
        }

        public Bone(Joint name, Vector3 position, Quaternion orientation)
            : this(name, position)
        {
            this.orientation = orientation;
        }
        public Bone(Joint name, Vector3 position, Quaternion orientation, Vector4 constriants)
            : this(name, position, orientation)
        { 
            Constraints = constriants;
        }
        #endregion

        // Directions 
        #region Direction getters
        public Vector3 GetXAxis()
        {
            return Vector3.NormalizeFast(Vector3.Transform(Vector3.UnitX, orientation));
        }

        public Vector3 GetYAxis()
        {
            return Vector3.NormalizeFast(Vector3.Transform(Vector3.UnitY, orientation));
        }

        public Vector3 GetZAxis()
        {
            return Vector3.NormalizeFast(Vector3.Transform(Vector3.UnitZ, orientation));
        }

        #endregion
        #region rotation methods

        public void Rotate(Quaternion rotation)
        {
            orientation = rotation * orientation;
        }

        public void RotateTowards(Vector3 v, float stiffness = 1f)
        {
            Rotate(QuaternionHelper2.GetRotationBetween(GetYAxis(), v, stiffness = this.stiffness));
        }
        #endregion
        public bool Equals(Bone other)
        {
            return name.Equals(other.Name) && orientation.Equals(other.Orientation) && pos.Equals(other.Pos);
        }

        public override string ToString()
        {
            return string.Format("{0} at position: {1} with orientation: {2}", name, pos, orientation);
        }
        public bool IsArm()
        {
            return
                //name == Joint.CLAVICLE_L ||
                //name == Joint.CLAVICLE_R ||
                name == Joint.SHOULDER_L ||
                name == Joint.SHOULDER_R ||
                name == Joint.ELBOW_L ||
                name == Joint.ELBOW_R ||
                name == Joint.HAND_L ||
                name == Joint.HAND_R;
        }
        public bool IsLeg()
        {
            return
                name == Joint.KNEE_L ||
                name == Joint.KNEE_R ||
                name == Joint.ANKLE_L ||
                name == Joint.ANKLE_R ||
                name == Joint.HIP_L ||
                name == Joint.HIP_R;
        }
    }
}
