#region --- LICENSE ---
/*
    The MIT License (MIT)

    Copyright (c) 2015-2018 Qualisys AB

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using OpenTK;

namespace QualisysRealTime.Unity.Skeleton
{
	public class SegmentTracking
	{
		private BipedSkeleton skeleton;
		private MarkersNames markerNames;
		private Dictionary<string, Vector3> markers = new Dictionary<string, Vector3>();
		private Dictionary<Joint, Dictionary<string, Vector3>> segments = new Dictionary<Joint, Dictionary<string, Vector3>>();

		private readonly string ORIGIN = "ORIGIN";
        //private readonly string DEBUG_MARKER = "L_HME";

		/// <summary>
		/// Initializes a new instance of the <see cref="QualisysRealTime.Unity.Skeleton.SegmentTracking"/> class.
		/// </summary>
		/// <param name="s">S.</param>
		/// <param name="mn">Mn.</param>
		/// <param name="lm">Lm.</param>
		public SegmentTracking(BipedSkeleton s, MarkersNames mn, List<Marker> lm)
		{
			this.skeleton = s;
			this.markerNames = mn;

			// Converting markers
			this.markers = new Dictionary<string, Vector3>();
			for (int i = 0; i < lm.Count; i++)
			{
				this.markers.Add(lm[i].Label, lm[i].Position);
			}

			// Add the tracking markers for the joints / segments
			string[] labels;

            // Hips
            labels = new string[] {
                markerNames.bodyBase,
                markerNames.leftHip,
                markerNames.rightHip,
                "L_PSIS", // Additional Tracking Marker
                "R_PSIS"  // Additional Tracking Marker
			};
            this.addTrackingMarkers(Joint.PELVIS, labels);

            // Chest
            labels = new string[] {
                markerNames.neck,
                markerNames.spine,
                markerNames.chest,
                "STRN" // Additional Tracking Marker
			};
            this.addTrackingMarkers(Joint.SPINE3, labels);

            // Left Upper Arm
            labels = new string[] {
				markerNames.leftElbow, 
				markerNames.leftInnerElbow, 
				markerNames.leftOuterElbow,
				"L_UPA" // Additional Tracking Marker
			};
			this.addTrackingMarkers(Joint.SHOULDER_L, labels);

			// Right Upper Arm
			labels = new string[] {
				markerNames.rightElbow, 
				markerNames.rightInnerElbow, 
				markerNames.rightOuterElbow,
				"R_UPA" // Additional Tracking Marker
			};
			this.addTrackingMarkers(Joint.SHOULDER_R, labels);

			// Left Lower Arm
			labels = new string[] {
				markerNames.leftWrist, 
				markerNames.leftWristRadius,
				"L_FRA" // Additional Tracking Marker
			};
			this.addTrackingMarkers(Joint.ELBOW_L, labels);

			// Right Lower Arm
			labels = new string[] {
				markerNames.rightWrist, 
				markerNames.rightWristRadius,
				"R_FRA" // Additional Tracking Marker
			};
			this.addTrackingMarkers(Joint.ELBOW_R, labels);

			// Left Thigh
			labels = new string[] {
				markerNames.leftUpperKnee, 
				markerNames.leftInnerKnee,
				markerNames.leftOuterKnee,
				"L_THI" // Additional Tracking Marker
			};
			this.addTrackingMarkers(Joint.HIP_L, labels);

			// Right Thigh
			labels = new string[] {
				markerNames.rightUpperKnee, 
				markerNames.rightInnerKnee,
				markerNames.rightOuterKnee,
				"R_THI" // Additional Tracking Marker
			};
			this.addTrackingMarkers(Joint.HIP_R, labels);

			// Left Shank
			labels = new string[] {
				markerNames.leftLowerKnee,
				markerNames.leftInnerAnkle,
				markerNames.leftOuterAnkle,
				"L_TIB" // Additional Tracking Marker
			};
			this.addTrackingMarkers(Joint.KNEE_L, labels);

			// Right Shank
			labels = new string[] {
				markerNames.rightLowerKnee,
				markerNames.rightInnerAnkle,
				markerNames.rightOuterAnkle,
				"R_TIB" // Additional Tracking Marker
			};
			this.addTrackingMarkers(Joint.KNEE_R, labels);
		}

		/// <summary>
		/// Processes the markers.
		/// </summary>
		/// <returns><c>true</c>, if markers was processed, <c>false</c> otherwise.</returns>
		/// <param name="s">S.</param>
		/// <param name="lm">Lm.</param>
		/// <param name="nm">Nm.</param>
		/// <param name="prefix">Prefix.</param>
		public bool ProcessMarkers(BipedSkeleton s, List<Marker> lm, ref Dictionary<string, Vector3> nm, string prefix)
		{
			this.skeleton = s;

			// Convert labeled markers to marker dictionary
			this.markers.Clear();
			for (int i = 0; i < lm.Count; i++)
			{
				this.markers.Add(lm[i].Label, lm[i].Position);
			}

			var virtualMarkersCreated = false;

			foreach (var segment in this.segments.Keys) 
			{
                // Check for unavailable markers in tracked segments
				if (this.segmentIsTracked(segment)) 
				{
					var availableMarkers   = new List<string>();
					var unavailableMarkers = new List<string>();

					// Check if one of the tracking markers is not available
					foreach (var label in this.segments[segment].Keys) 
					{
						if (label == ORIGIN) continue; // No origin markers
						if (
							this.markers[label].IsNaN()
							// DEBUG
							//|| label == DEBUG_MARKER
                        ) 
							unavailableMarkers.Add(label);
						else 
							availableMarkers.Add(label);
					}

					// Marker is not available, but there are enough tracking markers left
					if (unavailableMarkers.Count > 0 && availableMarkers.Count >= 2)
                    {
                        //UnityEngine.Debug.Log("real: " + this.markers[DEBUG_MARKER]);
						//var length1 = this.markers[DEBUG_MARKER].Length;
						createVirtualMarkers(segment, availableMarkers, unavailableMarkers);
						//UnityEngine.Debug.Log("virtual: " + this.markers[DEBUG_MARKER]);
						//var length2 = this.markers[DEBUG_MARKER].Length;
						//UnityEngine.Debug.Log("DISTANCE real to virtual is " + (length1-length2) );
						virtualMarkersCreated = true;
					}
                    else if (unavailableMarkers.Count == 0)
                    {
                        // Update segment tracking markers everytime all markers
                        // are available to increase accuracy
                        updateTrackingMarkers(segment);
                    }

					// Update markers
					foreach (var label in unavailableMarkers) 
					{
						nm[label] = this.markers[label];	
					}
				} 
				else 
				{
                    // Update segment tracking markers
                    updateTrackingMarkers(segment);					
				}
			}

			// Return if virtual markers were created
			return virtualMarkersCreated;
		}

		/// <summary>
		/// Adds the tracking markers.
		/// </summary>
		/// <param name="segment">Segment.</param>
		/// <param name="labels">Labels.</param>
		private void addTrackingMarkers(Joint segment, string [] labels) 
		{
			var trackingMarkers = new Dictionary<string, Vector3>();
			trackingMarkers.Add(ORIGIN, Vector3Helper.NaN);
			foreach (string label in labels) 
			{
				// Only add marker, if exists
				if (this.markers.ContainsKey(label)) {
					trackingMarkers.Add(label, Vector3Helper.NaN);
				}
			}
			this.segments.Add(segment, trackingMarkers);
		}

        /// <summary>
        /// // Update tracking marker positions
        /// </summary>
        /// <param name="segment"></param>
        private void updateTrackingMarkers(Joint segment)
        {
            List<string> labelList = new List<string>(this.segments[segment].Keys);
            foreach (var label in labelList)
            {
                this.segments[segment][label] = (label == ORIGIN) ? skeleton.Find(segment).Pos : this.markers[label];
            }
        }

        /// <summary>
        /// Segments the is tracked.
        /// </summary>
        /// <returns><c>true</c>, if is tracked was segmented, <c>false</c> otherwise.</returns>
        /// <param name="segment">Segment.</param>
        private bool segmentIsTracked(Joint segment)
		{
			Dictionary<string, Vector3> trackingMarkers = this.segments[segment];

			// A minimum of three valid markers + origin is necessary 
			if (trackingMarkers.Count <= 3) return false;

			// Check if all tracking markers are valid
			foreach (string label in trackingMarkers.Keys) 
			{
				if (trackingMarkers[label].IsNaN() || trackingMarkers[label].Length == 0) 
					return false;
			}
			return true;
		}

		/// <summary>
		/// Creates the virtual markers.
		/// </summary>
		/// <param name="segment">Segment.</param>
		/// <param name="availableMarkers">Available markers.</param>
		/// <param name="unavailableMarkers">Unavailable markers.</param>
		private void createVirtualMarkers(Joint segment, List<string> availableMarkers, List<string> unavailableMarkers)
		{
			foreach(var um in unavailableMarkers) 
			{
				createVirtualMarker(segment, availableMarkers, um);
			}
		}

        /// <summary>
        /// Creates the virtual marker.
        /// </summary>
        /// <param name="segment">Segment.</param>
        /// <param name="availableMarkers">Available markers.</param>
        /// <param name="unavailableMarker">Unavailable marker.</param>
        private void createVirtualMarker(Joint segment, List<string> availableMarkers, string unavailableMarker)
        {
            if (DEBUG.enabled) UnityEngine.Debug.Log("Create virtual marker " + unavailableMarker + " for segment " + segment);

            //UnityEngine.Debug.Log("availableMarkers[0] " + availableMarkers[0] + " availableMarkers[1] " + availableMarkers[1]);
            //UnityEngine.Debug.Log("unavailableMarker " + unavailableMarker);

            var trackingMarkers = this.segments[segment];

            Vector3 tcs, vAxis, xAxis, yAxis, zAxis, locPos, vPos;

            // Create world to local matrix
            tcs = (availableMarkers.Count > 2) ? trackingMarkers[availableMarkers[2]] : trackingMarkers[ORIGIN];
            xAxis = (tcs - trackingMarkers[availableMarkers[0]]).Normalized();
            vAxis = (trackingMarkers[availableMarkers[0]] - trackingMarkers[availableMarkers[1]]).Normalized();
            yAxis = Vector3.Cross(xAxis, vAxis);
            zAxis = Vector3.Cross(yAxis, xAxis);

            var worldToLocal = Matrix4x4Helper.GetMatrix(tcs.Convert(), zAxis.Convert(), yAxis.Convert(), xAxis.Convert()).inverse;
            locPos = worldToLocal.MultiplyPoint3x4(trackingMarkers[unavailableMarker].Convert()).Convert();
            
            //UnityEngine.Debug.Log("locPos " + locPos);

            // Create local to world matrix
            tcs = (availableMarkers.Count > 2) ? this.markers[availableMarkers[2]] : this.skeleton.Find(segment).Pos;
            xAxis = (tcs - this.markers[availableMarkers[0]]).Normalized();
            vAxis = (this.markers[availableMarkers[0]] - this.markers[availableMarkers[1]]).Normalized();
            yAxis = Vector3.Cross(xAxis, vAxis);
            zAxis = Vector3.Cross(yAxis, xAxis);

            var localToWorld = Matrix4x4Helper.GetMatrix(tcs.Convert(), zAxis.Convert(), yAxis.Convert(), xAxis.Convert());
            vPos = localToWorld.MultiplyPoint3x4(locPos.Convert()).Convert();

            // Set new marker position
            this.markers[unavailableMarker] = vPos;
        }
    }
}