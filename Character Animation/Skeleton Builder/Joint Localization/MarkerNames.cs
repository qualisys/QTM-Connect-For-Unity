#region --- LICENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015 Qualisys AB

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System.Collections;
using System.Collections.Generic;
namespace QualisysRealTime.Unity.Skeleton
{
    class MarkersNames : IEnumerable<string>
    {
        public string
            bodyBase = "SACR",
            leftHip = "L_IAS",
            rightHip = "R_IAS",
            spine= "TV12",
            neck =  "TV2",
            chest =  "SME",
            leftShoulder = "L_SAE",
            rightShoulder = "R_SAE",
            head =  "SGL",
            leftHead = "L_HEAD",
            rightHead = "R_HEAD",
            leftElbow  = "L_UOA",
            leftInnerElbow  = "L_HME",
            leftOuterElbow  = "L_HLE",
            rightElbow  = "R_UOA",
            rightInnerElbow  = "R_HME",
            rightOuterElbow  = "R_HLE",
            leftWrist  = "L_USP",
            leftWristRadius  = "L_RSP",
            leftHand  = "L_HM2",
            leftIndex = "L_Index",
            leftThumb = "L_Thumb",
            rightWrist  = "R_USP",
            rightWristRadius = "R_RSP",
            rightHand  = "R_HM2",
            rightIndex = "R_Index",
            rightThumb = "R_Thumb",
            leftUpperKnee  = "L_PAS",
            leftOuterKnee  = "L_FLE",
            leftInnerKnee  = "L_FME",
            leftLowerKnee  = "L_TTC",
            rightUpperKnee = "R_PAS",
            rightOuterKnee  = "R_FLE",
            rightInnerKnee  = "R_FME",
            rightLowerKnee  = "R_TTC",
            leftOuterAnkle  = "L_FAL",
            leftInnerAnkle  = "L_TAM",
            leftHeel  = "L_FCC",
            rightOuterAnkle  = "R_FAL",
            rightInnerAnkle  = "R_TAM",
            rightHeel  = "R_FCC",
            leftToe1  = "L_FM1",
            leftToe2  = "L_FM2",
            leftToe5 = "L_FM5",
            rightToe1  = "R_FM1",
            rightToe2  = "R_FM2",
            rightToe5  = "R_FM5";


        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<string> GetEnumerator()
        {
            yield return bodyBase;
            yield return leftHip;
            yield return rightHip;
            yield return spine;
            yield return neck;
            yield return chest;
            yield return leftShoulder;
            yield return rightShoulder;
            yield return head;
            yield return leftHead;
            yield return rightHead;
            yield return leftElbow;
            yield return leftInnerElbow ;
            yield return leftOuterElbow;
            yield return rightElbow;
            yield return rightInnerElbow;
            yield return rightOuterElbow;
            yield return leftWrist;
            yield return leftWristRadius;
            yield return leftHand ;
            yield return leftIndex;
            yield return leftThumb;
            yield return rightWrist;
            yield return rightWristRadius;
            yield return rightHand;
            yield return rightIndex;
            yield return rightThumb;
            yield return leftUpperKnee;
            yield return leftOuterKnee;
            yield return leftInnerKnee;
            yield return leftLowerKnee;
            yield return rightUpperKnee;
            yield return rightOuterKnee;
            yield return rightInnerKnee;
            yield return rightLowerKnee;
            yield return leftOuterAnkle;
            yield return leftInnerAnkle;
            yield return leftHeel;
            yield return rightOuterAnkle;
            yield return rightInnerAnkle;
            yield return rightHeel;
            yield return leftToe1;
            yield return leftToe2;
            yield return leftToe5;
            yield return rightToe1;
            yield return rightToe2;
            yield return rightToe5;
        }
    }
    [System.Serializable]
    static class MarkerNames
    {
    public static readonly List<string[]>
        bodyBasebetween = new List<string[]>()
            {
                new string[] { "R_IPS", "L_IPS" },
                new string[] { "RPSIS", "LPSIS" },
                new string[] { "Rt Lower PSIS", "Lt Lower PSIS" },
                new string[] { "R_PSIS", "L_PSIS" },
                new string[] { "L_Sacrum", "R_Sacrum" },
                new string[] { "RBWT", "LBWT"} 
            },
        neckBetween = new List<string[]>() { 
            new string[] { "Lt Up Back", "Rt Up Back" } },
        headBetween = new List<string[]>() 
            {   
                new string[] {"head_left_front", "head_right_front"},
                new string[] {"RFHD", "LFHD"}
            },
        rightToe2Between = new List<string[]>() 
            { 
                new string[] { "FM1", "FM5" }, 
                new string[] { "TOE_1_MET", "TOE_5_MET" }, 
                new string[] { "MT_1", "MT_5" }
            };

