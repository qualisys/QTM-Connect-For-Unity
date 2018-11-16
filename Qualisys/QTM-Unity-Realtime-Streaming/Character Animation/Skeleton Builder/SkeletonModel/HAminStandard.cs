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
        public static TreeNode<Segment> GetHAminSkeleton()
        {
            TreeNode<Segment> root = new TreeNode<Segment>(new Segment(SegmentName.PELVIS,
            HAminStandard.pelvisPos, Quaternion.Identity));
            #region segment structure
            {
                #region upper body 
                #region spine and head
                TreeNode<Segment> spine0 = root.AddChild(new Segment(SegmentName.SPINE0,
                    HAminStandard.spine0Pos,
                    HAminStandard.spine0Rot));
                {
                    TreeNode<Segment> spine1 = spine0.AddChild(new Segment(SegmentName.SPINE1,
                        HAminStandard.spine1Pos,
                        HAminStandard.spine1Rot));
                    {
                        TreeNode<Segment> spine3 = spine1.AddChild(new Segment(SegmentName.SPINE3,
                            HAminStandard.spine3Pos,
                            HAminStandard.spine3Rot));
                    {
                        TreeNode<Segment> neck = spine3.AddChild(new Segment(SegmentName.NECK,
                            HAminStandard.neckPos,
                            HAminStandard.neckRot));
                        {
                            TreeNode<Segment> head = neck.AddChild(new Segment(SegmentName.HEAD,
                                HAminStandard.headPos,
                                HAminStandard.headRot));
                            {
                                head.AddChild(new Segment(SegmentName.HEADTOP, HAminStandard.headTopPos, QuaternionHelper2.Zero));
                            }
                        }
                #endregion
                        #region arm left
                        TreeNode<Segment> clavicleLeft = spine3.AddChild(new Segment(SegmentName.CLAVICLE_L,
                            HAminStandard.spine3Pos,
                            HAminStandard.clavicleLeftRot));
                        {
                            TreeNode<Segment> shoulderLeft = clavicleLeft.AddChild(new Segment(SegmentName.SHOULDER_L,
                                HAminStandard.shoulderLeftPos,
                                HAminStandard.shoulderLeftRot));
                            {
                                TreeNode<Segment> elbowLeft = shoulderLeft.AddChild(new Segment(SegmentName.ELBOW_L,
                                    HAminStandard.elbowLeftPos,
                                    HAminStandard.elbowLeftRot));
                                {
                                    TreeNode<Segment> wristLeft = elbowLeft.AddChild(new Segment(SegmentName.WRIST_L,
                                        HAminStandard.wristLeftPos,
                                        HAminStandard.wristLeftRot));
                                    {
                                        TreeNode<Segment> handLeft = wristLeft.AddChild(new Segment(SegmentName.HAND_L,
                                            HAminStandard.handLeftPos,
                                            HAminStandard.handLeftRot));
                                        {
                                            handLeft.AddChild(new Segment(SegmentName.INDEX_L,
                                            HAminStandard.indexLeftPos,
                                            QuaternionHelper2.Zero));
                                        }
                                        TreeNode<Segment> trapezoidLeft = wristLeft.AddChild(new Segment(SegmentName.TRAP_L,
                                                HAminStandard.wristLeftPos,
                                                HAminStandard.trapezoidLeftRot));
                                        {
                                            trapezoidLeft.AddChild(new Segment(SegmentName.THUMB_L,
                                            HAminStandard.thumbLeftPos,
                                            QuaternionHelper2.Zero));
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        #region arm right
                        TreeNode<Segment> clavicleRight = 
                            spine3.AddChild(new Segment(SegmentName.CLAVICLE_R,
                            HAminStandard.spine3Pos,
                            HAminStandard.clavicleRightRot));
                        {
                            TreeNode<Segment> shoulderRight = 
                                clavicleRight.AddChild(new Segment(SegmentName.SHOULDER_R,
                                HAminStandard.shoulderRightPos,
                                HAminStandard.shoulderRightRot));
                            {
                                TreeNode<Segment> elbowRight = 
                                    shoulderRight.AddChild(new Segment(SegmentName.ELBOW_R,
                                    HAminStandard.elbowRightPos,
                                    HAminStandard.elbowRightRot));
                                {
                                    TreeNode<Segment> wristRight = 
                                        elbowRight.AddChild(new Segment(SegmentName.WRIST_R,
                                        HAminStandard.wristRightPos,
                                        HAminStandard.wristRightRot));    
                                    {
                                        TreeNode<Segment> handRight = 
                                            wristRight.AddChild(new Segment(SegmentName.HAND_R,
                                            HAminStandard.handRightPos,
                                            HAminStandard.handRightRot));
                                        {
                                            handRight.AddChild(new Segment(SegmentName.INDEX_R,
                                            HAminStandard.indexRightPos,
                                            QuaternionHelper2.Zero));
                                        TreeNode<Segment> trapezoidRight = 
                                            wristRight.AddChild(new Segment(SegmentName.TRAP_R,
                                            HAminStandard.wristRightPos,
                                            HAminStandard.trapezoidRightRot));
                                        {
                                            trapezoidRight.AddChild(new Segment(SegmentName.THUMB_R,
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
                TreeNode<Segment> hipLeft = 
                    root.AddChild(new Segment(SegmentName.HIP_L,
                        HAminStandard.hipLeftPos,
                        HAminStandard.hipLeftRot));
                {

                    TreeNode<Segment> kneeLeft = 
                        hipLeft.AddChild(new Segment(SegmentName.KNEE_L,
                            HAminStandard.kneeLeftPos,
                            HAminStandard.kneeLeftRot));
                    {
                        TreeNode<Segment> ankleLeft = 
                            kneeLeft.AddChild(new Segment(SegmentName.ANKLE_L,
                                HAminStandard.ankleLeftPos,
                                HAminStandard.ankleLeftRot));
                        {
                            TreeNode<Segment> footBaseLeft = 
                                ankleLeft.AddChild(new Segment(SegmentName.FOOTBASE_L, 
                                    HAminStandard.footBaseLeftPos, 
                                    HAminStandard.footBaseLeftRot));
                            {
                                footBaseLeft.AddChild(
                                    new Segment(SegmentName.TOE_L, 
                                        HAminStandard.toeLeftPos, 
                                        QuaternionHelper2.Zero)
                                    );
                            }
                        }
                    }
                }
                #endregion
                #region legs right
                TreeNode<Segment> hipRight = 
                    root.AddChild(new Segment(SegmentName.HIP_R,
                        HAminStandard.hipRightPos,
                        HAminStandard.hipRightRot));
                {
                    TreeNode<Segment> kneeRight = 
                        hipRight.AddChild(new Segment(SegmentName.KNEE_R,
                            HAminStandard.kneeRightPos,
                            HAminStandard.kneeRightRot));
                    {
                        TreeNode<Segment> ankleRight = 
                            kneeRight.AddChild(new Segment(SegmentName.ANKLE_R,
                                HAminStandard.ankleRightPos,
                                HAminStandard.ankleRightRot));
                        {
                            TreeNode<Segment> footBaseRight = 
                                ankleRight.AddChild(new Segment(SegmentName.FOOTBASE_R,
                                    HAminStandard.footBaseRightPos,
                                    HAminStandard.footBaseRightRot));
                            {
                                footBaseRight.AddChild(
                                    new Segment(SegmentName.TOE_R, 
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
