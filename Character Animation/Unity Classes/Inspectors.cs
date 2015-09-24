#region --- LICENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015 Qualisys AB

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using UnityEngine;

namespace QualisysRealTime.Unity.Skeleton {
    [System.Serializable]
    public class Skeleton
    {

        public Vector3 Offset = new Vector3(0, 0, 0);
        public bool showSkeleton = false;
        public Color skelettColor = Color.black;
        public bool showJoints = false;
        [Range(0.01f, 0.1f)]
        public float jointSize = 0.015f;
        public Color jointColor = Color.green;
        public bool showRotationTrace = false;
        [Range(0.01f, 0.5f)]
        public float traceLength = 0.08f;
        public bool showConstraints = false;
        public bool showTwistConstraints = false;
        [Range(10,500)]
        public int coneResolution = 50;
        [Range(0.01f, 0.5f)]
        public float coneSize = 0.08f;
        public Markers markers;
       [System.Serializable]
       public class Markers
       {
           public bool ShowMarkers = false;
           [Range(0.001f, 0.05f)]
           public float MarkerScale = 0.01f;
           public bool MarkerBones = false;
           public Color boneColor = Color.blue;
       }
    }
    [System.Serializable]
    public class HeadCam
    {
        public bool UseHeadCamera = false;
        public bool UseVRHeadSetRotation = true;
        public Vector3 CameraOffset = new Vector3(0, .11f, .11f);
    }
}
