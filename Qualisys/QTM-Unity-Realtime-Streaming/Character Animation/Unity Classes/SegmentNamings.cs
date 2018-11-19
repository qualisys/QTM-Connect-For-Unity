#region --- LICENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015-2018 Qualisys AB

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System.Linq;
using UnityEngine;

namespace QualisysRealTime.Unity.Skeleton {
    /// <summary>
    /// Class for identifying biped segments based on most common naming conventions.
    /// </summary>
    public class SegmentNamings {
        
        /// <summary>
        /// Type of the segment.
        /// </summary>
        [System.Serializable]
        public enum SegmentObject
        {
            Unassigned,
            Spine,
            Neck,
            Head,
            Arm,
            Leg,
            Fingers,
            Thumb
        }
        /// <summary>
        /// Segment side: Left and Right for limbs and Center for spine, head and tail.
        /// </summary>
        [System.Serializable]
        public enum BodySide
        {
            Left,
            Right,
            Center
        }
        
        /// <summary>
        ///  Charcters Segment names
        /// </summary>
        public static string[]
            LeftSide = { " L ", "_L_", "-L-", " l ", "_l_", "-l-", "Left", "left", "CATRigL", "CATL" },
            RightSide = { " R ", "_R_", "-R-", " r ", "_r_", "-r-", "Right", "right", "CATRigR", "CATR" },

            pelvisAlias = { "Pelvis", "pelvis", "Hip", "hip" },
            handAlias = { "Hand", "hand", "Wrist", "wrist", "Palm", "palm" },
            footAlias = { "Foot", "foot", "Ankle", "ankle" },

            spineAlias = { "Spine", "spine", "Pelvis", "pelvis", "Root", "root", "Torso", "torso", "Body", "body", "Hips", "hips", "Chest", "chest" },
            neckAlias = { "Neck", "neck" },
            headAlias = { "Head", "head" },

            armAlias = { "Shoulder", "shoulder", "Shldr", "shldr", "Collar", "collar", "Clavicle", "clavicle", "Arm", "arm", "Hand", "hand", "Wrist", "wrist", "Elbow", "elbow", "Palm", "palm" },

            legAlias = { "Leg", "leg", "Thigh", "thigh", "Calf", "calf", "Femur", "femur", "Knee", "knee", "Shin", "shin", "Foot", "foot", "Ankle", "ankle", "Hip", "hip" },

            fingerAlias = { "Finger", "finger", "Index", "index", "Mid", "mid", "Pinky", "pinky", "Ring", "ring" },
            thumbAlias = { "Thumb", "thumb", "Finger0", "finger0" },

            exludeName = { "Nub", "Dummy", "dummy", "Tip", "IK", "Mesh", "mesh", "Goal", "goal", "Pole", "pole", "slot" },
            spineExclude = { "Head", "head", "Pelvis", "pelvis", "Hip", "hip", "Root", "root" },
            headExclude = { "Top", "End" },
            armExclude = { "Finger", "finger", "Index", "index", "Point", "point", "Mid", "mid", "Pinky", "pinky", "Pink", "pink", "Ring", "Thumb", "thumb", "Adjust", "adjust", "Twist", "twist", "fing", "Fing"},
            fingerExclude = { "Thumb", "thumb", "Adjust", "adjust", "Twist", "twist", "Medial", "medial", "Distal", "distal", "Finger0",
                                    "02",
                                    "11","12",
                                    "21","22",
                                    "31","32",
                                    "41","42",
                                    "51","52",
                                },
            thumbExclude = { "Adjust", "adjust", "Twist", "twist", "Medial", "medial", "Distal", "distal" },
            legExclude = { "Toe", "toe", "Platform", "Adjust", "adjust", "Twist", "twist", "Digit", "digit" },
            neckExclude = { "Head", "head"};

        /// <summary>
        /// Returns only the transforms with the specified SegmentObject name.
        /// </summary>
        public static Transform[] GetTransformsOfType(SegmentObject type, Transform[] transforms)
        {
            return transforms.Where(b => (b != null && GetType(b.name) == type)).ToArray();
        }


        /// <summary>
        /// Returns only the transforms with the specified Side.
        /// </summary>
        /// <param name="bodySide">The side of the body</param>
        /// <param name="transforms">The Transforms where to search</param>
        /// <returns>A list of matching Transforms</returns>
        public static Transform[] GetTransformsOfSide(BodySide bodySide, Transform[] transforms)
        {
            return transforms.Where(j => (j != null && GetBodySideFromName(j.name) == bodySide)).ToArray();
        }

        /// <summary>
        /// Gets the segment of given segment type and segment side.
        /// </summary>
        /// <param name="segmentGroup">The type of the segment </param>
        /// <param name="bodySide">The side on wich the segment should be located</param>
        /// <param name="transforms">The segments to search among</param>
        /// <returns>Null if no match, otherwise the first hit</returns>
        public static Transform[] GetTypeAndSide(SegmentObject segmentGroup, BodySide bodySide, Transform[] transforms)
        {
            return GetTransformsOfSide(bodySide, GetTransformsOfType(segmentGroup, transforms));
        }
        
