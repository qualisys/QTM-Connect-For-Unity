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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QualisysRealTime.Unity.Skeleton
{
    static class UnityDebug
    {
        /// <summary>
        /// Draws the twist constraints accoriding in Unity using Giszmos
        /// </summary>
        /// <param name="b">The bone with its constraints</param>
        /// <param name="refBone">The to be twisted against</param>
        /// <param name="poss">The position of where it should be drawn</param>
        /// <param name="scale">The scale of the constraints</param>
        public static void DrawTwistConstraints(Bone b, Bone refBone, OpenTK.Vector3 poss, float scale)
        {
            if (b.Orientation.Xyz.IsNaN() || refBone.Orientation.Xyz.IsNaN())
            {
                return;
            }
            OpenTK.Vector3 thisY = b.GetYAxis();

            OpenTK.Quaternion referenceRotation = refBone.Orientation * b.ParentPointer;
            OpenTK.Vector3 parentY = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, referenceRotation);
            OpenTK.Vector3 parentZ = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitZ, referenceRotation);

            OpenTK.Quaternion rot = QuaternionHelper2.GetRotationBetween(parentY, thisY);
            OpenTK.Vector3 reference = OpenTK.Vector3.Transform(parentZ, rot);
            reference.Normalize();
            Debug.DrawRay(poss.Convert(), (b.GetZAxis() * scale*2).Convert(), Color.cyan);


            float startTwistLimit = OpenTK.MathHelper.DegreesToRadians(b.StartTwistLimit);
            OpenTK.Vector3 m = OpenTK.Vector3.Transform(reference, OpenTK.Quaternion.FromAxisAngle(thisY, startTwistLimit));
            m.Normalize();
            Debug.DrawRay(poss.Convert(), m.Convert() * scale, Color.yellow);

            float endTwistLimit = OpenTK.MathHelper.DegreesToRadians(b.EndTwistLimit);
            OpenTK.Vector3 m2 = OpenTK.Vector3.Transform(reference, OpenTK.Quaternion.FromAxisAngle(thisY, endTwistLimit));
            m2.Normalize();
            Debug.DrawRay(poss.Convert(), m2.Convert() * scale, Color.magenta);

            Debug.DrawLine((poss + (m*scale)).Convert(), (poss + (m2*scale)).Convert(), Color.cyan);
        }
        public static void DrawTwistConstraints(Bone b, Bone refBone, OpenTK.Vector3 poss)
        {
            DrawTwistConstraints(b, refBone, poss, 0.1f);
        }
        /// <summary>
        /// Using Unity Debug, draws the x,y,z axis of a Quaternion as x red, y green and z blue
        /// </summary>
        /// <param name="rot">The OpenTK Quaterion</param>
        /// <param name="pos">The UnityEngine Vector3 position to be drawn</param>
        /// <param name="scale">The float length of the axis in meter</param>
        public static void DrawRays(OpenTK.Quaternion rot, Vector3 pos, float scale)
        {
            OpenTK.Vector3 right = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitX, rot);
            OpenTK.Vector3 up = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, rot);
            OpenTK.Vector3 forward = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitZ, rot);
            Debug.DrawRay(pos, up.Convert() * scale, Color.green);
            Debug.DrawRay(pos, right.Convert() * scale, Color.red);
            Debug.DrawRay(pos, forward.Convert() * scale, Color.blue);
        }
        /// <summary>
        /// Using Unity Debug, draws the x,y,z axis of a Quaternion as x red, y green and z blue
        /// </summary>
        /// <param name="rot">The OpenTK Quaterion</param>
        /// <param name="pos">The OpenTK Vector3 position to be drawn</param>
        /// <param name="scale">The float length of the axis in meters</param>
        public static void DrawRays(OpenTK.Quaternion rot, OpenTK.Vector3 pos, float scale)
        {
            DrawRays(rot, pos.Convert(), scale);
        }
        /// <summary>
        /// Using Unity Debug, draws the x,y,z axis of a Quaternion at 7 cm scale as x red, y green and z blue
        /// </summary>
        /// <param name="rot">The OpenTK Quaterion</param>
        /// <param name="pos">The UnityEngine Vector3 position to be drawn</param>
        public static void DrawRays(OpenTK.Quaternion rot, Vector3 pos)
        {
            DrawRays(rot, pos, 0.07f);
        }
        /// <summary>
        /// Using Unity Debug, draws the x,y,z axis of a Quaternion at 7 cm scale as x red, y green and z blue
        /// </summary>
        /// <param name="rot">The OpenTK Quaterion</param>
        /// <param name="pos">The OpenTK Vector3 position to be drawn</param>
        public static void DrawRays(OpenTK.Quaternion rot, OpenTK.Vector3 pos)
        {
            DrawRays(rot, pos.Convert());
        }
        /// <summary>
        /// Using Unity Debug, draws the x,y,z axis of a Quaternion as x magenta, y yellow and z cyan
        /// </summary>
        /// <param name="rot">The OpenTK Quaterion</param>
        /// <param name="pos">The UnityEngine Vector3 position to be drawn</param>
        /// <param name="scale">The float length of the axis in meter</param>
        public static void DrawRays2(Quaternion rot, Vector3 pos, float scale)
        {
            OpenTK.Vector3 right = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitX, rot.Convert());
            OpenTK.Vector3 up = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitY, rot.Convert());
            OpenTK.Vector3 forward = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitZ, rot.Convert());
            Debug.DrawRay(pos, right.Convert() * scale, Color.magenta);
            Debug.DrawRay(pos, up.Convert() * scale, Color.yellow);
            Debug.DrawRay(pos, forward.Convert() * scale, Color.cyan);
        }
        /// <summary>
        /// Using Unity Debug, draws the x,y,z axis of a Quaternion as x magenta, y yellow and z cyan
        /// </summary>
        /// <param name="rot">The OpenTK Quaterion</param>
        /// <param name="pos">The OpenTK Vector3 position to be drawn</param>
        /// <param name="scale">The float length of the axis in meter</param>
        public static void DrawRays2(OpenTK.Quaternion rot, OpenTK.Vector3 pos, float scale)
        {
            DrawRays2(rot.Convert(), pos.Convert(), scale);
        }
        /// <summary>
        /// Using Unity Debug, draws the x,y,z axis of a Quaternion as x magenta, y yellow and z cyan
        /// </summary>
        /// <param name="rot">The OpenTK Quaterion</param>
        /// <param name="pos">The OpenTK Vector3 position to be drawn</param>
        public static void DrawRays2(OpenTK.Quaternion rot, OpenTK.Vector3 pos)
        {
            DrawRays2(rot.Convert(), pos.Convert(), 0.07f);
        }
        /// <summary>
        /// Draws an array in unity using Unity.Debug
        /// </summary>
        /// <param name="pos">The staring position of the OpenTK Vector3</param>
        /// <param name="dir">The direction of the vector of the OpenTK Vector3</param>
        /// <param name="c">The color the of the Vector as a UnityEngine Color</param>
        public static void DrawVector(OpenTK.Vector3 pos, OpenTK.Vector3 dir, Color c)
        {
            Debug.DrawRay(pos.Convert(), dir.Convert(), c);
        }
        
        /// <summary>
        /// Draws an array in unity using Unity.Debug in black
        /// </summary>
        /// <param name="pos">The staring position of the OpenTK Vector3</param>
        /// <param name="dir">The direction of the vector of the OpenTK Vector3</param>
        /// <param name="length">The length of the vector</param>
        public static void DrawVector(OpenTK.Vector3 pos, OpenTK.Vector3 dir, float length)
        {
            Debug.DrawRay(pos.Convert(), Vector3.Normalize(dir.Convert()) * length, Color.black);
        }
        /// <summary>
        /// Draws an array in unity using Unity.Debug
        /// </summary>
        /// <param name="pos">The staring position of the OpenTK Vector3</param>
        /// <param name="dir">The direction of the vector of the OpenTK Vector3</param>
        /// <param name="length">The length of the vector</param>
        /// <param name="c">The color of the vector as a UnityEngine Color</param>
        public static void DrawVector(OpenTK.Vector3 pos, OpenTK.Vector3 dir, float size, Color c)
        {
            Debug.DrawRay(pos.Convert(), Vector3.Normalize(dir.Convert()) * size, c);
        }
        /// <summary>
        /// Draws an array in unity using Unity.Debug
        /// </summary>
        /// <param name="pos">The staring position of the OpenTK Vector3</param>
        /// <param name="dir">The direction of the vector of the OpenTK Vector3</param>
        public static void DrawVector(OpenTK.Vector3 pos, OpenTK.Vector3 dir)
        {
            DrawVector(pos, dir, Color.black);
        }
        /// <summary>
        /// Draws a line from two 3d points in Unity
        /// </summary>
        /// <param name="start">The starting point as a OpenTK Vector3</param>
        /// <param name="end">The end point as a OpenTK Vector3</param>
        public static void DrawLine(OpenTK.Vector3 start, OpenTK.Vector3 end)
        {
            Debug.DrawLine(start.Convert(), end.Convert());
        }
        /// <summary>
        /// Draws a line from two 3d points in Unity in a specific color
        /// </summary>
        /// <param name="start">The starting point as a OpenTK Vector3</param>
        /// <param name="end">The end point as a OpenTK Vector3</param>
        /// <param name="c">The color of the line as a UnityEngine Color</param>
        public static void DrawLine(OpenTK.Vector3 start, OpenTK.Vector3 end, Color c)
        {
            Debug.DrawLine(start.Convert(), end.Convert(), c);
        }
        /// <summary>
        /// Draws an ellipse in Unity from two radii floating points, a Vector3 position, and a Quaternion rotation
        /// </summary>
        /// <param name="x">Radia x of the Ellipse</param>
        /// <param name="y">Radia Y of the Ellipse</param>
        /// <param name="pos">The position of the Ellipse</param>
        /// <param name="rot">The orientation of the ellipse</param>
        /// <param name="resolution">The amount of lines the ellipse should be drawn of</param>
        /// <param name="c">The color of the ellipse</param>
        public static void CreateEllipse(float x, float y, Vector3 pos, Quaternion rot, int resolution, Color c)
        {

            Vector3[] positions = new Vector3[resolution + 1];
            for (int i = 0; i <= resolution; i++)
            {
                float angle = (float)i / (float)resolution * 2.0f * Mathf.PI;
                positions[i] = new Vector3(x * Mathf.Cos(angle), 0.0f, y * Mathf.Sin(angle));
                positions[i] = rot * positions[i] + pos;
                if (i > 0)
                {
                    Debug.DrawLine(positions[i], positions[i - 1], c);
                }
            }
            Debug.DrawLine(positions[0], positions[positions.Length-1], c);

        }
        /// <summary>
        /// Draws an irregeular cone
        /// </summary>
        /// <param name="strains">The x,y,z,w radii</param>
        /// <param name="top">The position of the top of the cone</param>
        /// <param name="L1">The direction of the cone</param>
        /// <param name="rot">The rotation of the cone</param>
        /// <param name="resolution">The amount of lines the cone shoud be drawn from</param>
        /// <param name="scale">The height of the cone</param>
        public static void CreateIrregularCone(OpenTK.Vector4 strains, OpenTK.Vector3 top, OpenTK.Vector3 L1, OpenTK.Quaternion rot, int resolution, float scale)
        {
            L1.Normalize();
            List<OpenTK.Vector3> positions = new List<OpenTK.Vector3>();
            positions.AddRange(GetQuarter(strains.X, strains.Y, top, L1, rot, resolution, 1, scale));
            positions.AddRange(GetQuarter(strains.Z, strains.Y, top, L1, rot, resolution, 2, scale));
            positions.AddRange(GetQuarter(strains.Z, strains.W, top, L1, rot, resolution, 3, scale));
            positions.AddRange(GetQuarter(strains.X, strains.W, top, L1, rot, resolution, 4, scale));
            OpenTK.Vector3 prev = positions.First();
            Color c;
            Color c2 = Color.black;
            int i = 0;
            foreach (OpenTK.Vector3 v in positions)
            {
                float part = ((float)i % ((float)resolution / 4f)) / ((float)resolution / 4f);
                if (i < resolution * 0.25)
                {  //Q1
                    c = Color.Lerp(Color.blue, Color.red, part);
                }
                else if (i < resolution * 0.5)
                {   //Q4
                    c = Color.Lerp(Color.red, Color.green, part);
                }
                else if (i < resolution * 0.75)
                {   //Q3
                    c = Color.Lerp(Color.green, Color.yellow, part);
                }
                else
                {   //Q2
                    c = Color.Lerp(Color.yellow, Color.blue, part);
                }
                i++;
                DrawLine(v, prev , c2);
                DrawLine(top, v , c);
                prev = v;
            }

            c = Color.blue;
            DrawLine(prev, positions.First(), c);
            DrawLine(top, positions.First(), c);
            //return positions.ToArray();
        }
        private static List<OpenTK.Vector3> GetQuarter(
            float a, float b, OpenTK.Vector3 top, 
            OpenTK.Vector3 L1,       OpenTK.Quaternion rot, 
            int resolution,         int p, float scale)
        {
            OpenTK.Quaternion extraRot = OpenTK.Quaternion.Identity;
            OpenTK.Vector3 L2 = L1;
            if (a > 90 && b > 90)
            {
                L2 = -L1;
                a = 180 - a;
                b = 180 - b;
            }
            else if ((a > 90) ^ (b > 90))
            {
            #region Crazy cone
                OpenTK.Vector3 right = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitX, rot);
                OpenTK.Vector3 forward = OpenTK.Vector3.Transform(OpenTK.Vector3.UnitZ, rot);
                L2 = right;
                switch (p)
                {
                case 1:
                    if (a > 90) L2 = right;
                    else L2 = forward;
                    break;
                case 2:
                    if (a > 90) L2 = -right;
                    else L2 = forward;
                    break;
                case 3:
                    if (a > 90) L2 = -right;
                    else L2 = -forward;
                    break;
                case 4:
                    if (a > 90) L2 = right;
                    else L2 = -forward;
                    break;
                default:
                    break;
                }
                if (a > 90)
                    a = a - 90;
                else
                    b = b - 90;
                float angle = OpenTK.Vector3.CalculateAngle(L2, L1);
                OpenTK.Vector3 axis = OpenTK.Vector3.Cross(L2, L1);
                extraRot = OpenTK.Quaternion.FromAxisAngle(axis, angle);
                extraRot = OpenTK.Quaternion.Invert(extraRot);
            #endregion
            }

            float A = Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(Math.Min(89.99f, a)));
            float B = Mathf.Tan(OpenTK.MathHelper.DegreesToRadians(Math.Min(89.99f, b)));
            List<OpenTK.Vector3> te = new List<OpenTK.Vector3>();
            float part = resolution * (p / 4f);
            float start = resolution * ((p - 1f) / 4f);
            for (float i = start; i < part; i++)
            {
                float angle = i / resolution * 2.0f * Mathf.PI;
                float x = A * Mathf.Cos(angle);
                float z = B * Mathf.Sin(angle);
                OpenTK.Vector3 t = new OpenTK.Vector3(x, 0.0f, z );
                t = OpenTK.Vector3.Transform(t, extraRot * rot );
                t += L2;
                t.Normalize();
                t *= scale;
                t += top;
                te.Add(t);
            }
            return te;
        }
    }
}