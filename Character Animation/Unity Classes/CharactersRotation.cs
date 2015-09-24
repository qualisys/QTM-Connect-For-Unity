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

namespace QualisysRealTime.Unity.Skeleton
{
    [System.Serializable]
    public enum CharactersModel
    {
        Model1, Model2, Model3, Model4, Model5, Model6, Model7, EmptyModel
    }
    [System.Serializable]
    public class BoneRotations
    {
        public Vector3
            root = new Vector3(0f, 0f, 270f),
        hip = new Vector3(0f, 270f, 0f),
        spine = new Vector3(0f, 270f, 0f),
        neck = new Vector3(0f, 270f, 0f),
        head = new Vector3(0f, 270f, 0f),

        legUpperLeft = new Vector3(0f, 270f, 0f),
        legLowerLeft = new Vector3(0f, 270f, 0f),
        footLeft = new Vector3(0f, 270f, 90f),

        legUpperRight = new Vector3(0f, 270f, 0f),
        legLowerRight = new Vector3(0f, 270f, 0f),
        footRight = new Vector3(0f, 270f, 90f),

        clavicleLeft = new Vector3(0f, 90f, 0f),
        armUpperLeft = new Vector3(0f, 90f, 0f),
        armLowerLeft = new Vector3(0f, 90f, 0f),
        handLeft = new Vector3(0f, 180f, 0f),
        thumbLeft = new Vector3(0f, 180f, 0f),
        fingersLeft = new Vector3(0f, 180f, 0f),