        #region hip
        public static readonly List<string>
            bodyBaseAKA = new List<string>() { "SACR", "SACRUM", "LOWER_LUMBAR", "LV5_S1", "S1" },
            leftHipAKA = new List<string>() { "L_IAS", "L_ASIS","LASIS","LEFT_HIP","L_ICT","Lt Hip Front", "LFWT", "LASI"},
            rightHipAKA = new List<string>() { "R_IAS", "R_ASIS", "RASIS", "RIGHT_HIP", "R_ICT", "Rt Hip Front", "RFWT", "RASI" },
        #endregion
        #region upperbody
            spineAKA = new List<string>() 
            {   "TV12", "TH12", "D12", "T12",
                "TV11", "TH11", "D11", "T11", 
                "TV13", "TH13", "D13", "T13", 
                "TV10", "TH10", "D10", "T10", 
                "TV14", "TV14", "D14", "T14",
                "L1",   "LV3"       },
            neckAKA = new List<string>() { "TV2", "TV1", "C7", "C7_TOP_SPINE", "NECK", "Thor1"},
            chestAKA = new List<string>() { "SME", "SJN", "CLAV" },
            leftShoulderAKA = new List<string>() {"L_SAE", "L_ACR", "LEFT_SHOULDER", "L_SHOULDER", "L_ACROMION", "LEFTSHOULDER", "LEFT SHOULDER", "LSHO", "LSHOULDER", "LSHOULD" },
            rightShoulderAKA = new List<string>() { "R_SAE", "R_ACR", "RIGHT_SHOULDER", "R_SHOULDER", "R_ACROMION", "RIGHTSHOULDER", "RIGHT SHOULDER", "RSHO", "RSHOULDER", "RSHOULD" } ,
        #endregion
                #region head
                headAKA = new List<string>() {"SGL", "Front of Head", "F_HEAD", "FOREHEAD" },
            leftHeadAKA = new List<string>() { "L_HEAD", "HEAD_LEFT_FRONT", "Left Head", "L_HEAD", "LBHD", "HEAD2", "LTEMP" },
            rightHeadAKA = new List<string>() { "R_HEAD", "HEAD_RIGHT_FRONT", "Right Head", "R_HEAD", "RBHD", "HEAD1", "RTEMP" },
        #endregion
        #region elbow
        #region left elbow
            leftElbowAKA = new List<string>() {"L_UOA", "L_ELB", "LEFT_ELBOW" },
            leftInnerElbowAKA = new List<string>() { "L_HME", "L_ELBOW_MED", "LMELBW" },
            leftOuterElbowAKA = new List<string>() { "L_HLE", "L_ELBOW", "Lt Elbow", "LELB", "LLELBW" },
        #endregion
                #region right elbow
                rightElbowAKA = new List<string>() {"R_UOA", "R_ELB", "RIGHT_ELBOW" },
            rightInnerElbowAKA = new List<string>() { "R_HME", "R_ELBOW_MED", "RMELBW" },
            rightOuterElbowAKA = new List<string>() { "R_HLE", "R_ELBOW", "Rt Elbow", "RELB", "RLELBW" },
        #endregion
        #endregion
        #region hand
        #region lefthand
            leftWristAKA = new List<string>() { "L_USP", "L_ULNA", "LEFT_WRIST_INNER", "L_WRIST_MED", "Lt Wrist Ulna", "LT Ulna", "LWRB", "L_HAND_M", "LMWRIST" },
            leftWristRadiusAKA = new List<string>() { "L_RSP", "L_RADUIS", "LEFT_WRIST_OUTER", "L_WRIST_LAT", "Lt Wrist Radius", "Lt Radius", "LWRA", "L_HAND_L", "LLWRIST" },
            leftHandAKA = new List<string>() { "L_HM2", "Lt 3rd Digita", "LFIN", "LHAND" },
            leftIndexAKA = new List<string>() { "L_Index", "L_INDEX1", "L_MC1" },
            leftThumbAKA = new List<string>() { "L_Thumb","L_THUMB" },
        #endregion
        #region right hand
            rightWristAKA = new List<string>() { "R_USP", "R_ULNA", "RIGHT_WRIST_INNER", "R_WRIST_MED", "Rt Wrist Ulna", "RT Ulna", "RWRB", "R_HAND_M","RMWRIST" },
            rightWristRadiusAKA = new List<string>() {"R_RSP", "R_RADUIS", "RIGHT_WRIST_OUTER", "R_WRIST_LAT", "Rt Wrist Radius" ,"RT Radius", "RWRA", "R_HAND_L", "RLWRIST" },
            rightHandAKA = new List<string>() { "R_HM2", "Rt 3rd Digita", "RFIN", "RHAND" },
            rightIndexAKA = new List<string>() { "R_Index", "R_INDEX1", "R_MC1" },
            rightThumbAKA = new List<string>() { "R_Thumb", "R_THUMB" },
        #endregion
        #endregion
        #region knee
        #region left knee
            leftUpperKneeAKA = new List<string>() { "L_PAS", "L_SUPPAT" },
            leftOuterKneeAKA = new List<string>() { "L_FLE", "L_LKNEE", "LEFT_KNEE", "l_knjntln", "Lt Lat Knee", "L_KNEE_LAT", "LKNE", "LKNEE", "LLKNEE" },
            leftInnerKneeAKA = new List<string>() { "L_FME", "L_MKNEE", "LEFT_MEDIAL_KNEE", "Lt Medial Knee", "L_KNEE_MED", "LMKNEE" },
            leftLowerKneeAKA = new List<string>() { "L_TTC", "l_tubtib", "Lt Tibia", "L_TIB_1" },
        #endregion
        #region right knee

