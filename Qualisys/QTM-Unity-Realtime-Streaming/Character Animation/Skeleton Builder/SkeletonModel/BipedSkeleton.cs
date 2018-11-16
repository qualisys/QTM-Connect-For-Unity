#region --- LICENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015-2018 Qualisys AB

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

namespace QualisysRealTime.Unity.Skeleton
{
    /// <summary>
    /// Segment identifiers
    /// </summary>
    public enum SegmentName
    {
        PELVIS,
        // Left leg chain
        HIP_L,
        KNEE_L,
        ANKLE_L,
        FOOTBASE_L,
        TOE_L,
        // Right leg chain
        HIP_R,
        KNEE_R,
        ANKLE_R,
        FOOTBASE_R,
        TOE_R,
        //Spine chain
        SPINE0,
        SPINE1,
        SPINE2,
        SPINE3,
        NECK,
        HEAD,
        HEADTOP,
        //Left arm chain
        CLAVICLE_L,
        SHOULDER_L,
        ELBOW_L,
        WRIST_L,
        TRAP_L,
        THUMB_L,
        HAND_L,
        INDEX_L,
        //Right arm chain
        CLAVICLE_R,
        SHOULDER_R,
        ELBOW_R,
        WRIST_R,
        TRAP_R,
        THUMB_R,
        HAND_R,
        INDEX_R
    };

    public class BipedSkeleton
    {
        protected TreeNode<Segment> root;
        public TreeNode<Segment> Root { get { return root; } }
        private ConstraintsExamples constraints = new ConstraintsExamples();

        public BipedSkeleton()
        {
            root = HAminStandard.GetHAminSkeleton();
            constraints.SetConstraints(this);
        }

        public Segment Find(SegmentName key)
        {
            foreach (var b in root)
                if (b.Data.Name == key) return b.Data;
            return null;
        }

        public Segment this[SegmentName key]
        {
            get
            {
                return root.FindTreeNode(node => node.Data != null && node.Data.Name.Equals(key)).Data;
            }
            set
            {
                root.FindTreeNode(node => node.Data.Name.Equals(key)).Data = value;
            }
        }
    }
}
