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
    class ConstraintsExamples
    {

        private Vector4 
            Spine = new Vector4(20, 30, 20, 20),
            Neck = new Vector4(60, 85, 60, 85),
        
            Femur = new Vector4(15, 150, 40, 55),
            Knee = new Vector4(15, 0, 15, 160),
            Ankle = new Vector4(10, 10, 30, 30),
            FootBase = new Vector4(30, 30, 30, 30),
        
            Clavicula = new Vector4(15, 40, 30, 15),
            Shoulder = new Vector4(80, 95, 120, 120),
            Elbow = new Vector4(10, 175, 10, 5),
            Wrist = new Vector4(60, 30, 60, 45);

        private Vector2 
            NoTwist = new Vector2(359, 1),
            SpineTwist = new Vector2(340, 20),
            NeckTwist = new Vector2(270, 90),

            FemurTwist = new Vector2(300, 50),
            KneeTwist = new Vector2(333, 30),
            AnkleTwist = new Vector2(340, 40),
            FootBaseTwist = new Vector2(350, 10),

            ClaviculaTwist = new Vector2(350, 10),
            ShoulderTwist = new Vector2(260, 40),
            ElbowTwist = new Vector2(300, 60),
            WristTwist = new Vector2(345, 15);

        private float 
            verystiff = 0.5f,
            barelymoving = 0.3f;
        
        public void SetConstraints(BipedSkeleton skeleton)
        {
            #region Cone constraints
            #region Spine too head
            skeleton[SegmentName.SPINE0].Constraints = (Spine);
            skeleton[SegmentName.SPINE1].Constraints = (Spine);
            skeleton[SegmentName.NECK].Constraints = (Neck);
            #endregion
            #region Legs
            skeleton[SegmentName.HIP_L].Constraints = (SwapXZ(Femur));
            skeleton[SegmentName.HIP_R].Constraints = (Femur);
            skeleton[SegmentName.KNEE_L].Constraints = (SwapXZ(Knee));
            skeleton[SegmentName.KNEE_R].Constraints = (Knee);
            skeleton[SegmentName.ANKLE_L].Constraints = (SwapXZ(Ankle));
            skeleton[SegmentName.ANKLE_R].Constraints = (Ankle);
            skeleton[SegmentName.FOOTBASE_L].Constraints = (SwapXZ(FootBase));
            skeleton[SegmentName.FOOTBASE_R].Constraints = (FootBase);
            #endregion
            #region Arms
            skeleton[SegmentName.CLAVICLE_L].Constraints = (SwapXZ(Clavicula));
            skeleton[SegmentName.CLAVICLE_R].Constraints = (Clavicula);
            skeleton[SegmentName.SHOULDER_L].Constraints = (SwapXZ(Shoulder));
            skeleton[SegmentName.SHOULDER_R].Constraints = (Shoulder);
            skeleton[SegmentName.ELBOW_L].Constraints = (SwapXZ(Elbow));
            skeleton[SegmentName.ELBOW_R].Constraints = (Elbow);
            skeleton[SegmentName.WRIST_L].Constraints = (SwapXZ(Wrist));
            skeleton[SegmentName.WRIST_R].Constraints = (Wrist);
            #endregion
            #endregion

            #region ParentPointers
            skeleton[SegmentName.CLAVICLE_R].ParentPointer = QuaternionHelper2.RotationZ(-MathHelper.PiOver2);
            skeleton[SegmentName.CLAVICLE_L].ParentPointer = QuaternionHelper2.RotationZ(MathHelper.PiOver2);
            skeleton[SegmentName.HIP_R].ParentPointer = QuaternionHelper2.RotationZ(MathHelper.Pi);
            skeleton[SegmentName.HIP_L].ParentPointer = QuaternionHelper2.RotationZ(MathHelper.Pi);
            skeleton[SegmentName.ANKLE_R].ParentPointer = QuaternionHelper2.RotationX(MathHelper.PiOver4) * QuaternionHelper2.RotationZ(-MathHelper.PiOver4);
            skeleton[SegmentName.ANKLE_L].ParentPointer = QuaternionHelper2.RotationX(MathHelper.PiOver4) * QuaternionHelper2.RotationZ(MathHelper.PiOver4);
            skeleton[SegmentName.FOOTBASE_L].ParentPointer = QuaternionHelper2.RotationX(MathHelper.PiOver4) *
                QuaternionHelper2.RotationZ(-MathHelper.PiOver4);
                skeleton[SegmentName.FOOTBASE_R].ParentPointer = QuaternionHelper2.RotationX(MathHelper.PiOver4) *
                QuaternionHelper2.RotationZ(MathHelper.PiOver4);
            #endregion

            #region TwistConstraints
            #region Spine
            skeleton[SegmentName.SPINE0].TwistLimit = (SpineTwist);
            skeleton[SegmentName.SPINE1].TwistLimit = (SpineTwist);
            skeleton[SegmentName.SPINE3].TwistLimit = (NoTwist);
            skeleton[SegmentName.NECK].TwistLimit = (NeckTwist);
            #endregion
            #region Legs
            skeleton[SegmentName.HIP_L].TwistLimit = (FemurTwist);
            skeleton[SegmentName.HIP_R].TwistLimit = (FemurTwist);
            skeleton[SegmentName.KNEE_L].TwistLimit = (KneeTwist);
            skeleton[SegmentName.KNEE_R].TwistLimit = (KneeTwist);
            skeleton[SegmentName.ANKLE_L].TwistLimit = (AnkleTwist);
            skeleton[SegmentName.ANKLE_R].TwistLimit = (AnkleTwist);

            skeleton[SegmentName.FOOTBASE_L].TwistLimit = (FootBaseTwist);
            skeleton[SegmentName.FOOTBASE_R].TwistLimit = (FootBaseTwist);
            #endregion
            #region Arms
            skeleton[SegmentName.CLAVICLE_L].TwistLimit = (ClaviculaTwist);
            skeleton[SegmentName.SHOULDER_L].TwistLimit = (ShoulderTwist);
            skeleton[SegmentName.ELBOW_L].TwistLimit = (ElbowTwist);
            skeleton[SegmentName.WRIST_L].TwistLimit = (WristTwist);
            skeleton[SegmentName.CLAVICLE_R].TwistLimit = (ClaviculaTwist);
            skeleton[SegmentName.SHOULDER_R].TwistLimit = (ShoulderTwist);
            skeleton[SegmentName.ELBOW_R].TwistLimit = (ElbowTwist);
            skeleton[SegmentName.WRIST_R].TwistLimit = (WristTwist);
            #endregion
            #endregion
            #region stiffness
            #region Arms
            skeleton[SegmentName.CLAVICLE_L].Stiffness = (verystiff);
            skeleton[SegmentName.CLAVICLE_R].Stiffness = (verystiff);

            skeleton[SegmentName.WRIST_L].Stiffness = (barelymoving);
            skeleton[SegmentName.WRIST_R].Stiffness = (barelymoving);
            #endregion
            #region Legs
            skeleton[SegmentName.ANKLE_L].Stiffness = (barelymoving);
            skeleton[SegmentName.ANKLE_R].Stiffness = (barelymoving);
            skeleton[SegmentName.FOOTBASE_L].Stiffness = (barelymoving);
            skeleton[SegmentName.FOOTBASE_R].Stiffness = (barelymoving);
            #endregion
            #endregion
        }
        private Vector4 SwapXZ(Vector4 v)
        {
            return new Vector4(v.Z, v.Y, v.X, v.W);
        }
        private Vector2 SwapXY(Vector2 v)
        {
            return new Vector2(v.Y, v.X);
        }
    }
}