            rightUpperKneeAKA = new List<string>() {"R_PAS", "R_SUPPAT" },
            rightOuterKneeAKA = new List<string>() {"R_FLE", "R_LKNEE", "RIGHT_KNEE", "r_knjntln","Rt Lat Knee", "R_KNEE_LAT", "RKNE", "RKNEE", "RLKNEE" },
            rightInnerKneeAKA = new List<string>() {"R_FME", "R_MKNEE", "RIGHT_MEDIAL_KNEE","Rt Medial Knee", "R_KNEE_MED", "RMKNEE" },
            rightLowerKneeAKA = new List<string>() { "R_TTC", "r_tubtib", "Rt Tibia", "R_TIB_1" },

        #endregion
        #endregion
        #region foot
            #region left foot
            leftOuterAnkleAKA = new List<string>() { "L_FAL", "L_LMAL", "LEFT_ANKLE_OUTER", "l_ankle", "Lt Ankle", "L_ANKLE_LAT", "LANK", "LLANK" },
            leftInnerAnkleAKA = new List<string>() { "L_TAM", "L_MMAL", "LEFT_MEDIAL_ANKLE", "Lt Medial Ankle", "L_ANKLE_MED", "LMANK" },
            leftHeelAKA = new List<string>() {"L_FCC", "L_HEEL_CALC", "LEFT_HEEL", "l_heel", "L_FCC1", "L_FCC2", "Lt Heel", "LHEE", "L_h", "LHEEL", "L_HEEL" },
            #endregion
        #region right foot
            rightOuterAnkleAKA = new List<string>() {"R_FAL", "R_LMAL", "RIGHT_ANKLE_OUTER", "l_ankle", "Rt Ankle", "R_ANKLE_LAT", "RANK","RLANK" },
            rightInnerAnkleAKA = new List<string>() { "R_TAM", "R_MMAL", "RIGHT_MEDIAL_ANKLE", "Rt Medial Ankle", "R_ANKLE_MED", "RMANK" },
            rightHeelAKA = new List<string>() {"R_FCC", "R_HEEL_CALC", "RIGHT_HEEL", "r_heel", "R_FCC1", "R_FCC2", "Rt Heel", "RHEE", "R_h", "RHEEL", "R_HEEL" },
        #endregion
            #region toes
            #region left toes
            leftToe1AKA = new List<string>() { "L_FM1", "L_TOE_1_MET", "L_MT_1", "LTOE", "L_D1MT", "LFRST" },
            leftToe2AKA = new List<string>() { "L_FM2", "l_toe", "Lt 2nd Toe", "LFOOT" },
            leftToe5AKA = new List<string>() { "L_FM5", "L_TOE_5_MET", "Lt 5th Toe", "L_MT_5", "LMT5", "L_D5MT", "LFIFTH" },
            #endregion
            #region right toes
            rightToe1AKA = new List<string>() { "R_FM1", "R_TOE_1_MET", "R_MT_1", "RTOE", "R_D1MT", "RFRST" },
            rightToe2AKA = new List<string>() { "R_FM2", "r_toe","Rt 2nd Toe", "RFOOT" },
            rightToe5AKA = new List<string>() {"R_FM5", "R_TOE_5_MET","Rt 5th Toe", "R_MT_5", "RMT5", "R_D5MT", "RFIFTH" };
        #endregion
        #endregion
        #endregion
        /*
            public static readonly List<string>
             BodyBaseAKA = new List<string>() { "SACR", "SACRUM", "LOWER_LUMBAR", "LV5_S1", "S1" },
             HipAKA = new List<string>() { "IAS", "ASIS", "ASIS", "HIP", "ICT", "Hip Front", "FWT" },
             SpineAKA = new List<string>() 
                    {   "TV12", "TH12", "D12", "T12",
                        "TV11", "TH11", "D11", "T11", 
                        "TV13", "TH13", "D13", "T13", 
                        "TV10", "TH10", "D10", "T10", 
                        "TV14", "TV14", "D14", "T14",
                        "L1",   "LV3"       },

         NeckAKA = new List<string>() { "TV2", "TV1", "C7", "C7_TOP_SPINE" },
         ChestAKA = new List<string>() { "SME", "SJN", "CLAV" },
         ShoulderAKA = new List<string>() { "SAE", "ACR", "SHOULDER", "ACROMION", "SHO" },

         FrontHeadAKA = new List<string>() { "SGL", "Front of Head", "F_HEAD" },
         SideHeadAKA = new List<string>() { "HEAD", "HEADFRONT", "Head", "HEAD", "BHD" },

         ElbowAKA = new List<string>() { "UOA", "ELB", "ELBOW" },
         InnerElbowAKA = new List<string>() { "HME", "ELBOW_MED" },
         OuterElbowAKA = new List<string>() { "HLE", "ELBOW", "Elbow", "ELB" },

         WristAKA = new List<string>() { "USP", "ULNA", "WRIST_INNER", "WRIST_MED", "Wrist Ulna", "Ulna", "WRB" },
         WristRadiusAKA = new List<string>() { "RSP", "RADUIS", "WRIST_OUTER", "WRIST_LAT", "Wrist Radius", "Radius", "WRA" },

         HandAKA = new List<string>() { "HM2", "3rd Digita", "FIN" },
         IndexAKA = new List<string>() { "Index", "INDEX1" },
         ThumbAKA = new List<string>() { "Thumb", "THUMB" },

         UpperKneeAKA = new List<string>() { "PAS", "SUPPAT" },
         OuterKneeAKA = new List<string>() { "FLE", "LKNEE", "KNEE", "knjntln", "Knee", "KNEE_LAT", "KNE" },
         InnerKneeAKA = new List<string>() { "FME", "MKNEE", "MEDIAL_KNEE", "Medial Knee", "KNEE_MED" },
         LowerKneeAKA = new List<string>() { "TTC", "tubtib", "Tibia", "TIB_1" },

         OuterAnkleAKA = new List<string>() { "FAL", "LMAL", "ANKLE_OUTER", "ankle", "Ankle", "ANKLE_LAT", "ANK" },
         InnerAnkleAKA = new List<string>() { "TAM", "MMAL", "MEDIAL_ANKLE", "Medial Ankle", "ANKLE_MED" },

         HeelAKA = new List<string>() { "FCC", "HEEL_CALC", "HEEL", "heel", "FCC1", "FCC2", "Heel", "HEE" },

         Toe1AKA = new List<string>() { "FM1", "TOE_1_MET", "MT_1", "TOE" },
         Toe2AKA = new List<string>() { "FM2", "toe", "2nd Toe" },
         Toe5AKA = new List<string>() { "FM5", "TOE_5_MET", "5th Toe", "MT_5", "MT5" }
         ;
            */
    }
}
