#region --- LICENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015 Qualisys AB

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using OpenTK;
using System.Collections.Generic;
using System.Linq;

namespace QualisysRealTime.Unity.Skeleton
{
    class MarkersPreprocessor
    {
        private Dictionary<string, Vector3> markers = new Dictionary<string, Vector3>();
        private Dictionary<string, Vector3> markersLastFrame = new Dictionary<string, Vector3>();
        private string prefix;
        private bool sacrumBetween = false;
        private bool frontHeadBetween = false;

        private string[] sacrumBetweenMarkers;
        private string[] frontHeadBetweenMarkers;
        /// <summary>
        /// The ma
        /// </summary>
        private Vector3
            lastSACRUMknown = Vector3Helper.MidPoint(new Vector3(0.0774f, 1.0190f, -0.1151f), new Vector3(-0.0716f, 1.0190f, -0.1138f)),
            lastRIASknown = new Vector3(0.0925f, 0.9983f, 0.1052f),
            lastLIASknown = new Vector3(-0.0887f, 1.0021f, 0.1112f);

        private MarkersNames m;
        /// <summary>
        /// Constructor sets the markers name in the MarkesName class used for joint localization
        /// </summary>
        /// <param name="labelMarkers">The list of labelmarkets</param>
        /// <param name="markerNames">A reference to the markers names</param>
        /// <param name="bodyPrefix">Any possible prefix of the markersname</param>
        public MarkersPreprocessor(List<LabeledMarker> labelMarkers, out MarkersNames markerNames, string bodyPrefix = "")
        {
            this.prefix = bodyPrefix;
            markers = new Dictionary<string, Vector3>();
            //Converting markers
            for (int i = 0; i < labelMarkers.Count; i++)
            {
                markers.Add(labelMarkers[i].Label, labelMarkers[i].Position.Convert());
            }
            //Finding the markers aliases
            markerNames  = NameSet(markers.Keys);
            //foreach (var n in markerNames) UnityEngine.Debug.Log(n);
            m = markerNames;
            // adding non existing markers
            markersLastFrame = new Dictionary<string, Vector3>();
            foreach (var mark in m)
            {
                markersLastFrame.Add(mark, Vector3Helper.NaN);
                if (!markers.ContainsKey(mark))
                {
                    markers.Add(mark, Vector3Helper.NaN);
                }
            }
            // setting arbitrary hip makres positions
            markersLastFrame[m.bodyBase] = lastSACRUMknown;
            markersLastFrame[m.leftHip] = lastLIASknown;
            markersLastFrame[m.rightHip] = lastRIASknown;
        }
        /// <summary>
        /// Prepare the markerset for Joint localization, predicts the 
        /// </summary>
        /// <param name="labelMarkers">The list of labelmarkets</param>
        /// <param name="newMarkers">a reference to the dictionary to be </param>
        /// <param name="prefix">The possible prefix of all markers</param>
        public void ProcessMarkers(List<LabeledMarker> labelMarkers, out Dictionary<string,Vector3> newMarkers, string prefix)
        {
            var temp = markers;
            markers = markersLastFrame;
            markersLastFrame = temp;
            markers.Clear();
            for (int i = 0; i < labelMarkers.Count; i++)
            {
                markers.Add(labelMarkers[i].Label, labelMarkers[i].Position.Convert());
            }

            foreach (var markername in m)
            {
                if (!markers.ContainsKey(markername))
                {
                    markers.Add(markername, Vector3Helper.NaN);
                }
            }
            // sacrum can be defined by two markers
            if (sacrumBetween)
            {
                markers[m.bodyBase] = 
                    Vector3Helper.MidPoint(markers[sacrumBetweenMarkers[0]],
                                            markers[sacrumBetweenMarkers[1]]);
            }
            // 
            if (frontHeadBetween)
            {
                markers[m.head] = 
                        Vector3Helper.MidPoint(markers[frontHeadBetweenMarkers[0]],
                                            markers[frontHeadBetweenMarkers[1]]);
            }
            if (markers[m.leftHip].IsNaN()
                || markers[m.rightHip].IsNaN()
                || markers[m.bodyBase].IsNaN())
            {
                MissingEssientialMarkers(markers);
            }
            else
            {
                lastSACRUMknown = markers[m.bodyBase];
                lastRIASknown = markers[m.rightHip];
                lastLIASknown = markers[m.leftHip];
            }
            newMarkers = markers;
        }
        /// <summary>
        /// If any of the hip markers are missing, we predict them using the last position
        /// </summary>
        /// <param name="markers">The dictionary of markers</param>
        private void MissingEssientialMarkers(Dictionary<string,Vector3> markers)
        {
            Vector3 dirVec1, dirVec2, possiblePos1, possiblePos2,
                    sacrumLastFrame = lastSACRUMknown,
                    liasLastFrame   = lastLIASknown,
                    riasLastFrame   = lastRIASknown;

            Vector3
                Sacrum = markers[m.bodyBase],
                RIAS = markers[m.rightHip],
                LIAS = markers[m.leftHip];
            bool s = !Sacrum.IsNaN(),
                 r = !RIAS.IsNaN(),
                 l = !LIAS.IsNaN();
            if (s) // sacrum exists
            {
                if (r) // sacrum and rias exist, lias missing
                {
                    dirVec1 = liasLastFrame - sacrumLastFrame; // vector from sacrum too lias in last frame
                    dirVec2 = liasLastFrame - riasLastFrame;
                    Quaternion between = Quaternion.Invert(
                                QuaternionHelper2.GetRotationBetween(
                                (RIAS - Sacrum), (riasLastFrame - sacrumLastFrame))
                                );
                    Vector3 transVec1 = Vector3.Transform(dirVec1, (between));
                    Vector3 transVec2 = Vector3.Transform(dirVec2, (between));
                    possiblePos1 = Sacrum + transVec1; // add vector from sacrum too lias last frame to this frames' sacrum
                    possiblePos2 = RIAS + transVec2;
                    markers[m.leftHip] = DontMovedToMuch(markersLastFrame[m.leftHip], Vector3Helper.MidPoint(possiblePos1, possiblePos2)); // get mid point of possible positions

                }
                else if (l) // sacrum  and lias exists, rias missing
                {
                    dirVec1 = riasLastFrame - sacrumLastFrame;
                    dirVec2 = riasLastFrame - liasLastFrame;
                    Quaternion between = Quaternion.Invert(
                                            QuaternionHelper2.GetRotationBetween(
                                            (LIAS - Sacrum), (liasLastFrame - sacrumLastFrame))
                                            );
                    Vector3 transVec1 = Vector3.Transform(dirVec1, (between));
                    Vector3 transVec2 = Vector3.Transform(dirVec2, (between));
                    possiblePos1 = Sacrum + transVec1;
                    possiblePos2 = LIAS + transVec2;
                    markers[m.rightHip] = DontMovedToMuch(markersLastFrame[m.rightHip], Vector3Helper.MidPoint(possiblePos1, possiblePos2));
                }
                else // only sacrum exists, lias and rias missing
                {
                    markers[m.rightHip] = DontMovedToMuch(markersLastFrame[m.rightHip] , Sacrum + riasLastFrame - sacrumLastFrame);
                    markers[m.leftHip] = DontMovedToMuch(markersLastFrame[m.leftHip], Sacrum + liasLastFrame - sacrumLastFrame);
                }
            }
            else if (r) // rias exists, sacrum missing
            {
                if (l) // rias and ias exists, sacrum missing
                {
                    dirVec1 = sacrumLastFrame - riasLastFrame;
                    dirVec2 = sacrumLastFrame - liasLastFrame;

                    Quaternion between = Quaternion.Invert(
                        QuaternionHelper2.GetRotationBetween(
                        (LIAS - RIAS), (liasLastFrame - riasLastFrame))
                        );
                    Vector3 transVec1 = Vector3.Transform(dirVec1, (between));
                    Vector3 transVec2 = Vector3.Transform(dirVec2, (between));
                    possiblePos1 = RIAS + transVec1;
                    possiblePos2 = LIAS + transVec2;
                    markers[m.bodyBase] = DontMovedToMuch(markersLastFrame[m.bodyBase] ,Vector3Helper.MidPoint(possiblePos1,possiblePos2));
                }
                else // only rias exists, lias and sacrum missing
                {
                    markers[m.bodyBase] = DontMovedToMuch(markersLastFrame[m.bodyBase], RIAS + sacrumLastFrame - riasLastFrame);
                    markers[m.leftHip] = DontMovedToMuch(markersLastFrame[m.leftHip], RIAS + liasLastFrame - riasLastFrame);
                }
            }
            else if (l) // only lias exists, rias and sacrum missing
            {
                markers[m.bodyBase] = DontMovedToMuch(markersLastFrame[m.bodyBase], LIAS + sacrumLastFrame - liasLastFrame);
                markers[m.rightHip] = DontMovedToMuch(markersLastFrame[m.rightHip], LIAS + riasLastFrame - liasLastFrame);
            }
            else // all markers missing
            {
                markers[m.rightHip] = markersLastFrame[m.rightHip];
                markers[m.leftHip] = markersLastFrame[m.leftHip];
                markers[m.bodyBase] = markersLastFrame[m.bodyBase];
            }
        }

