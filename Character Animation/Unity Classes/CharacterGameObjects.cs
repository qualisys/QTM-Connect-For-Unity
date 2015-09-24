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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QualisysRealTime.Unity.Skeleton {

	/// <summary>
	/// References to transform joints of the character.
	/// </summary>
    [System.Serializable]
    public class CharacterGameObjects : IEnumerable<Transform>
    {
        #region Central joints transforms
        public Transform root;
        public Transform pelvis;
        public Transform[] spine;
        public Transform neck;
        public Transform head;
        #endregion
        #region left leg transforms
        public Transform leftThigh;
        public Transform leftCalf;
        public Transform leftFoot;
        #endregion
        #region right leg transforms
        public Transform rightThigh;
        public Transform rightCalf;
        public Transform rightFoot;
        #endregion
        #region left arm transforms
        public Transform leftClavicle;
        public Transform leftUpperArm;
        public Transform leftForearm;
        public Transform leftHand;
        #endregion
        #region right arm transforms
        public Transform rightClavicle;
        public Transform rightUpperArm;
        public Transform rightForearm;
        public Transform rightHand;
        #endregion
        #region fingers transforms
        public Transform thumbLeft;

        public Transform thumbRight;

        public Transform[] fingersLeft;

        public Transform[] fingersRight;
        #endregion


        /// <summary>
        /// Sets the references to the joints of a character. Returns true if a valid biped has been found.
        /// </summary>
        /// <param name="root">The root of the character</param>
        /// <param name="useFingers">bool wheter to use fingers or not</param>
        /// <returns></returns>
        public bool SetLimbs(Transform root, bool useFingers = false)
        {
            this.root = root;

            // Find with the help of the animator
            AnimatorFindJoints(root.GetComponent<Animator>(), useFingers);
            if (IsAllJointsSet(useFingers))
            {
                return true;
            }
            else
            {
                // Try to find by names
                FindJointsByNaming(root, useFingers);
                return IsAllJointsSet(useFingers);
            }
        }


        /// <summary>
        /// Add gameobjects using the Animator.
        /// </summary>
        public void AnimatorFindJoints(Animator animator, bool useFingers)
        {
            if (!animator) return;

            if (!pelvis) pelvis = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (!head) head = animator.GetBoneTransform(HumanBodyBones.Head);

            if (!neck) neck = animator.GetBoneTransform(HumanBodyBones.Neck);

            if (spine == null)
            { 
                spine = new Transform[2];
                spine[0] = animator.GetBoneTransform(HumanBodyBones.Spine);
                spine[1] = animator.GetBoneTransform(HumanBodyBones.Chest);
            }

            if (!leftClavicle) leftClavicle = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            if (!rightClavicle) rightClavicle = animator.GetBoneTransform(HumanBodyBones.RightShoulder);

            if (!leftThigh) leftThigh = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            if (!leftCalf) leftCalf = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            if (!leftFoot) leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);

            if (!rightThigh) rightThigh = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            if (!rightCalf) rightCalf = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            if (!rightFoot) rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

            if (!leftClavicle) leftClavicle = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            if (!leftUpperArm) leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            if (!leftForearm) leftForearm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            if (!leftHand) leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);

            if (!rightClavicle) rightClavicle = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            if (!rightUpperArm) rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            if (!rightForearm) rightForearm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            if (!rightHand) rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);


            // Add fingers
            if (useFingers)
            {
                if (!thumbRight) thumbRight = animator.GetBoneTransform(HumanBodyBones.RightThumbProximal);
                if (!thumbLeft) thumbLeft = animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal);

                if (fingersLeft == null) fingersLeft = new Transform[4];
                fingersLeft[0] = animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
                fingersLeft[1] = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
                fingersLeft[2] = animator.GetBoneTransform(HumanBodyBones.LeftRingProximal);
                fingersLeft[3] = animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal);

                if (fingersRight == null) fingersRight = new Transform[4];
                fingersRight[0] = animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
                fingersRight[1] = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
                fingersRight[2] = animator.GetBoneTransform(HumanBodyBones.RightRingProximal);
                fingersRight[3] = animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);
            }
        }
        /// <summary>
        /// Detects joints based on names and the hierarchy.
        /// </summary>
        /// <param name="root">The root of the biped humanoid</param>
        /// <param name="useFingers">bool wheter to include fingers or not</param>
        public void FindJointsByNaming(Transform root, bool useFingers)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>();

            // Find limbs
            // Get left arm
            AddLeftArm(transforms);
            //// Get Right arm
            AddRightArm(transforms);
            // Get left leg
            AddLeftLeg(transforms);
            // Get right leg
            AddRightLeg(transforms);
            // Find fingers
            if (useFingers && !IsAllFingersSet())
            {
                AddFingers();
                if (!IsAllFingersSet())
                {
                    fingersLeft = null;
                    fingersRight = null;
                }
            }
            // Find head bone
            if (!head) head = JointNamings.GetBone(transforms, JointNamings.JointObject.Head);
            // Find Neck
            if (!neck) neck = JointNamings.GetBone(transforms, JointNamings.JointObject.Neck);
            // Find Pelvis
            if (!pelvis) pelvis = JointNamings.GetMatch(transforms, JointNamings.pelvisAlias);
            //if pelvis not found, pelvis is common ancestor of the thighs
            if (!pelvis) pelvis = rightThigh.CommonAncestorOf(leftThigh);

            // Find spine
            Transform left, right;
            if (leftClavicle && rightClavicle)
            {
                left = leftClavicle;
                right = rightClavicle;
            } else
            {
                left = leftUpperArm;
                right = rightUpperArm;
            }
            if (left && right && pelvis)
            {
                Transform lastSpine = left.CommonAncestorOf(right);
                if (lastSpine)
                {
                    // if pelvis is not ancestor of last spine
                    if (!pelvis.IsAncestorOf(lastSpine))
                    {
                        // Set common ancestor to pelvis
                        pelvis = pelvis.CommonAncestorOf(lastSpine);
                    }
                    // Spine is all ancestors between last spine and pelvis
                    spine = GetAncestorsBetween(lastSpine, pelvis);
                    // Head is not set
                    if (!head)
                    {
                        for (int i = 0; i < lastSpine.childCount; i++)
                        {
                            Transform child = lastSpine.GetChild(i);
                            // all children of last spine that is not left and right
                            if (!child.ContainsChild(left) && !child.ContainsChild(right))
                            {
                                // if that object has a child, it is probably the head and that object the neck
                                if (child.childCount == 1)
                                {
                                    head = child.GetChild(0);
                                    neck = child;
                                    break;
                                }
                                // otherwise we set last spine as neck and its child as head
                                else
                                {
                                    head = child;
                                    neck = lastSpine;
                                    break;
                                }
                            }
                        }
                    }
                    else if (!neck)
                    {  // if Neck is not set but head is
                        if (lastSpine.IsAncestorOf(head))
                        {
                            neck = lastSpine;
                        }
                        else
                        {
                            for (int i = 0; i < lastSpine.childCount; i++)
                            {
                                Transform child = lastSpine.GetChild(i);

                                if (!child.ContainsChild(left) && !child.ContainsChild(right))
                                {
                                    neck = child;
                                }
                            }
                        }
                    }
                }
                if (neck && spine.Length > 0 && spine[spine.Length-1] == neck)
                {
                    Array.Resize(ref spine, spine.Length - 1); // Can't have neck among the spines
                }
            }
        }
        /// <summary>
        /// Adds fingers to the references
        /// </summary>
        /// <returns>True if fingers were found</returns>
        public void AddFingers() 
        {
            if (leftHand.childCount <= 0 || rightHand.childCount <= 0) return;
            var children = leftHand.GetDirectChildren();
            thumbLeft = JointNamings.GetBone(children, JointNamings.JointObject.Thumb, JointNamings.BodySide.Left);
            Transform[] results = JointNamings.GetTypeAndSide(JointNamings.JointObject.Fingers, JointNamings.BodySide.Left, children);
            if (results.Length == 4)
            {
                fingersLeft = new Transform[4];
                fingersLeft[0] = results[0];
                fingersLeft[1] = results[1];
                fingersLeft[2] = results[2];
                fingersLeft[3] = results[3];
            }
            else if (results.Length == 1) 
            {
                fingersLeft = new Transform[1];
                fingersLeft[0] = results[0];
            }
            else if (leftHand && leftHand.childCount >= 5)
            {
                if (!thumbLeft) thumbLeft = leftHand.GetChild(0);
                fingersLeft = new Transform[4];
                fingersLeft[0] = leftHand.GetChild(1);
                fingersLeft[1] = leftHand.GetChild(2);
                fingersLeft[2] = leftHand.GetChild(3);
                fingersLeft[3] = leftHand.GetChild(4);
            }

            children = rightHand.GetDirectChildren();
            thumbRight = JointNamings.GetBone(children, JointNamings.JointObject.Thumb, JointNamings.BodySide.Right);
            results = JointNamings.GetTypeAndSide(JointNamings.JointObject.Fingers, JointNamings.BodySide.Right, rightHand.GetDirectChildren());
            if (results.Length == 4)
            {
                fingersRight = new Transform[4];
                fingersRight[0] = results[0];
                fingersRight[1] = results[1];
                fingersRight[2] = results[2];
                fingersRight[3] = results[3];
            }
            else if (results.Length == 1) 
            {
                fingersRight = new Transform[1];
                fingersRight[0] = results[0];
            }
            else if (rightHand && rightHand.childCount >= 5)
            {
                if (!thumbRight) thumbRight = rightHand.GetChild(0);
                fingersRight = new Transform[4];
                fingersRight[0] = rightHand.GetChild(1);
                fingersRight[1] = rightHand.GetChild(2);
                fingersRight[2] = rightHand.GetChild(3);
                fingersRight[3] = rightHand.GetChild(4);
            }
        }
        /// <summary>
        /// Finding and adding left arm
        /// </summary>
        /// <param name="transforms"></param>
        public void AddLeftArm(Transform[] transforms)
        {
            Transform[] results = JointNamings.GetTypeAndSide(JointNamings.JointObject.Arm, JointNamings.BodySide.Left, transforms);
            if (results.Length == 4)
            {
                if (!leftClavicle) leftClavicle = results[0];
                if (!leftUpperArm) leftUpperArm = results[1];
                if (!leftForearm) leftForearm = results[2];
                if (!leftHand) leftHand = results[3];
            }
            else if (results.Length == 3)
            {
                if (!leftUpperArm) leftUpperArm = results[0];
                if (!leftForearm) leftForearm = results[1];
                if (!leftHand) leftHand = results[2];
            }
            else foreach (var res in results) UnityEngine.Debug.Log(res);

        }
        /// <summary>
        /// Finding and adding right arm
        /// </summary>
        /// <param name="transforms"></param>
        public void AddRightArm(Transform[] transforms)
        {
            Transform[] results = JointNamings.GetTypeAndSide(JointNamings.JointObject.Arm, JointNamings.BodySide.Right, transforms);
            if (results.Length == 4)
            {
                if (!rightClavicle) rightClavicle = results[0];
                if (!rightUpperArm) rightUpperArm = results[1];
                if (!rightForearm) rightForearm = results[2];
                if (!rightHand) rightHand = results[3];
            }
        }
        /// <summary>
        /// Finding and adding left leg
        /// </summary>
        /// <param name="transforms"></param>
        public void AddLeftLeg(Transform[] transforms)
        {
            Transform[] results = JointNamings.GetTypeAndSide(JointNamings.JointObject.Leg, JointNamings.BodySide.Left, transforms);
            if (results.Length == 3 || results.Length == 4)
            {
                if (leftThigh == null) leftThigh = results[0];
                if (leftCalf == null) leftCalf = results[1];
                if (leftFoot == null) leftFoot = results[2];
            }
        }
        /// <summary>
        /// Finding and adding right leg        
        /// </summary>
        /// <param name="transforms"></param>
        public void AddRightLeg(Transform[] transforms)
        {
            Transform[] results = JointNamings.GetTypeAndSide(JointNamings.JointObject.Leg, JointNamings.BodySide.Right, transforms);
            if (results.Length == 3 || results.Length == 4)
            {
                if (rightThigh == null) rightThigh = results[0];
                if (rightCalf == null) rightCalf = results[1];
                if (rightFoot == null) rightFoot = results[2];
            }
        }

        /// <summary>
        /// Returns a array of all ancestors of Transform 1 until given Transform 2 or no more parents. Including Transform 1
        /// </summary>
        /// <param name="from">The starting transform, array is inclusive this</param>
        /// <param name="until">The transform to stop at, the array is exclusive this.</param>
        /// <returns>An array with the transform between From and Until</returns>
        public Transform[] GetAncestorsBetween(Transform from, Transform until)
        {
            List<Transform> between = new List<Transform>();
            var temp = from;
            while (    temp
                    && temp != until 
                    && temp != root
                    && until.IsAncestorOf(from))
            {
                if (temp.position != until.position 
                    && (!temp.parent ||  temp.parent.position != temp.position))
                {
                    between.Add(temp);
                }
                temp = temp.parent;
            }
            between.Reverse();
            return between.ToArray();
        }

        /// <summary>
        /// Checks for null among the joints, if any joints could not be identified
        ///
        /// </summary>
        /// <param name="useFingers">bool whether fingers should be included or not</param>
        /// <returns>True if all joints are not null</returns>
        public bool IsAllJointsSet(bool useFingers)
        {
            return
                root &&
                pelvis &&
                leftThigh &&
                leftCalf &&
                leftFoot &&

                rightThigh &&
                rightCalf &&
                rightFoot &&

                leftClavicle &&
                leftUpperArm &&
                leftForearm &&
                leftHand &&

                rightClavicle &&
                rightUpperArm &&
                rightForearm &&
                rightHand &&
                neck &&
                spine != null &&
                spine.All(s => s) &&
                (useFingers ? IsAllFingersSet() : true);
        }

        /// <summary>
        /// Check for null references among fingers.
        /// </summary>
        /// <returns>True if all fingers are set</returns>
        public bool IsAllFingersSet()
        {
            return
                fingersLeft != null &&
                fingersLeft.All(f => f) &&
                fingersRight != null &&
                fingersRight.All(f => f) &&
                thumbLeft &&
                thumbRight;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Transform> GetEnumerator()
        {
            yield return root;
            yield return pelvis;
            if (spine != null)
                foreach (var s in spine)
                    yield return s;

            yield return neck;
            yield return head;
            
            yield return leftThigh;
            yield return leftCalf;
            yield return leftFoot;
            
            yield return rightThigh;
            yield return rightCalf;
            yield return rightFoot;
            
            yield return leftClavicle;
            yield return leftUpperArm;
            yield return leftForearm;
            yield return leftHand;
            yield return thumbLeft;
            if (fingersLeft != null) 
                foreach (var f in fingersLeft) 
                    yield return f;

            yield return rightClavicle;
            yield return rightUpperArm;
            yield return rightForearm;
            yield return rightHand;
            yield return thumbRight;
            if (fingersRight != null) 
                foreach (var f in fingersRight) 
                    yield return f;
        }
        public void PrintAll()
        {
            UnityEngine.Debug.LogFormat("root {0}", root);
            UnityEngine.Debug.LogFormat("pelvis {0}", pelvis);
            if (spine != null)
                for (int s = 0; s < spine.Length; s++)
                    UnityEngine.Debug.LogFormat("spine{0} {1}", s, spine[s]);
            UnityEngine.Debug.LogFormat("neck {0}", neck);
            UnityEngine.Debug.LogFormat("head {0}", head);

            UnityEngine.Debug.LogFormat("leftThigh {0}", leftThigh);
            UnityEngine.Debug.LogFormat("leftCalf {0}", leftCalf);
            UnityEngine.Debug.LogFormat("leftFoot {0}", leftFoot);

            UnityEngine.Debug.LogFormat("rightThigh {0}", rightThigh);
            UnityEngine.Debug.LogFormat("rightCalf {0}", rightCalf);
            UnityEngine.Debug.LogFormat("rightFoot {0}", rightFoot);

            UnityEngine.Debug.LogFormat("leftClavicle {0}", leftClavicle);
            UnityEngine.Debug.LogFormat("leftUpperArm {0}", leftUpperArm);
            UnityEngine.Debug.LogFormat("leftForearm {0}", leftForearm);
            UnityEngine.Debug.LogFormat("leftHand {0}", leftHand);
            UnityEngine.Debug.LogFormat("thumbLeft {0}", thumbLeft);
            if (fingersLeft != null)
                for (int f = 0; f < fingersLeft.Length; f++) 
                    UnityEngine.Debug.LogFormat("fingersLeft{0} {1}", f, fingersLeft[f]);

            UnityEngine.Debug.LogFormat("rightClavicle {0}", rightClavicle);
            UnityEngine.Debug.LogFormat("rightUpperArm {0}", rightUpperArm);
            UnityEngine.Debug.LogFormat("rightForearm {0}", rightForearm);
            UnityEngine.Debug.LogFormat("rightHand {0}", rightHand);
            UnityEngine.Debug.LogFormat("thumbRight {0}", thumbRight);
            if (fingersRight != null)
                for (int f = 0; f < fingersRight.Length; f++ ) 
                    UnityEngine.Debug.LogFormat("fingersRight{0} {1}", f, fingersRight[f]);
        }
    }
}
