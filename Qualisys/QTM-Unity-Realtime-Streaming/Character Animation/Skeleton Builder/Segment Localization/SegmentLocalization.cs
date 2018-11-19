﻿#region --- LICENSE ---
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
using System.Collections.Generic;

namespace QualisysRealTime.Unity.Skeleton
{
    /// <summary>
    /// The class transforming the markers to a skeleton using the 
    /// </summary>
    class Values
    {
        public Quaternion hipOrientation;
        public Quaternion chestOrientation;
        public Quaternion headOrientation;
        public Vector3 hipForward;
        public Vector3 chestForward;
        public Vector3 upperArmForwardLeft;
        public Vector3 upperArmForwardRight;
        public Vector3 lowerArmForwardLeft;
        public Vector3 lowerArmForwardRight;
        public Vector3 kneeForwardLeft;
        public Vector3 kneeForwardRight;

        public Vector3 lowerLegUpLeft;
        public Vector3 lowerLegUpRight;
        public Vector3 rightHipPos;
        public Vector3 leftHipPos;
        public Vector3 sternumClavicle;
        public Vector3 spine1;
        public Vector3 head;
        public Vector3 shoulderRight;
        public Vector3 shoulderLeft;
        public Vector3 elbowRight;
        public Vector3 elbowLeft;
        public Vector3 handRight;
        public Vector3 handLeft;
        public Vector3 ankleRight;
        public Vector3 ankleLeft;
        public Vector3 footBaseRight;
        public Vector3 footBaseLeft;
        public Vector3 kneeRight;
        public Vector3 kneeLeft;
    }

    class SegmentLocalization
    {
        private Values o = new Values();

        // Contains all functions for finding segment position
        private List<Action<Segment>> jcFuncs;
        private Dictionary<string, Vector3> markers;
        private MarkersNames m;

        #region Data necessary to estimate segments
        private BodyData bd;
        public BodyData BodyData { get { return bd; } }
        private Quaternion prevChestOri = Quaternion.Identity;
        private Vector3 ZeroVector3 = Vector3.Zero;
        private Quaternion ZeroQuaternion = QuaternionHelper2.Zero;
        private Vector3 UnitX = Vector3.UnitX;
        private Vector3 UnitY = Vector3.UnitY;
        private Vector3 UnitZ = Vector3.UnitZ;
        #endregion

        /// <summary>
        /// Setting up for segment localization
        /// </summary>
        /// <param name="markers">The aliases of the marker names</param>
        public SegmentLocalization(MarkersNames markerNames)
        {
            this.m = markerNames;
            bd = new BodyData(m);

            jcFuncs = new List<Action<Segment>>() {
                    (b) => Pelvis(b),
                    (b) => SpineRoot(b),
                    (b) => MidSpine(b),
                    (b) => SpineEnd(b),
                    (b) => Neck(b),
                    (b) => GetHead(b),
                    (b) => GetHeadTop(b),
                    (b) => GetShoulderLeft(b),
                    (b) => GetUpperArmLeft(b),
                    (b) => GetLowerArmLeft(b),
                    (b) => GetWristLeft(b),
                    (b) => GetHandLeft(b),
                    (b) => GetIndexLeft(b),
                    (b) => GetTrapLeft(b),
                    (b) => GetThumbLeft(b),
                    (b) => GetShoulderRight(b), 
                    (b) => GetUpperArmRight(b),
                    (b) => GetLowerArmRight(b),
                    (b) => GetWristRight(b),
                    (b) => GetHandRight(b),
                    (b) => GetIndexRight(b),
                    (b) => GetTrapRight(b),
                    (b) => GetThumbRight(b),
                    (b) => UpperLegLeft(b),       
                    (b) => LowerLegLeft(b),
                    (b) => GetAnkleLeft(b),
                    (b) => GetFootBaseLeft(b),
                    (b) => GetFootLeft(b),
                    (b) => UpperLegRight(b),
                    (b) => LowerLegRight(b), 
                    (b) => GetAnkleRight(b),    
                    (b) => GetFootBaseRight(b),
                    (b) => GetFootRight(b),
                };
        }

        /// <summary>
        /// Fills in the skeleton with the segment positions given the set of markers
        /// </summary>
        /// <param name="markerData">The dictionary contaiing the markers and their position</param>
        /// <param name="skeleton">The skeleton to be filled in</param>
        public void GetSegmentLocations(Dictionary<string, Vector3> markerData, ref BipedSkeleton skeleton)
        {
            o = new Values(); // reset segment pos and orientations
            markers = markerData;

            // Collect data from markers about body proportions,
            // this is necessary for shoulder segment localization. 
            bd.CalculateBodyData(markers, ChestOrientation);

            // get all segments
            int i = 0;
            SetSegmentsRecursive(skeleton.Root, ref i);
        }