        /// <summary>
        /// Finds aliases of different markers and replaces the names
        /// </summary>
        /// <param name="markersNames">A collection of the names of the markers</param>
        /// <returns>The set marker names</returns>
        private MarkersNames NameSet(ICollection<string> markersNames)
        {
            MarkersNames m = new MarkersNames();
            #region hip
            var quary = MarkerNames.bodyBaseAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            if (quary == null)
            {
                var q2 = MarkerNames.bodyBasebetween.FirstOrDefault(n => markersNames.Contains(prefix + n[0]) && markersNames.Contains(prefix + n[1]));
                if (q2 != null)
                {
                    sacrumBetween = true;
                    sacrumBetweenMarkers = new string[2];
                    sacrumBetweenMarkers[0] = prefix + q2[0];
                    sacrumBetweenMarkers[1] = prefix + q2[1];

                }
            }
            else
            {
                m.bodyBase = prefix + quary;
            }
            SetName(markersNames, MarkerNames.leftHipAKA, ref m.leftHip, prefix);
            SetName(markersNames, MarkerNames.rightHipAKA, ref m.rightHip, prefix);
            #endregion

            #region upperbody
            SetName(markersNames, MarkerNames.spineAKA, ref m.spine, prefix);
            SetName(markersNames, MarkerNames.neckAKA, ref m.neck, prefix);
            SetName(markersNames, MarkerNames.chestAKA, ref m.chest, prefix);
            SetName(markersNames, MarkerNames.leftShoulderAKA, ref m.leftShoulder, prefix);
            SetName(markersNames, MarkerNames.rightShoulderAKA, ref m.rightShoulder, prefix);
            #endregion

            #region head
            quary = MarkerNames.headAKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            if (quary == null)
            {
                var q2 = MarkerNames.headBetween.FirstOrDefault(n => markersNames.Contains(prefix + n[0]) && markersNames.Contains(prefix + n[1]));
                if (q2 != null)
                {
                    frontHeadBetween = true;
                    frontHeadBetweenMarkers = new string[2];;
                    frontHeadBetweenMarkers[0] = prefix + q2[0];
                    frontHeadBetweenMarkers[1] = prefix + q2[1];
                }
            } else m.head = prefix + quary;

            SetName(markersNames, MarkerNames.leftHeadAKA, ref m.leftHead, prefix);

            SetName(markersNames, MarkerNames.rightHeadAKA, ref m.rightHead, prefix);
            #endregion

            #region legs

            SetName(markersNames, MarkerNames.leftUpperKneeAKA, ref m.leftUpperKnee, prefix);
            SetName(markersNames, MarkerNames.rightUpperKneeAKA, ref m.rightUpperKnee, prefix);
            SetName(markersNames, MarkerNames.leftOuterKneeAKA, ref m.leftOuterKnee, prefix);
            SetName(markersNames, MarkerNames.rightOuterKneeAKA, ref m.rightOuterKnee, prefix);
            SetName(markersNames, MarkerNames.leftInnerKneeAKA, ref m.leftInnerKnee, prefix);
            SetName(markersNames, MarkerNames.rightInnerKneeAKA, ref m.rightInnerKnee, prefix);
            SetName(markersNames, MarkerNames.leftLowerKneeAKA, ref m.leftLowerKnee, prefix);
            SetName(markersNames, MarkerNames.rightLowerKneeAKA, ref m.rightLowerKnee, prefix);
            #endregion

            #region foot
            SetName(markersNames, MarkerNames.leftOuterAnkleAKA, ref m.leftOuterAnkle, prefix);
            SetName(markersNames, MarkerNames.rightOuterAnkleAKA, ref m.rightOuterAnkle, prefix);
            SetName(markersNames, MarkerNames.rightInnerAnkleAKA, ref m.rightInnerAnkle, prefix);
            SetName(markersNames, MarkerNames.leftInnerAnkleAKA, ref m.leftInnerAnkle, prefix);

            SetName(markersNames, MarkerNames.leftHeelAKA, ref m.leftHeel, prefix);
            SetName(markersNames, MarkerNames.rightHeelAKA, ref m.rightHeel, prefix);

            quary = MarkerNames.leftToe2AKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            if (quary == null)
            {
                quary = MarkerNames.leftToe1AKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            }
            m.leftToe2 = ((quary == null) ? m.leftToe2 :prefix + quary);

            quary = MarkerNames.rightToe2AKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            if (quary == null)
            {
                quary = MarkerNames.rightToe1AKA.FirstOrDefault(n => markersNames.Contains(prefix + n));
            }
            m.rightToe2 = ((quary == null) ? m.rightToe2 :prefix + quary);
            #endregion

            #region arms
            SetName(markersNames, MarkerNames.leftElbowAKA, ref m.leftElbow, prefix);
            SetName(markersNames, MarkerNames.rightElbowAKA, ref m.rightElbow, prefix);
            SetName(markersNames, MarkerNames.leftInnerElbowAKA, ref m.leftInnerElbow, prefix);
            SetName(markersNames, MarkerNames.rightInnerElbowAKA, ref m.rightInnerElbow, prefix);
            SetName(markersNames, MarkerNames.leftOuterElbowAKA, ref m.leftOuterElbow, prefix);
            SetName(markersNames, MarkerNames.rightOuterElbowAKA, ref m.rightOuterElbow, prefix);
            SetName(markersNames, MarkerNames.leftWristAKA, ref m.leftWrist, prefix);
            SetName(markersNames, MarkerNames.rightWristAKA, ref m.rightWrist, prefix);
            SetName(markersNames, MarkerNames.leftWristRadiusAKA, ref m.leftWristRadius, prefix);
            SetName(markersNames, MarkerNames.rightWristRadiusAKA, ref m.rightWristRadius, prefix);
            #endregion

            #region hands
            SetName(markersNames, MarkerNames.leftHandAKA, ref m.leftHand, prefix);
            SetName(markersNames, MarkerNames.rightHandAKA, ref m.rightHand, prefix);
            SetName(markersNames, MarkerNames.rightIndexAKA, ref m.rightIndex, prefix);
            SetName(markersNames, MarkerNames.leftIndexAKA, ref m.leftIndex, prefix);
            SetName(markersNames, MarkerNames.rightThumbAKA, ref m.rightThumb, prefix);
            SetName(markersNames, MarkerNames.leftThumbAKA, ref m.leftThumb, prefix);
            #endregion
            return m;
        }
        /// <summary>
        /// Search for an alias of a marker and set it to that name
        /// </summary>
        /// <param name="markerNames"></param>
        /// <param name="alias"></param>
        /// <param name="name"></param>
        /// <param name="prefix"></param>
        private void SetName(ICollection<string> markerNames, List<string> alias, ref string name, string prefix = "")
        {
            var quary = alias.FirstOrDefault(n => markerNames.Contains(prefix + n));
            name = ((quary == null) ? name : prefix + quary);
        }
        /// <summary>
        /// Makes sure a makers cant move to fast when predicting the hipmarkers
        /// </summary>
        /// <param name="from">The position last frame</param>
        /// <param name="to">The position this frame</param>
        /// <returns>this frame if moved by less then 0.02m, 0.02m otherwise</returns>
        private Vector3 DontMovedToMuch(Vector3 from, Vector3 to)
        {
            Vector3 move = (to - from);
            if (move.Length > 0.02f)
            {
                move.NormalizeFast();
                move *= 0.02f;
                return from + move;
            }
            return to;
        }
    }
}