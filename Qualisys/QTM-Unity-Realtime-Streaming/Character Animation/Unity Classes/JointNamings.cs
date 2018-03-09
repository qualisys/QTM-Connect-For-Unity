#region --- LICENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015 Qualisys AB

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System.Linq;
using UnityEngine;

namespace QualisysRealTime.Unity.Skeleton {
	/// <summary>
	/// Class for identifying biped bones based on most common naming conventions.
	/// </summary>
	public class JointNamings {
		
		/// <summary>
		/// Type of the bone.
		/// </summary>
		[System.Serializable]
		public enum JointObject {
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
        /// Bone side: Left and Right for limbs and Center for spine, head and tail.
        /// </summary>
        [System.Serializable]
        public enum BodySide
        {
            Left,
            Right,
            Center
        }
		
		/// <summary>
        ///  Charcters Joint names
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

            armAlias = { "Shoulder", "shoulder", "Collar", "collar", "Clavicle", "clavicle", "Arm", "arm", "Hand", "hand", "Wrist", "wrist", "Elbow", "elbow", "Palm", "palm" },

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
        /// Returns only the bones with the specified JointObject.
		/// </summary>
		public static Transform[] GetBonesOfType(JointObject type, Transform[] bones) {
            return bones.Where(b => (b != null && GetType(b.name) == type)).ToArray();
		}


        /// <summary>
        /// Returns only the joints with the specified Side.
        /// </summary>
        /// <param name="jointSide">The side of the joint</param>
        /// <param name="joints">The Transforms where to search</param>
        /// <returns>A list of matching Transforms</returns>
		public static Transform[] GetJointsOfSide(BodySide jointSide, Transform[] joints) {
            return joints.Where(j => (j != null && GetSideOfJointName(j.name) == jointSide)).ToArray();
		}

        /// <summary>
        /// Gets the joint of given joint type and joint side.
        /// </summary>
        /// <param name="jointType">The type of the joint </param>
        /// <param name="jointSide">The side on wich the joint should be located</param>
        /// <param name="transforms">The bones to search among</param>
        /// <returns>Null if no match, otherwise the first hit</returns>
		public static Transform[] GetTypeAndSide(JointObject jointType, BodySide jointSide, Transform[] transforms) {
			return GetJointsOfSide(jointSide, GetBonesOfType(jointType, transforms));
		}
		
		/// <summary>
		/// Returns only the bones that match all the namings in params string[][] namings
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
		public static Transform GetMatch(Transform[] transforms, params string[][] namings) {
           return transforms.FirstOrDefault(t => namings.All(n => Matches(t.name, n)));
		}

        /// <summary>
        /// Gets the type of the joint.
        /// </summary>
        /// <param name="boneName">The name of the joint</param>
        /// <returns>The enum of the joint name</returns>
		public static JointObject GetType(string boneName) {
            // Type Neck
            if (IsType(boneName, neckAlias, neckExclude)) return JointObject.Neck;
            // Type Spine
            if (IsType(boneName, spineAlias, spineExclude)) return JointObject.Spine;
            // Type Head
            if (IsType(boneName, headAlias, headExclude)) return JointObject.Head;
            // Type Arm
            if (IsType(boneName, armAlias, armExclude)) return JointObject.Arm;
            // Type Leg
            if (IsType(boneName, legAlias, legExclude)) return JointObject.Leg;
            // Type Finger
            if (IsType(boneName, fingerAlias, fingerExclude)) return JointObject.Fingers;
            // Type Thumb
            if (IsType(boneName, thumbAlias, thumbExclude)) return JointObject.Thumb;
			return JointObject.Unassigned;
		}

        /// <summary>
        /// Gets the joint side.
        /// </summary>
        /// <param name="jointName">The name of the joint</param>
        /// <returns>The enum representing the side</returns>
		public static BodySide GetSideOfJointName(string jointName) {
            if (Matches(jointName, LeftSide) || LastLetter(jointName) == "L" || FirstLetter(jointName) == "L")  return BodySide.Left;
            else if (Matches(jointName, RightSide) || LastLetter(jointName) == "R" || FirstLetter(jointName) == "R") return BodySide.Right;
			else return BodySide.Center;
		}

        /// <summary>
        /// Returns a tranform from a given JointType
        /// </summary>
        /// <param name="transforms">The transform to be searched</param>
        /// <param name="jointType">The type of the joint to be returned</param>
        /// <param name="jointSide">The side of which the joint is located</param>
        /// <param name="namings">The Names of the joints</param>
        /// <returns>The transform of the first result</returns>
		public static Transform GetBone(Transform[] transforms, JointObject jointType, BodySide jointSide = BodySide.Center, params string[][] namings) {
            return GetMatch(GetTypeAndSide(jointType, jointSide, transforms), namings);
		}
        /// <summary>
        /// Returns true if the joint matches the given names and not the given exlusions
        /// </summary>
        /// <param name="jointName">The name of the joint</param>
        /// <param name="aliases">The names to be matched</param>
        /// <param name="exclusions">The name the joint cannot contain</param>
        /// <returns>True if match, false otherwise</returns>
        private static bool IsType(string jointName, string[] aliases, string[] exclusions)
        {
            return Matches(jointName, aliases) && !Exclude(jointName, exclusions);
        }
        /// <summary>
        /// Returns true if the possible names matches the given name expect the exluded names
        /// </summary>
        /// <param name="jointName">The given name</param>
        /// <param name="possibleNames">The possible names</param>
        /// <returns>True if no match among the exluded and a match with the given, false otherwise</returns>
		private static bool Matches(string jointName, string[] possibleNames) {
            return !Exclude(jointName, exludeName) 
                && possibleNames.Any(nc => jointName.Contains(nc));
		}
		/// <summary>
        /// Returns true if the given names math the exclusions
		/// </summary>
		/// <param name="jointName">The given joint name</param>
		/// <param name="exclusions">The names to match</param>
		/// <returns>True if match, false otherwise</returns>
		private static bool Exclude(string jointName, string[] exclusions) {
            return exclusions.Any(nc => jointName.Contains(nc));
		}
		/// <summary>
		/// Returns the first letter of the string
		/// </summary>
		/// <param name="first">The string</param>
		/// <returns>The first letter. If string is empty, return The string</returns>
		private static string FirstLetter(string first) {
			return (first.Length > 0) ? first.Substring(0, 1) : first;
		}
		/// <summary>
        /// Returns the last letter of the string
        /// </summary>
		/// <param name="last"></param>
        /// <returns>The last letter. If string is empty: return The string</returns>
        private static string LastLetter(string last)
        {
            return (last.Length > 0) ? last.Substring(last.Length - 1, 1) : last;
		}
	}
}