        /// <summary>
        /// Recursive function to set the new segment position and rotation
        /// </summary>
        /// <param name="currentSegment">The current segment</param>
        /// <param name="index">The index of where we are in the tree</param>
        private void SetSegmentsRecursive(TreeNode<Segment> currentSegment, ref int index) 
        {
            jcFuncs[index++](currentSegment.Data);
            if (!currentSegment.IsLeaf) SetSegmentsRecursive(currentSegment.Children, ref index);
        }

        /// <summary>
        /// Helper function
        /// </summary>
        /// <param name="segmentList"></param>
        /// <param name="index"></param>
        private void SetSegmentsRecursive(ICollection<TreeNode<Segment>> segmentList, ref int index)
        {
            foreach (var b in segmentList)
            {
                SetSegmentsRecursive(b, ref index);
            }
        }

        #region Getters and Setters used for segment localization

        /// <summary>
        /// The orientation of the hip
        /// </summary>
        private Quaternion HipOrientation
        {
            get
            {
                if (o.hipOrientation == ZeroQuaternion)
                {
                    Vector3 front = Vector3Helper.MidPoint(markers[m.leftHip], markers[m.rightHip])
                                    - markers[m.bodyBase];
                    Vector3 right = markers[m.leftHip] - markers[m.rightHip];
                    o.hipOrientation = QuaternionHelper2.GetOrientationFromYX(Vector3.Cross(right, front), right);
                }

                return o.hipOrientation;
            }
        }

        /// <summary>
        /// The orientation of the chest / upper body
        /// </summary>
        private Quaternion ChestOrientation
        {
            get
            {
                if (o.chestOrientation == ZeroQuaternion)
                {
                    Vector3 neckPos = markers[m.neck];
                    Vector3 chestPos = markers[m.chest];
                    Vector3 rightShoulderPos = markers[m.rightShoulder];
                    Vector3 leftShoulderPos = markers[m.leftShoulder];
                    Vector3 backspinePos = markers[m.spine];
                    Vector3 Yaxis, Xaxis;
                    Vector3 mid = Vector3Helper.MidPoint(rightShoulderPos, leftShoulderPos);
                    Quaternion rotation;
                    bool slearp = true;// mid.IsNaN() || leftShoulderPos.IsNaN() || rightShoulderPos.IsNaN();

                    // Calculate y-axis
                    if (!mid.IsNaN())
                    {
                        Yaxis = mid - markers[m.bodyBase];
                        slearp = false;
                    }
                    else if (!backspinePos.IsNaN() && !neckPos.IsNaN()) // prio 1, 12th Thoracic to 2nd Thoracic
                    {
                        Yaxis = Vector3Helper.MidPoint(neckPos, backspinePos) - markers[m.bodyBase];
                    }
                    else if (!neckPos.IsNaN()) // prio 2, Sacrum to 2nd Thoracic
                    {
                        Yaxis = neckPos - markers[m.bodyBase];
                    }
                    else if (!backspinePos.IsNaN()) // prio 3, Sacrum to 12th Thoracic
                    {
                        Yaxis = backspinePos - markers[m.bodyBase];
                    }
                    else // last resort, use hip orientation
                    {
                        Yaxis = Vector3.Transform(UnitY, Quaternion.Slerp(prevChestOri, HipOrientation, 0.5f));
                    }

                    // Calculate x-axis
                    if (!rightShoulderPos.IsNaN() || !leftShoulderPos.IsNaN())
                    {
                        if (!rightShoulderPos.IsNaN() && !leftShoulderPos.IsNaN()) // prio 1, use left and right scapula
                        {
                            Xaxis = leftShoulderPos - rightShoulderPos;
                        }
                        else if (!chestPos.IsNaN() && !neckPos.IsNaN())
                        {
                            mid = Vector3Helper.MidPoint(chestPos, neckPos);
                            if (!rightShoulderPos.IsNaN()) // prio 2, use right scapula and mid of Sternum and 2nd Thoracic
                            {
                                Xaxis = mid - rightShoulderPos;
                            }
                            else // prio 3, use left scapula and mid of Sternum and 2nd Thoracic
                            {
                                Xaxis = leftShoulderPos - mid;
                            }
                        }
                        else
                        {
                            Xaxis = -Vector3.Transform(UnitX, Quaternion.Slerp(prevChestOri, HipOrientation, 0.5f));
                        }
                    }
                    else // last resort, use hip prev orientation
                    {
                        Xaxis = -Vector3.Transform(UnitX, Quaternion.Slerp(prevChestOri, HipOrientation, 0.5f));
                    }

                    rotation = slearp ? 
                        Quaternion.Slerp(QuaternionHelper2.GetOrientationFromYX(Yaxis, Xaxis), prevChestOri, 0.8f) : 
                        QuaternionHelper2.GetOrientationFromYX(Yaxis, Xaxis);
                    prevChestOri = rotation;
                    o.chestOrientation = rotation;
                }
                return o.chestOrientation;
            }
        }