        clavicleRight = new Vector3(0f, 90f, 0f),
        armUpperRight = new Vector3(0f, 90f, 0f),
        armLowerRight = new Vector3(0f, 90f, 0f),
        handRight = new Vector3(0f, 0f, 0f),
        thumbRight = new Vector3(0f, 0f, 0f),
        fingersRight = new Vector3(0f, 0f, 0f),
        headCamera = new Vector3(0f, 270f, 270f);
    }
    [System.Serializable]
    public class Model1 : BoneRotations
    {
        public Model1()
        {
            root = new Vector3(0f, 0f, 270f);
            hip = new Vector3(0f, 270f, 0f);
            spine = new Vector3(0f, 270f, 0f);
            neck = new Vector3(0f, 270f, 0f);
            head = new Vector3(0f, 270f, 0f);

            legUpperLeft = new Vector3(0f, 270f, 0f);
            legLowerLeft = new Vector3(0f, 270f, 0f);
            footLeft = new Vector3(0f, 270f, 90f);

            legUpperRight = new Vector3(0f, 270f, 0f);
            legLowerRight = new Vector3(0f, 270f, 0f);
            footRight = new Vector3(0f, 270f, 90f);

            clavicleLeft = new Vector3(0f, 90f, 0f);
            armUpperLeft = new Vector3(0f, 90f, 0f);
            armLowerLeft = new Vector3(0f, 90f, 0f);
            handLeft = new Vector3(0f, 180f, 0f);
            thumbLeft = new Vector3(0f, 180f, 0f);
            fingersLeft = new Vector3(0f, 180f, 0f);

            clavicleRight = new Vector3(0f, 90f, 0f);
            armUpperRight = new Vector3(0f, 90f, 0f);
            armLowerRight = new Vector3(0f, 90f, 0f);
            handRight = new Vector3(0f, 0f, 0f);
            thumbRight = new Vector3(0f, 0f, 0f);
            fingersRight = new Vector3(0f, 0f, 0f);
            headCamera = new Vector3(0f, 270f, 270f);
        }
    }
    [System.Serializable]
    public class Model2 : BoneRotations
    {
        public Model2()
        {
            root = new Vector3(0f, 0f, 0f);
            hip = new Vector3(0f, 0f, 0f);
            spine = new Vector3(0f, 0f, 0f);
            neck = new Vector3(0f, 0f, 0f);
            head = new Vector3(0f, 0f, 0f);

            legUpperLeft = new Vector3(0f, 0f, 180f);
            legLowerLeft = new Vector3(0f, 0f, 180f);
            footLeft = new Vector3(270f, 0f, 180f);

            legUpperRight = new Vector3(0f, 0f, 180f);
            legLowerRight = new Vector3(0f, 0f, 180f);
            footRight = new Vector3(270f, 0f, 180f);

            clavicleLeft = new Vector3(0f, 0f, 270f);
            armUpperLeft = new Vector3(0f, 0f, 260f);
            armLowerLeft = new Vector3(0f, 0f, 270f);
            handLeft = new Vector3(345f, 0f, 270f);
            thumbLeft = new Vector3(270f, 0f, 270f);
            fingersLeft = new Vector3(10f, 0f, 270f);

            clavicleRight = new Vector3(0f, 0f, 90f);
            armUpperRight = new Vector3(0f, 0f, 100f);
            armLowerRight = new Vector3(0f, 0f, 90f);
            handRight = new Vector3(345f, 0f, 90f);
            thumbRight = new Vector3(270f, 0f, 90f);
            fingersRight = new Vector3(0f, 0f, 90f);
            headCamera = new Vector3(0f, 0f, 0f);
        }
    }
    [System.Serializable]
    public class Model3 : BoneRotations
    {
        public Model3()
        {
            root = new Vector3(0f, 0f, 0f);
            hip = new Vector3(90f, 180f, 0f);
            spine = new Vector3(90f, 180f, 0f);
            neck = new Vector3(90f, 180f, 0f);
            head = new Vector3(90f, 180f, 0f);

            legUpperLeft = new Vector3(90f, 0f, 0f);
            legLowerLeft = new Vector3(90f, 0f, 0f);
            footLeft = new Vector3(40f, 0f, 0f);

            legUpperRight = new Vector3(90f, 0f, 0f);
            legLowerRight = new Vector3(90f, 0f, 0f);
            footRight = new Vector3(40f, 0f, 0f);

            clavicleLeft = new Vector3(90f, 0f, 0f);
            armUpperLeft = new Vector3(90f, 0f, 0f);
            armLowerLeft = new Vector3(90f, 0f, 0f);
            handLeft = new Vector3(90f, 90f, 0f);
            thumbLeft = new Vector3(90f, 0f, 0f);
            fingersLeft = new Vector3(90f, 0f, 0f);

            clavicleRight = new Vector3(90f, 0f, 0f);
            armUpperRight = new Vector3(90f, 0f, 0f);
            armLowerRight = new Vector3(90f, 0f, 0f);
            handRight = new Vector3(90f, 0f, 0f);
            thumbRight = new Vector3(90f, 0f, 0f);
            fingersRight = new Vector3(90f, 0f, 0f);
            headCamera = new Vector3(0f, 0f, 0f);
        }
    }
    [System.Serializable]
    public class Model4 : BoneRotations
    {
        public Model4()
        {
            root = new Vector3(0f, 90f, 270f);
            hip = new Vector3(0f, 0f, 0f);
            spine = new Vector3(0f, 0f, 0f);
            neck = new Vector3(0f, 0f, 0f);
            head = new Vector3(0f, 0f, 0f);

            legUpperLeft = new Vector3(0f, 0f, 0f);
            legLowerLeft = new Vector3(30f, 0f, 0f);
            footLeft = new Vector3(270f, 0f, 0f);

            legUpperRight = new Vector3(0f, 0f, 0f);
            legLowerRight = new Vector3(30f, 0f, 0f);
            footRight = new Vector3(270f, 0f, 0f);

            clavicleLeft = new Vector3(0f, 180f, 215f);
            armUpperLeft = new Vector3(0f, 180f, 270f);
            armLowerLeft = new Vector3(0f, 180f, 270f);
            handLeft = new Vector3(90f, 270f, 0f);
            thumbLeft = new Vector3(90f, 270f, 0f);
            fingersLeft = new Vector3(90f, 270f, 0f);

            clavicleRight = new Vector3(0f, 180f, 145f);
            armUpperRight = new Vector3(0f, 180f, 90f);
            armLowerRight = new Vector3(0f, 180f, 90f);
            handRight = new Vector3(90f, 90f, 0f);
            thumbRight = new Vector3(90f, 90f, 0f);
            fingersRight = new Vector3(90f, 90f, 0f);
            headCamera = new Vector3(0f, 0f, 0f);
        }
    }
    [System.Serializable]
    public class Model5 : BoneRotations
    {
        public Model5()
        {
            root = new Vector3(0f, 90f, 270f);
            hip = new Vector3(0f, 0f, 90f);
            spine = new Vector3(0f, 0f, 0f);
            neck = new Vector3(0f, 0f, 0f);
            head = new Vector3(90f, 0f, 0f);

            legUpperLeft = new Vector3(0f, 0f, 0f);
            legLowerLeft = new Vector3(0f, 0f, 0f);
            footLeft = new Vector3(0f, 0f, 0f);

            legUpperRight = new Vector3(0f, 180f, 0f);
            legLowerRight = new Vector3(0f, 180f, 0f);
            footRight = new Vector3(0f, 180f, 0f);

            clavicleLeft = new Vector3(0f, 270f, 0f);
            armUpperLeft = new Vector3(0f, 270f, 0f);
            armLowerLeft = new Vector3(0f, 270f, 0f);
            handLeft = new Vector3(0f, 270f, 0f);
            thumbLeft = new Vector3(0f, 0f, 0f);
            fingersLeft = new Vector3(0f, 0f, 0f);

            clavicleRight = new Vector3(0f, 270f, 0f);
            armUpperRight = new Vector3(0f, 270f, 0f);
            armLowerRight = new Vector3(0f, 270f, 0f);
            handRight = new Vector3(0f, 270f, 0f);
            thumbRight = new Vector3(0f, 0f, 0f);
            fingersRight = new Vector3(0f, 0f, 0f);
            headCamera = new Vector3(0f, 90f, 0f);
        }
    }
    [System.Serializable]
    public class Model6 : BoneRotations
    {
        public Model6()
        {
            root  = new Vector3(0f, 0f, 0f);
            hip   = new Vector3(0f, 270f, 270f);
            spine = new Vector3(0f, 270f, 270f);
            neck  = new Vector3(0f, 270f, 270f);
            head  = new Vector3(0f, 270f, 270f);

            legUpperLeft = new Vector3(0f, 90f, 270f);
            legLowerLeft = new Vector3(0f, 90f, 270f);
            footLeft     = new Vector3(0f, 270f, 310f);

            legUpperRight = new Vector3(0f, 270f, 90f);
            legLowerRight = new Vector3(0f, 270f, 90f);
            footRight     = new Vector3(0f, 270f, 130f);

            clavicleLeft = new Vector3(180f, 180f, 90f);
            armUpperLeft = new Vector3(180f, 180f, 90f);
            armLowerLeft = new Vector3(180f, 180f, 30f);
            handLeft     = new Vector3(0f, 0f, 270f);
            thumbLeft    = new Vector3(0f, 0f, 270f);
            fingersLeft  = new Vector3(0f, 0f, 270f);

            clavicleRight = new Vector3(0f, 180f, 90f);
            armUpperRight = new Vector3(0f, 180f, 90f);
            armLowerRight = new Vector3(0f, 270f, 90f);
            handRight     = new Vector3(0f, 180f, 90f);
            thumbRight    = new Vector3(0f, 180f, 90f);
            fingersRight  = new Vector3(0f, 180f, 90f);
            headCamera = new Vector3(0f, 270f, 270f);
        }
    }
    [System.Serializable]
    public class Model7 : BoneRotations
    {
        public Model7()
        {
            root = new Vector3(0f, 0f, 270f);
            hip = new Vector3(0f, 0f, 0f);
            spine = new Vector3(0f, 0f, 0f);
            neck = new Vector3(0f, 0f, 0f);
            head = new Vector3(0f, 0f, 0f);

            legUpperLeft = new Vector3(0f, 90f, 0f);
            legLowerLeft = new Vector3(0f, 180f, 0f);
            footLeft = new Vector3(30f, 180f, 0f);

            legUpperRight = new Vector3(0f, 270f, 0f);
            legLowerRight = new Vector3(0f, 180f, 0f);
            footRight = new Vector3(30f, 180f, 0f);

            clavicleLeft = new Vector3(0f, 90f, 0f);
            armUpperLeft = new Vector3(0f, 180f, 0f);
            armLowerLeft = new Vector3(0f, 180f, 0f);

            handLeft = new Vector3(0f, 90f, 330f);
            thumbLeft = new Vector3(0f, 0f, 0f);
            fingersLeft = new Vector3(0f, 0f, 0f);

            clavicleRight = new Vector3(0f, 0f, 0f);
            armUpperRight = new Vector3(0f, 0f, 0f);
            armLowerRight = new Vector3(0f, 0f, 0f);

            handRight = new Vector3(0f, 270f, 30f);
            thumbRight = new Vector3(0f, 0f, 0f);
            fingersRight = new Vector3(0f, 0f, 0f);
            headCamera = new Vector3(0f, 0f, 270f);
        }
    }
    [System.Serializable]
    public class Empty : BoneRotations
    {
        public Empty()
        {
            root = new Vector3(0f, 0f, 0f);
            hip = new Vector3(0f, 0f, 0f);
            spine = new Vector3(0f, 0f, 0f);
            neck = new Vector3(0f, 0f, 0f);
            head = new Vector3(0f, 0f, 0f);

            legUpperLeft = new Vector3(0f, 0f, 0f);
            legLowerLeft = new Vector3(0f, 0f, 0f);
            footLeft = new Vector3(0f, 0f, 0f);

            legUpperRight = new Vector3(0f, 0f, 0f);
            legLowerRight = new Vector3(0f, 0f, 0f);
            footRight = new Vector3(0f, 0f, 0f);

            clavicleLeft = new Vector3(0f, 0f, 0f);
            armUpperLeft = new Vector3(0f, 0f, 0f);
            armLowerLeft = new Vector3(0f, 0f, 0f);

            handLeft = new Vector3(0f, 0f, 0f);
            thumbLeft = new Vector3(0f, 0f, 0f);
            fingersLeft = new Vector3(0f, 0f, 0f);

            clavicleRight = new Vector3(0f, 0f, 0f);
            armUpperRight = new Vector3(0f, 0f, 0f);
            armLowerRight = new Vector3(0f, 0f, 0f);

            handRight = new Vector3(0f, 0f, 0f);
            thumbRight = new Vector3(0f, 0f, 0f);
            fingersRight = new Vector3(0f, 0f, 0f);
            headCamera = new Vector3(0f, 0f, 0f);
        }
    }
}
