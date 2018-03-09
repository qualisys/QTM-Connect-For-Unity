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
    /// Class for standrad ISO/IEC FCD 19774 specification of joints
    /// http://h-anim.org/Specifications/H-Anim200x/ISO_IEC_FCD_19774/
    /// </summary>
    class HAminStandard
    {
        #region JointPos
        public static readonly Vector3 
            spine0Pos = new Vector3(0.0028f, 1.0568f, -0.0776f),
            spine1Pos = new Vector3(0.0051f, 1.2278f, -0.0808f),
            spine3Pos = new Vector3(0.0063f, 1.4761f, -0.0484f),
            neckPos = new Vector3(0.0066f, 1.5132f, -0.0301f),
            headPos = new Vector3(0.0044f, 1.6209f, 0.0236f),
            headTopPos = new Vector3(0.0050f, 1.7504f, 0.0055f),

            shoulderLeftPos = new Vector3(-0.1907f, 1.4407f, -0.0325f),
            elbowLeftPos = new Vector3(-0.1949f, 1.1388f, -0.0620f),
            wristLeftPos = new Vector3(-0.1959f, 0.8694f, -0.0521f),
            handLeftPos = new Vector3(-0.1961f, 0.8055f, -0.0218f),
            indexLeftPos = new Vector3(-0.1945f, 0.7169f, -0.0173f),
            thumbLeftPos = new Vector3(-0.1864f, 0.8190f, 0.0506f),

            shoulderRightPos = new Vector3(0.2029f, 1.4376f, -0.0387f),
            elbowRightPos = new Vector3(0.2014f, 1.1357f, -0.0682f),
            wristRightPos = new Vector3(0.1984f, 0.8663f, -0.0583f),
            handRightPos = new Vector3(0.1983f, 0.8024f, -0.0280f),
            indexRightPos = new Vector3(0.2028f, 0.7139f, -0.0236f),
            thumbRightPos = new Vector3(0.1955f, 0.8159f, 0.0464f),

            hipLeftPos = new Vector3(-0.0950f, 0.9171f, 0.0029f),
            kneeLeftPos = new Vector3(-0.0867f, 0.4913f, 0.0318f),
            ankleLeftPos = new Vector3(-0.0801f, 0.0712f, -0.0766f),
            toeLeftPos = new Vector3(-0.0801f, 0.0039f, 0.0732f),
            footBaseLeftPos = 
                Vector3Helper.MidPoint(toeLeftPos, new Vector3(-0.0692f, 0.0297f, -0.1221f)),

            hipRightPos = new Vector3(0.0961f, 0.9124f, -0.0001f),
            kneeRightPos = new Vector3(0.1040f, 0.4867f, 0.0308f),
            ankleRightPos = new Vector3(0.1101f, 0.0656f, -0.0736f),
            toeRightPos = new Vector3(0.1086f, 0.0000f, 0.0762f),
            footBaseRightPos = 
                Vector3Helper.MidPoint(toeRightPos, new Vector3(0.0974f, 0.0259f, -0.1171f)),

            pelvisPos = Vector3Helper.MidPoint(hipLeftPos, hipRightPos);

        #endregion JointPos
        #region JointRot
        public static readonly Quaternion
            spine0Rot = QuaternionHelper2.LookAtRight(spine0Pos, spine1Pos, -Vector3.UnitX),
            spine3Rot = QuaternionHelper2.LookAtRight(spine1Pos, spine3Pos, -Vector3.UnitX),
            spine1Rot = QuaternionHelper2.LookAtRight(spine3Pos, neckPos, -Vector3.UnitX),
            neckRot = QuaternionHelper2.LookAtRight(neckPos, headPos, -Vector3.UnitX),
            headRot = QuaternionHelper2.LookAtRight(headPos, headTopPos, -Vector3.UnitX),

            clavicleLeftRot = QuaternionHelper2.LookAtUp(spine3Pos, shoulderLeftPos, Vector3.UnitZ),
            shoulderLeftRot = QuaternionHelper2.LookAtRight(shoulderLeftPos, elbowLeftPos, Vector3.UnitX),
            elbowLeftRot = QuaternionHelper2.LookAtRight(elbowLeftPos, wristLeftPos, Vector3.UnitX),
            wristLeftRot = QuaternionHelper2.LookAtRight(wristLeftPos, handLeftPos, Vector3.UnitX),
            trapezoidLeftRot = QuaternionHelper2.LookAtRight(wristLeftPos, thumbLeftPos, Vector3.UnitX),
            handLeftRot = QuaternionHelper2.LookAtRight(handLeftPos, indexLeftPos, Vector3.UnitX),


            clavicleRightRot = QuaternionHelper2.LookAtUp(spine3Pos, shoulderRightPos, Vector3.UnitZ),
            shoulderRightRot = QuaternionHelper2.LookAtRight(shoulderRightPos, elbowRightPos, Vector3.UnitX),
            elbowRightRot = QuaternionHelper2.LookAtRight(elbowRightPos, wristRightPos, Vector3.UnitX),
            wristRightRot = QuaternionHelper2.LookAtRight(wristRightPos, handRightPos, Vector3.UnitX),
            trapezoidRightRot = QuaternionHelper2.LookAtRight(wristRightPos, thumbRightPos, Vector3.UnitX),
            handRightRot = QuaternionHelper2.LookAtRight(handRightPos, indexRightPos, Vector3.UnitX),

            hipLeftRot = QuaternionHelper2.LookAtRight(hipLeftPos, kneeLeftPos, Vector3.UnitX),
            kneeLeftRot = QuaternionHelper2.LookAtRight(kneeLeftPos, ankleLeftPos, Vector3.UnitX),
            ankleLeftRot = QuaternionHelper2.LookAtUp(ankleLeftPos, footBaseLeftPos, Vector3.UnitY),
            footBaseLeftRot = QuaternionHelper2.LookAtUp(footBaseLeftPos, toeLeftPos, Vector3.UnitY),

            hipRightRot = QuaternionHelper2.LookAtRight(hipRightPos, kneeRightPos, Vector3.UnitX),
            kneeRightRot = QuaternionHelper2.LookAtRight(kneeRightPos, ankleRightPos, Vector3.UnitX),
            ankleRightRot = QuaternionHelper2.LookAtUp(ankleRightPos, footBaseRightPos, Vector3.UnitY),
            footBaseRightRot = QuaternionHelper2.LookAtUp(footBaseRightPos, toeRightPos, Vector3.UnitY);
        #endregion
        /// <summary>
        /// Constructur returns a skeleton in standrad ISO/IEC FCD 19774 specification
        /// http://h-anim.org/Specifications/H-Anim200x/ISO_IEC_FCD_19774/
        /// </summary>
        /// <returns></returns>
        public static TreeNode<Bone> GetHAminSkeleton()
        {
            TreeNode<Bone> root = new TreeNode<Bone>(new Bone(Joint.PELVIS,
            HAminStandard.pelvisPos, Quaternion.Identity));
            #region bone structure
            {
                #region upper body 
                #region spine and head
                TreeNode<Bone> spine0 = root.AddChild(new Bone(Joint.SPINE0,
                    HAminStandard.spine0Pos,
                    HAminStandard.spine0Rot));
                {
                    TreeNode<Bone> spine1 = spine0.AddChild(new Bone(Joint.SPINE1,
                        HAminStandard.spine1Pos,
                        HAminStandard.spine1Rot));
                    {
                        TreeNode<Bone> spine3 = spine1.AddChild(new Bone(Joint.SPINE3,
                            HAminStandard.spine3Pos,
                            HAminStandard.spine3Rot));
                    {
                        TreeNode<Bone> neck = spine3.AddChild(new Bone(Joint.NECK,
                            HAminStandard.neckPos,
                            HAminStandard.neckRot));
                        {
                            TreeNode<Bone> head = neck.AddChild(new Bone(Joint.HEAD,
                                HAminStandard.headPos,
                                HAminStandard.headRot));
                            {
                                head.AddChild(new Bone(Joint.HEADTOP, HAminStandard.headTopPos, QuaternionHelper2.Zero));
                            }
                        }
                #endregion
                        #region arm left
                        TreeNode<Bone> clavicleLeft = spine3.AddChild(new Bone(Joint.CLAVICLE_L,
                            HAminStandard.spine3Pos,
                            HAminStandard.clavicleLeftRot));
                        {
                            TreeNode<Bone> shoulderLeft = clavicleLeft.AddChild(new Bone(Joint.SHOULDER_L,
                                HAminStandard.shoulderLeftPos,
                                HAminStandard.shoulderLeftRot));
                            {
                                TreeNode<Bone> elbowLeft = shoulderLeft.AddChild(new Bone(Joint.ELBOW_L,
                                    HAminStandard.elbowLeftPos,
                                    HAminStandard.elbowLeftRot));
                                {
                                    TreeNode<Bone> wristLeft = elbowLeft.AddChild(new Bone(Joint.WRIST_L,
                                        HAminStandard.wristLeftPos,
                                        HAminStandard.wristLeftRot));
                                    {
                                        TreeNode<Bone> handLeft = wristLeft.AddChild(new Bone(Joint.HAND_L,
                                            HAminStandard.handLeftPos,
                                            HAminStandard.handLeftRot));
                                        {
                                            handLeft.AddChild(new Bone(Joint.INDEX_L,
                                            HAminStandard.indexLeftPos,
                                            QuaternionHelper2.Zero));
                                        }
                                        TreeNode<Bone> trapezoidLeft = wristLeft.AddChild(new Bone(Joint.TRAP_L,
                                                HAminStandard.wristLeftPos,
                                                HAminStandard.trapezoidLeftRot));
                                        {
                                            trapezoidLeft.AddChild(new Bone(Joint.THUMB_L,
                                            HAminStandard.thumbLeftPos,
                                            QuaternionHelper2.Zero));
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        #region arm right
                        TreeNode<Bone> clavicleRight = 
                            spine3.AddChild(new Bone(Joint.CLAVICLE_R,
                            HAminStandard.spine3Pos,
                            HAminStandard.clavicleRightRot));
                        {
                            TreeNode<Bone> shoulderRight = 
                                clavicleRight.AddChild(new Bone(Joint.SHOULDER_R,
                                HAminStandard.shoulderRightPos,
                                HAminStandard.shoulderRightRot));
                            {
                                TreeNode<Bone> elbowRight = 
                                    shoulderRight.AddChild(new Bone(Joint.ELBOW_R,
                                    HAminStandard.elbowRightPos,
                                    HAminStandard.elbowRightRot));
                                {
                                    TreeNode<Bone> wristRight = 
                                        elbowRight.AddChild(new Bone(Joint.WRIST_R,
                                        HAminStandard.wristRightPos,
                                        HAminStandard.wristRightRot));    
                                    {
                                        TreeNode<Bone> handRight = 
                                            wristRight.AddChild(new Bone(Joint.HAND_R,
                                            HAminStandard.handRightPos,
                                            HAminStandard.handRightRot));
                                        {
                                            handRight.AddChild(new Bone(Joint.INDEX_R,
                                            HAminStandard.indexRightPos,
                                            QuaternionHelper2.Zero));
                                        TreeNode<Bone> trapezoidRight = 
                                            wristRight.AddChild(new Bone(Joint.TRAP_R,
                                            HAminStandard.wristRightPos,
                                            HAminStandard.trapezoidRightRot));
                                        {
                                            trapezoidRight.AddChild(new Bone(Joint.THUMB_R,
                                            HAminStandard.thumbRightPos,
                                            QuaternionHelper2.Zero));
                                        }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
                }
                #endregion
                #region legs left
                TreeNode<Bone> hipLeft = 
                    root.AddChild(new Bone(Joint.HIP_L,
                        HAminStandard.hipLeftPos,
                        HAminStandard.hipLeftRot));
                {

                    TreeNode<Bone> kneeLeft = 
                        hipLeft.AddChild(new Bone(Joint.KNEE_L,
                            HAminStandard.kneeLeftPos,
                            HAminStandard.kneeLeftRot));
                    {
                        TreeNode<Bone> ankleLeft = 
                            kneeLeft.AddChild(new Bone(Joint.ANKLE_L,
                                HAminStandard.ankleLeftPos,
                                HAminStandard.ankleLeftRot));
                        {
                            TreeNode<Bone> footBaseLeft = 
                                ankleLeft.AddChild(new Bone(Joint.FOOTBASE_L, 
                                    HAminStandard.footBaseLeftPos, 
                                    HAminStandard.footBaseLeftRot));
                            {
                                footBaseLeft.AddChild(
                                    new Bone(Joint.TOE_L, 
                                        HAminStandard.toeLeftPos, 
                                        QuaternionHelper2.Zero)
                                    );
                            }
                        }
                    }
                }
                #endregion
                #region legs right
                TreeNode<Bone> hipRight = 
                    root.AddChild(new Bone(Joint.HIP_R,
                        HAminStandard.hipRightPos,
                        HAminStandard.hipRightRot));
                {
                    TreeNode<Bone> kneeRight = 
                        hipRight.AddChild(new Bone(Joint.KNEE_R,
                            HAminStandard.kneeRightPos,
                            HAminStandard.kneeRightRot));
                    {
                        TreeNode<Bone> ankleRight = 
                            kneeRight.AddChild(new Bone(Joint.ANKLE_R,
                                HAminStandard.ankleRightPos,
                                HAminStandard.ankleRightRot));
                        {
                            TreeNode<Bone> footBaseRight = 
                                ankleRight.AddChild(new Bone(Joint.FOOTBASE_R,
                                    HAminStandard.footBaseRightPos,
                                    HAminStandard.footBaseRightRot));
                            {
                                footBaseRight.AddChild(
                                    new Bone(Joint.TOE_R, 
                                        HAminStandard.toeRightPos, 
                                        QuaternionHelper2.Zero));
                            }
                        }
                    }
                }
                #endregion
            }
                #endregion
            return root;
        }
    }
}