        /// <summary>
        /// Returns only the segments that match all the namings in params string[][] namings
        /// </summary>
        /// <returns>
        /// The matching Transforms
        /// </returns>
        /// <param name='transforms'>
        /// Transforms.
        /// </param>
        /// <param name='namings'>
        /// Namings.
        /// </param>
        public static Transform GetMatch(Transform[] transforms, params string[][] namings)
        {
           return transforms.FirstOrDefault(t => namings.All(n => Matches(t.name, n)));
        }

        /// <summary>
        /// Gets SegmentObject from a segment name.
        /// </summary>
        /// <param name="segmentName">The name of the segment</param>
        /// <returns>The enum of the segment name</returns>
        public static SegmentObject GetType(string segmentName)
        {
            // Type Neck
            if (IsType(segmentName, neckAlias, neckExclude)) return SegmentObject.Neck;
            // Type Spine
            if (IsType(segmentName, spineAlias, spineExclude)) return SegmentObject.Spine;
            // Type Head
            if (IsType(segmentName, headAlias, headExclude)) return SegmentObject.Head;
            // Type Arm
            if (IsType(segmentName, armAlias, armExclude)) return SegmentObject.Arm;
            // Type Leg
            if (IsType(segmentName, legAlias, legExclude)) return SegmentObject.Leg;
            // Type Finger
            if (IsType(segmentName, fingerAlias, fingerExclude)) return SegmentObject.Fingers;
            // Type Thumb
            if (IsType(segmentName, thumbAlias, thumbExclude)) return SegmentObject.Thumb;
            return SegmentObject.Unassigned;
        }

        /// <summary>
        /// Gets the BodySide from a segment name.
        /// </summary>
        /// <param name="name">The name string</param>
        /// <returns>The enum representing the side</returns>
        public static BodySide GetBodySideFromName(string name)
        {
            if (Matches(name, LeftSide) || LastLetter(name) == "L" || FirstLetter(name) == "L")
                return BodySide.Left;
            else if (Matches(name, RightSide) || LastLetter(name) == "R" || FirstLetter(name) == "R")
                return BodySide.Right;
            return BodySide.Center;
        }

        /// <summary>
        /// Returns a tranform from a given segmentObject
        /// </summary>
        /// <param name="transforms">The transform to be searched</param>
        /// <param name="segmentObject">The type of the segment to be returned</param>
        /// <param name="bodySide">The side of which the segment is located</param>
        /// <param name="namings">The Names of the segments</param>
        /// <returns>The transform of the first result</returns>
        public static Transform GetSegment(Transform[] transforms, SegmentObject segmentObject, BodySide bodySide = BodySide.Center, params string[][] namings)
        {
            return GetMatch(GetTypeAndSide(segmentObject, bodySide, transforms), namings);
        }
        /// <summary>
        /// Returns true if the segment matches the given names and not the given exlusions
        /// </summary>
        /// <param name="name">The name of the segment</param>
        /// <param name="aliases">The names to be matched</param>
        /// <param name="exclusions">The name the segment cannot contain</param>
        /// <returns>True if match, false otherwise</returns>
        private static bool IsType(string name, string[] aliases, string[] exclusions)
        {
            return Matches(name, aliases) && !Exclude(name, exclusions);
        }
        /// <summary>
        /// Returns true if the possible names matches the given name expect the exluded names
        /// </summary>
        /// <param name="segmentName">The given name</param>
        /// <param name="possibleNames">The possible names</param>
        /// <returns>True if no match among the exluded and a match with the given, false otherwise</returns>
        private static bool Matches(string segmentName, string[] possibleNames)
        {
            return !Exclude(segmentName, exludeName) && possibleNames.Any(nc => segmentName.Contains(nc));
        }
        /// <summary>
        /// Returns true if the given names math the exclusions
        /// </summary>
        /// <param name="segmentName">The given segment name</param>
        /// <param name="exclusions">The names to match</param>
        /// <returns>True if match, false otherwise</returns>
        private static bool Exclude(string segmentName, string[] exclusions)
        {
            return exclusions.Any(nc => segmentName.Contains(nc));
        }
        /// <summary>
        /// Returns the first letter of the string
        /// </summary>
        /// <param name="first">The string</param>
        /// <returns>The first letter. If string is empty, return The string</returns>
        private static string FirstLetter(string first)
        {
            return (first.Length > 0) ? first.ToUpper().Substring(0, 1) : first;
        }
        /// <summary>
        /// Returns the last letter of the string
        /// </summary>
        /// <param name="last"></param>
        /// <returns>The last letter. If string is empty: return The string</returns>
        private static string LastLetter(string last)
        {
            return (last.Length > 0) ? last.ToUpper().Substring(last.Length - 1, 1) : last;
        }
    }
}