        /// <summary>
        /// The orientation of the head
        /// </summary>
        private Quaternion HeadOrientation
        {
            get
            {
                if (o.headOrientation == ZeroQuaternion)
                {
                    o.headOrientation = QuaternionHelper2.GetOrientation(markers[m.head], markers[m.leftHead], markers[m.rightHead]);
                }
                return o.headOrientation;
            }
        }

        /// <summary>
        /// The vector defining forward of the hip
        /// </summary>
        private Vector3 HipForward
        {
            get
            {
                if (o.hipForward == ZeroVector3)
                    o.hipForward = Vector3.Transform(UnitZ, HipOrientation);
                return o.hipForward;
            }
        }

        /// <summary>
        ///  The vector defining forward of the chest / upper body
        /// </summary>
        private Vector3 ChestForward
        {
            get
            {
                if (o.chestForward == ZeroVector3)
                {
                    o.chestForward = Vector3.Transform(UnitZ, ChestOrientation);
                }
                return o.chestForward;
            }
        }

        /// <summary>
        /// The position of the left Upper Arm, commonly known as shoulder
        /// </summary>
        private Vector3 UpperArmForwardLeft
        {
            get
            {
                if (o.upperArmForwardLeft == ZeroVector3)
                {

                    Vector3 midPoint = Vector3Helper.MidPoint(markers[m.leftInnerElbow], markers[m.leftOuterElbow]);
                    o.upperArmForwardLeft = Vector3.NormalizeFast(midPoint - markers[m.leftElbow]);
                }
                return o.upperArmForwardLeft;
            }
        }

        /// <summary>
        /// The position of the left Upper Arm, commonly known as shoulder
        /// </summary>
        private Vector3 UpperArmForwardRight
        {
            get
            {
                if (o.upperArmForwardRight == ZeroVector3)
                {
                    Vector3 midPoint = Vector3Helper.MidPoint(markers[m.rightInnerElbow], markers[m.rightOuterElbow]);
                    o.upperArmForwardRight = Vector3.Normalize(midPoint - markers[m.rightElbow]);
                }
                return o.upperArmForwardRight;
            }
        }

        /// <summary>
        /// The position of the left lower Arm, commonly known as elbow
        /// </summary>
        private Vector3 LowerArmForwardLeft
        {
            get
            {
                if (o.lowerArmForwardLeft == ZeroVector3)
                {
                    o.lowerArmForwardLeft = Vector3.NormalizeFast(markers[m.leftWristRadius] - markers[m.leftWrist]);
                }
                return o.lowerArmForwardLeft;
            }
        }

        /// <summary>
        /// The position of the right lower Arm, commonly known as elbow
        /// </summary>
        private Vector3 LowerArmForwardRight
        {
            get
            {
                if (o.lowerArmForwardRight == ZeroVector3)
                {
                    o.lowerArmForwardRight = Vector3.NormalizeFast(markers[m.rightWristRadius] - markers[m.rightWrist]);
                }
                return o.lowerArmForwardRight;
            }
        }

        /// <summary>
        /// The vector defining the forward of the left knee
        /// </summary>
        private Vector3 KneeForwardLeft
        {
            get
            {
                if (o.kneeForwardLeft == ZeroVector3)
                {
                    o.kneeForwardLeft = KneeLeft - markers[m.leftOuterKnee];
                }
                return o.kneeForwardLeft;
            }
        }

        /// <summary>
        /// The vector defining the forward of the right knee
        /// </summary>
        private Vector3 KneeForwardRight
        {
            get
            {
                if (o.kneeForwardRight == ZeroVector3)
                {
                    o.kneeForwardRight = markers[m.rightOuterKnee] - KneeRight;
                }
                return o.kneeForwardRight;
            }
        }

