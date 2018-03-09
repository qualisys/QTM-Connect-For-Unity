#region --- LICENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015 Qualisys AB

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QualisysRealTime.Unity.Skeleton
{
    public static class Extensions
    {
        /// <summary>
        /// Converts a OpenTK Vector3 to a Unity Vector3
        /// </summary>
        /// <param name="q">A OpenTK Vector3 to be converted</param>
        /// <returns>A Unity Vector3</returns>
        public static Vector3 Convert(this OpenTK.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
        /// <summary>
        /// Converts a Unity Vector3 to a OpenTK Vector3
        /// </summary>
        /// <param name="q">A Unity Vector3 to be converted</param>
        /// <returns>A OpenTK Vector3</returns>
        public static OpenTK.Vector3 Convert(this UnityEngine.Vector3 v)
        {
            return new OpenTK.Vector3(v.x, v.y, v.z);
        }
        /// <summary>
        /// Converts a OpenTK Quaternion to a Unity Quaternion
        /// </summary>
        /// <param name="q">A OpenTK Quaternion to be converted</param>
        /// <returns>A Unity Quaternion</returns>
        public static Quaternion Convert(this OpenTK.Quaternion q)
        {
            return new Quaternion(q.X, q.Y, q.Z, q.W);
        }
        /// <summary>
        /// Converts a Unity Quaternion to a OpenTK Quaternion
        /// </summary>
        /// <param name="q">A Unity Quaternion to be converted</param>
        /// <returns>A OpenTK Quaternion</returns>
        public static OpenTK.Quaternion Convert(this UnityEngine.Quaternion q)
        {
            return new OpenTK.Quaternion(q.x, q.y, q.z, q.w);
        }

        /// <summary>
        /// Returns every direct child of a GameObject
        /// </summary>
        /// <param name="parent">The parent of the childs</param>
        /// <returns>Array of direct children</returns>
        public static Transform[] GetDirectChildren(this Transform parent)
        {
            List<Transform> res = new List<Transform>();
            foreach (Transform child in parent) res.Add(child);
            return res.ToArray();
        }
        /// <summary>
        /// Returns true if the transforms has a given the child
        /// </summary>
        /// <param name="transform">The transform that is either a parent or not</param>
        /// <param name="child">The transform that is either a child or not of this</param>
        /// <returns>True if any fo the children is the given transform or the given transform is the same as this</returns>
        public static bool ContainsChild(this Transform transform, Transform child)
        {
            return 
                (transform == child) 
                || transform.GetComponentsInChildren<Transform>().Contains(child);
        }

        /// <summary>   
        /// Determines whether the Transform is an ancestor of this Transform.
        /// </summary>
        /// <param name="transform">The transform</param>
        /// <param name="ancestor">The  transform to be checked whether it is an ancestor of this or not</param>
        /// <returns>True if given transform is an ancestor of this transform, false otherwise</returns>
        public static bool IsAncestorOf(this Transform ancestor, Transform transform)
        {
            return
                (transform &&
                ancestor &&
                transform.parent &&
                transform.parent == ancestor) ||
                ( transform.parent && ancestor.IsAncestorOf(transform.parent));
        }
        /// <summary>
        /// Returns the first common ancestor of the two transforms
        /// </summary>
        /// <param name="t1">The first transform</param>
        /// <param name="t2">The secound transform</param>
        /// <returns>The transform that is the first common ancestor, null otherwise</returns>
        public static Transform CommonAncestorOf(this Transform t1, Transform t2)
        {
            if (!t1 || !t2) return null;
            else if (t1 == t2 && t1.parent) return t1.parent;
            else if (t1.parent && t2.parent && t1.parent == t2.parent) return t1.parent;
            else return t1.CommonAncestorsOfSearch(t2);
        }
        /// <summary>
        /// Returns the first common ancestor of the two transforms
        /// </summary>
        /// <param name="t1">The first transform</param>
        /// <param name="t2">The secound transform</param>
        /// <returns>The transform that is the first common ancestor, null otherwise</returns>
        public static Transform CommonAncestorsOfSearch(this Transform t1, Transform t2)
        {
            if (!t1 || !t2) return null;
            else if (t1.IsAncestorOf(t2)) return t1;
            else if (!t2.parent) return null;
            else return t2.parent.CommonAncestorsOfSearch(t1);
        }
    }
}