        /// <summary>
        /// The vector defing whats up with the lower left leg
        /// </summary>
        private Vector3 LowerLegUpLeft
        {
            get
            {
                if (o.lowerLegUpLeft == ZeroVector3)
                {
                    Vector3 belove =
                       !FootBaseLeft.IsNaN() ? FootBaseLeft :
                       !AnkleLeft.IsNaN() ? AnkleLeft :
                       !markers[m.leftHeel].IsNaN() ? markers[m.leftHeel] :
                       !markers[m.leftOuterAnkle].IsNaN() ? markers[m.leftOuterAnkle] :
                       !markers[m.leftInnerAnkle].IsNaN() ? markers[m.leftInnerAnkle] :
                       !markers[m.leftToe2].IsNaN() ? markers[m.leftToe2] :
                       HipSegmentLeftPosition - (Vector3.Transform(UnitY, HipOrientation) * HipSegmentLeftPosition.LengthFast);
                    Vector3 above =
                        !KneeLeft.IsNaN() ? KneeLeft :
                        !markers[m.leftLowerKnee].IsNaN() ? markers[m.leftLowerKnee] :
                        !markers[m.leftUpperKnee].IsNaN() ? markers[m.leftUpperKnee] :
                        !markers[m.leftInnerKnee].IsNaN() ? markers[m.leftInnerKnee] :
                        !markers[m.leftOuterKnee].IsNaN() ? markers[m.leftOuterKnee] :
                        HipSegmentLeftPosition;
                    o.lowerLegUpLeft = above - belove;
                }
                return o.lowerLegUpLeft;
            }
        }

        /// <summary>
        /// The vector defing whats up with the lower right leg.
        /// wazzup leg?
        /// </summary>
        private Vector3 LowerLegUpRight
        {
            get
            {
                if (o.lowerLegUpRight == ZeroVector3)
                {
                    Vector3 belove =
                        !FootBaseRight.IsNaN() ? FootBaseRight :
                        !AnkleRight.IsNaN() ? AnkleRight :
                        !markers[m.rightHeel].IsNaN() ? markers[m.rightHeel] :
                        !markers[m.rightOuterAnkle].IsNaN() ? markers[m.rightOuterAnkle] :
                        !markers[m.rightInnerAnkle].IsNaN() ? markers[m.rightInnerAnkle] :
                        !markers[m.leftToe2].IsNaN() ? markers[m.leftToe2] :
                        HipSegmentRightPosition - (Vector3.Transform(UnitY,HipOrientation) * HipSegmentRightPosition.LengthFast );
                    Vector3 above =
                        !KneeRight.IsNaN() ? KneeRight :
                        !markers[m.rightLowerKnee].IsNaN() ? markers[m.rightLowerKnee] :
                        !markers[m.rightUpperKnee].IsNaN() ? markers[m.rightUpperKnee] :
                        !markers[m.rightInnerKnee].IsNaN() ? markers[m.rightInnerKnee] :
                        !markers[m.rightOuterKnee].IsNaN() ? markers[m.rightOuterKnee] :
                        HipSegmentRightPosition;
                    o.lowerLegUpRight = above - belove;
                }
                return o.lowerLegUpRight;
            }
        }

        #endregion

        #region Special segments position

        /// <summary>
        /// 
        /// </summary>
        private Vector3 HipSegmentRightPosition
        {
            get {
                if (o.rightHipPos == ZeroVector3)
                {
                    o.rightHipPos = GetHipSegmentPosition(true);
                }
                return o.rightHipPos;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Vector3 HipSegmentLeftPosition
        {
            get
            {
                if (o.leftHipPos == ZeroVector3)
                {
                    o.leftHipPos = GetHipSegmentPosition(false);
                }
                return o.leftHipPos;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Vector3 SternumClavicle
        {
            get
            {
                if (o.sternumClavicle == ZeroVector3)
                {
                    Vector3 back = markers[m.neck];
                    Vector3 front = markers[m.chest];
                    Vector3 neckPos;
                    Vector3 neck2ChestVector = bd.NeckToChestVector;
                    Vector3 transformedNeckToChestVector = Vector3.Transform(neck2ChestVector, ChestOrientation) / 2;
                    if (!front.IsNaN() && neck2ChestVector != ZeroVector3)
                    {
                        neckPos = front - transformedNeckToChestVector;
                    }
                    else if (!back.IsNaN() && neck2ChestVector != ZeroVector3)
                    {
                        neckPos = back + transformedNeckToChestVector;
                    }
                    else if (!back.IsNaN())
                    {
                        neckPos = back + ChestForward * BodyData.MarkerToSpineDist;
                    }
                    else
                    {
                        neckPos = Vector3Helper.MidPoint(markers[m.leftShoulder], markers[m.rightShoulder]);
                    }
                    o.sternumClavicle = neckPos;
                }
                return o.sternumClavicle;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Vector3 Spine1
        {
            get {
                if (o.spine1 == ZeroVector3)
                {
                    //Vector3 pos;
                    //Vector3 target;
                    //if (markers[m.neck].IsNaN())
                    //{
                    //    pos = markers[m.bodyBase];
                    //    target = markers[m.spine];
                    //}
                    //else
                    //{
                    //    pos = markers[m.spine];
                    //    target = markers[m.neck];
                    //}
                    //Vector3 front = Vector3.Transform(UnitZ, QuaternionHelper2.LookAtUp(pos, target, ChestForward));
                    //front.Normalize();
                    //pos = markers[m.spine];
                    //front = HipForward;
                    //pos += front * BodyData.MarkerToSpineDist;
                    //o.spine1 = pos;markers[m.spine]
                    o.spine1 = markers[m.spine] + HipForward * BodyData.MarkerToSpineDist;
                }
                return o.spine1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Vector3 Head
        {
            get
            {
                if (o.head == ZeroVector3)
                {
                    Vector3 headPos = Vector3Helper.MidPoint(markers[m.leftHead], markers[m.rightHead]);
                    //Move head position down
                    //Vector3 down = (SternumClavicle - headPos);//-Vector3.Transform(UnitY, HeadOrientation);
                    //down.NormalizeFast();
                    //down *= BodyData.MidHeadToHeadJoint;
                    //headPos += down;
                    o.head = headPos;
                }
                return o.head;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Vector3 ShoulderLeft
        {
            get
            {
                if (o.shoulderLeft == ZeroVector3)
                {
                    o.shoulderLeft = GetUpperarmSegmentPosition(false);
                }
                return o.shoulderLeft;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Vector3 ShoulderRight
        {
            get
            {
                if (o.shoulderRight == ZeroVector3)
                {
                    o.shoulderRight = GetUpperarmSegmentPosition(true);
                }
                return o.shoulderRight;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Vector3 ElbowLeft
        {
            get
            {
                if (o.elbowLeft == ZeroVector3)
                {
                    o.elbowLeft = Vector3Helper.MidPoint(markers[m.leftInnerElbow], markers[m.leftOuterElbow]);
                }
                return o.elbowLeft;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Vector3 ElbowRight
        {
            get
            {
                if (o.elbowRight == ZeroVector3)
                {
                    o.elbowRight = Vector3Helper.MidPoint(markers[m.rightInnerElbow], markers[m.rightOuterElbow]);
                }
                return o.elbowRight;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Vector3 WristLeft
        {
            get
            {
                if (o.handLeft == ZeroVector3)
                {
                    o.handLeft = Vector3Helper.MidPoint(markers[m.leftWrist], markers[m.leftWristRadius]);
                }
                return o.handLeft;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Vector3 WristRight
        {
            get
            {
                if (o.handRight == ZeroVector3)
                {
                    o.handRight = Vector3Helper.MidPoint(markers[m.rightWrist], markers[m.rightWristRadius]);
                }
                return o.handRight;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Vector3 KneeRight
        {
            get
            {
                if (o.kneeRight == ZeroVector3)
                {
                    if (markers[m.rightInnerKnee].IsNaN())
                    {
                        o.kneeRight = GetKneePosition(true);
                    }
                    else
                    {
                        o.kneeRight = Vector3Helper.MidPoint(markers[m.rightOuterKnee], markers[m.rightInnerKnee]);
                    }
                }
                return o.kneeRight;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Vector3 KneeLeft
        {
            get
            {
                if (o.kneeLeft  == ZeroVector3)
                {
                    if (markers[m.leftInnerKnee].IsNaN())
                    {
                        o.kneeLeft = GetKneePosition(false);
                    }
                    else
                    {
                        o.kneeLeft = Vector3Helper.MidPoint(markers[m.leftOuterKnee], markers[m.leftInnerKnee]);
                    }
                }
                return o.kneeLeft;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Vector3 AnkleLeft
        {
            get
            {
                    if (o.ankleLeft == ZeroVector3)
                    {
                        if (markers[m.leftInnerAnkle].IsNaN())
                        {
                            o.ankleLeft = GetAnklePosition(false);
                        }
                        else
                        {
                            o.ankleLeft = Vector3Helper.MidPoint(markers[m.leftOuterAnkle], markers[m.leftInnerAnkle]);
                        }
                    }
                return o.ankleLeft;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Vector3 AnkleRight
        {
            get
            {
                if (o.ankleRight == ZeroVector3)
                {
                    if (markers[m.rightInnerAnkle].IsNaN())
                    {
                        o.ankleRight = GetAnklePosition(true);
                    }
                    else
                    {
                        o.ankleRight = Vector3Helper.MidPoint(markers[m.rightOuterAnkle], markers[m.rightInnerAnkle]);
                    }
                }
                return o.ankleRight;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Vector3 FootBaseLeft
        {
            get
            {
                if (o.footBaseLeft == ZeroVector3)
                {
                    o.footBaseLeft = Vector3Helper.PointBetween(markers[m.leftHeel], markers[m.leftToe2], 0.4f);
                }
                return o.footBaseLeft;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Vector3 FootBaseRight
        {
            get
            {
                if (o.footBaseRight == ZeroVector3)
                {
                    o.footBaseRight = Vector3Helper.PointBetween(markers[m.rightHeel], markers[m.rightToe2], 0.4f);
                }
                return o.footBaseRight;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isRightHip"></param>
        /// <returns></returns>
        private Vector3 GetHipSegmentPosition(bool isRightHip)
        {
            // As described by Harrington et al. 2006
            // Prediction of the hip segment centre in adults, children, and patients with
            // cerebral palsy based on magnetic resonance imaging
            Vector3 ASISMid = Vector3Helper.MidPoint(markers[m.rightHip], markers[m.leftHip]);
            float Z, X, Y,
                pelvisDepth = (ASISMid - markers[m.bodyBase]).Length * 1000,
                pelvisWidth = (markers[m.leftHip] - markers[m.rightHip]).Length * 1000;
            X =  0.33f * pelvisWidth -  7.3f;
            Y = -0.30f * pelvisWidth - 10.9f;
            Z = -0.24f * pelvisDepth -  9.9f;
            if (!isRightHip) X = -X;
            return ASISMid + Vector3.Transform((new Vector3(X, Y, Z) / 1000), HipOrientation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isRightShoulder"></param>
        /// <returns></returns>
        private Vector3 GetUpperarmSegmentPosition(bool isRightShoulder)
        {
            // as described by Campbell et al. 2009 in 
            // MRI development and validation of two new predictive methods of
            // glenohumeral segment centre location identification and comparison with
            // established techniques
            float
                x = 96.2f - 0.302f * bd.ChestDepth - 0.364f * bd.Height + 0.385f * bd.Mass,
                y = -66.32f + 0.30f * bd.ChestDepth - 0.432f * bd.Mass,
                z = 66.468f - 0.531f * bd.ShoulderWidth + 0.571f * bd.Mass;
            if (isRightShoulder) x = -x;
            Vector3 res = new Vector3(x, y, z) / 1000; // to mm
            res = Vector3.Transform(res, ChestOrientation); //QuaternionHelper.Rotate(ChestOrientation, res);
            res += isRightShoulder ? markers[m.rightShoulder] : markers[m.leftShoulder];
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isRightKnee"></param>
        /// <returns></returns>
        private Vector3 GetKneePosition(bool isRightKnee)
        {
            Vector3 x, z, M1, M2, M3, negateY = new Vector3(1f, -1f, 1f);
                if (isRightKnee)
                {
                    M1 =  markers[m.rightOuterKnee];//FLE
                    M2 = markers[m.rightOuterAnkle];//FAL
                    M3 = markers[m.rightLowerKnee];//TTC
                }
                else
                {
                    M1 = markers[m.leftOuterKnee];//FLE
                    M2 = markers[m.leftOuterAnkle];//FAL
                    M3 = markers[m.leftLowerKnee];//TTC
            }
            x = Vector3Helper.MidPoint(M1, M2) - M3;
            z = M1 - M2;
            float scalingFactor = z.Length;
            Matrix4 R = Matrix4Helper.GetOrientationMatrix(x, z);
            Vector3 trans = new Vector3(
                -0.1033f*scalingFactor,
                -0.09814f*scalingFactor,
                0.0597f*scalingFactor);
            if (isRightKnee) Vector3.Multiply(ref trans, ref negateY, out trans);
            return Vector3.TransformVector(trans, R) + M1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isRightAnkle"></param>
        /// <returns></returns>
        private Vector3 GetAnklePosition(bool isRightAnkle)
        {
            //Stolen from Visual3d
            Vector3 x, z, M1, M2, M3, negateY = new Vector3(1f, -1f, 1f);
            Matrix4 R;
            if (isRightAnkle)
            {
                M1 = markers[m.rightOuterKnee];//FLE
                M3 = markers[m.rightLowerKnee];//TTC
                M2 = markers[m.rightOuterAnkle];//FAL
            }
            else
            {
                M1 = markers[m.leftOuterKnee];//FLE
                M3 = markers[m.leftLowerKnee];//TTC
                M2 = markers[m.leftOuterAnkle];//FAL
            }
            x = Vector3Helper.MidPoint(M1, M2) - M3;
            z = M2 - M1;
            float scalefactor = z.Length;
            R = Matrix4Helper.GetOrientationMatrix(x, z);
            Vector3 trans = new Vector3(
                -0.07675f * scalefactor, 
                0.05482f * scalefactor, 
                -0.02741f * scalefactor);
            if (!isRightAnkle) Vector3.Multiply(ref trans, ref negateY, out trans);
            return Vector3.TransformVector(trans, R) + M2;
        }

        #endregion

        #region Getters - Pelvis to Head
        private void Pelvis(Segment b)
        {
            b.Pos = Vector3Helper.MidPoint(HipSegmentRightPosition, HipSegmentLeftPosition); 
            b.Orientation = HipOrientation;
        }

        private void SpineRoot(Segment b)
        {
            Vector3 target = !Spine1.IsNaN() ? Spine1 : SternumClavicle;
            Vector3 pos = markers[m.bodyBase] + HipForward * BodyData.MarkerToSpineDist;
            b.Pos = pos;
            b.Orientation = QuaternionHelper2.LookAtUp(pos, target, HipForward);
        }

        private void MidSpine(Segment b)
        {
            b.Pos = Spine1;
            b.Orientation = QuaternionHelper2.LookAtRight(Spine1, SternumClavicle,  HipSegmentLeftPosition - HipSegmentRightPosition);
        }

        private void SpineEnd(Segment b)
        {
            b.Pos = SternumClavicle;
            b.Orientation = ChestOrientation;
        }

        private void Neck(Segment b)
        {
            Vector3 up = Vector3.Transform(UnitY, ChestOrientation);
            Vector3 pos = SternumClavicle + up * BodyData.SpineLength * 2;
            b.Pos = pos;
            b.Orientation = QuaternionHelper2.LookAtUp(pos, Head, ChestForward);
        }

        private void GetHead(Segment b)
        {
            b.Pos = Head;
            b.Orientation = HeadOrientation;
        }

        private void GetHeadTop(Segment b)
        {
            b.Pos = Head + Vector3.Transform(UnitY, HeadOrientation) * (BodyData.MidHeadToHeadOriginDist * 2);
        }        
        #endregion

        #region Getters - Legs
        private void UpperLegLeft(Segment b)
        {
            b.Pos = HipSegmentLeftPosition;
            b.Orientation = QuaternionHelper2.LookAtRight(HipSegmentLeftPosition, KneeLeft, HipSegmentRightPosition-HipSegmentLeftPosition );
        }

        private void UpperLegRight(Segment b)
        {
            b.Pos = HipSegmentRightPosition;
            b.Orientation = QuaternionHelper2.LookAtRight(HipSegmentRightPosition, KneeRight, HipSegmentRightPosition - HipSegmentLeftPosition);
        }

        private void LowerLegLeft(Segment b)
        {
            b.Pos = KneeLeft;
            b.Orientation = QuaternionHelper2.LookAtRight(KneeLeft, AnkleLeft, KneeForwardLeft); 
        }

        private void LowerLegRight(Segment b)
        {
            b.Pos = KneeRight;
            b.Orientation = QuaternionHelper2.LookAtRight(KneeRight, AnkleRight, KneeForwardRight);
        }

        private void GetAnkleLeft(Segment b)
        {
            b.Pos = AnkleLeft;
            if (markers[m.leftInnerAnkle].IsNaN())
            {
                Vector3 up = KneeLeft - AnkleLeft;
                b.Orientation = QuaternionHelper2.LookAtUp(AnkleLeft, FootBaseLeft, up);
            }
            else
            {
                Vector3 right =  markers[m.leftInnerAnkle] - markers[m.leftOuterAnkle];
                b.Orientation = QuaternionHelper2.LookAtRight(AnkleLeft, FootBaseLeft, right);
            }
        }

        private void GetAnkleRight(Segment b)
        {
            b.Pos = AnkleRight;
            if (markers[m.rightInnerAnkle].IsNaN())
            {
                Vector3 up = KneeRight - AnkleRight;
                b.Orientation = QuaternionHelper2.LookAtUp(AnkleRight, FootBaseRight, up);
            }
            else
            {
                Vector3 right = markers[m.rightOuterAnkle] - markers[m.rightInnerAnkle];
                b.Orientation = QuaternionHelper2.LookAtRight(AnkleRight, FootBaseRight, right);
            }
        }

        private void GetFootBaseLeft(Segment b)
        {
            b.Pos = FootBaseLeft;
            b.Orientation = QuaternionHelper2.LookAtUp(b.Pos, markers[m.leftToe2], LowerLegUpLeft);
        }

        private void GetFootBaseRight(Segment b)
        {
            b.Pos = FootBaseRight;
            b.Orientation = QuaternionHelper2.LookAtUp(b.Pos, markers[m.rightToe2], LowerLegUpRight);
        }

        private void GetFootLeft(Segment b)
        {
            b.Pos = markers[m.leftToe2];
        }

        private void GetFootRight(Segment b)
        {
            b.Pos = markers[m.rightToe2];
        }        
        #endregion

        #region Getters - Arms
        private void GetShoulderLeft(Segment b)
        {
            b.Pos = SternumClavicle;
            b.Orientation = QuaternionHelper2.LookAtUp(SternumClavicle, ShoulderLeft, ChestForward);
        }

        private void GetShoulderRight(Segment b)
        {
            b.Pos = SternumClavicle;
            b.Orientation = QuaternionHelper2.LookAtUp(SternumClavicle, ShoulderRight, ChestForward);
        }

        private void GetUpperArmLeft(Segment b)
        {
            b.Pos = ShoulderLeft;
            b.Orientation = QuaternionHelper2.LookAtRight(ShoulderLeft, ElbowLeft, markers[m.leftInnerElbow] - markers[m.leftOuterElbow]);
        }

        private void GetUpperArmRight(Segment b)
        {
            b.Pos = ShoulderRight;
            b.Orientation = QuaternionHelper2.LookAtRight(ShoulderRight, ElbowRight, markers[m.rightOuterElbow] - markers[m.rightInnerElbow]);
        }

        private void GetLowerArmLeft(Segment b)
        {
            b.Pos = ElbowLeft;
            b.Orientation = QuaternionHelper2.LookAtUp(ElbowLeft, WristLeft, LowerArmForwardLeft);
        }

        private void GetLowerArmRight(Segment b)
        {
            b.Pos = ElbowRight;
            b.Orientation = QuaternionHelper2.LookAtUp(ElbowRight, WristRight, LowerArmForwardRight);
        }

        private void GetWristLeft(Segment b)
        {
            b.Pos = WristLeft;
            b.Orientation = QuaternionHelper2.LookAtUp(WristLeft, markers[m.leftHand], LowerArmForwardLeft);
        }

        private void GetWristRight(Segment b)
        {
            b.Pos = WristRight;
            b.Orientation = QuaternionHelper2.LookAtUp(WristRight, markers[m.rightHand], LowerArmForwardRight);
        }

        #region Getters - Hands
        private void GetTrapLeft(Segment b)
        {
            b.Pos = WristLeft;
            b.Orientation = QuaternionHelper2.LookAtUp(WristLeft, markers[m.leftThumb], LowerArmForwardLeft);
        }
        private void GetTrapRight(Segment b)
        {
            b.Pos = WristRight;
            b.Orientation = QuaternionHelper2.LookAtUp(WristRight, markers[m.rightThumb], LowerArmForwardRight);
        }
        private void GetHandLeft(Segment b)
        {
            b.Pos = markers[m.leftHand];
            b.Orientation = QuaternionHelper2.LookAtUp(markers[m.leftHand], markers[m.leftIndex], LowerArmForwardLeft);
        }
        private void GetHandRight(Segment b)
        {
            b.Pos = markers[m.rightHand];
            b.Orientation = QuaternionHelper2.LookAtUp(markers[m.rightHand], markers[m.rightIndex], LowerArmForwardRight);
        }
        private void GetThumbLeft(Segment b)
        {
            b.Pos = markers[m.leftThumb];
        }
        private void GetThumbRight(Segment b)
        {
            b.Pos = markers[m.rightThumb];
        }
        private void GetIndexLeft(Segment b)
        {
            b.Pos = markers[m.leftIndex];
        }
        private void GetIndexRight(Segment b)
        {
            b.Pos = markers[m.rightIndex];
        }
        #endregion

        #endregion
    }
}