// Unity SDK for Qualisys ack Manager. Copyright 2015-2023 Qualisys AB
//
using UnityEngine;

namespace QualisysRealTime.Unity
{
    static class MatrixExtensions
    {
        public static Vector3 ExtractPosition(this Matrix4x4 matrix)
        {
            Vector3 position;
            position.x = matrix.m03;
            position.y = matrix.m13;
            position.z = matrix.m23;
            return position;
        }
    }


    class RTForcePlate : MonoBehaviour
    {
        public string forcePlateName = "Force-plate 1";

        public LineRenderer forceArrow;
        
        public LineRenderer momentArrow;
        
        public GameObject forcePlateCube;

        private ForceVector forceVectorCached;

        Vector3 VisualDownscaleForce(Vector3 v)
        { 
            // Downscale to look good in the scene
            // Inverted to display above the force plate
            return v / -500.0f;
        }

        Vector3 VisualDownscaleMoment(Vector3 v)
        { 
            // Downscale to look good in the scene
            // Inverted to be compatible with the force
            return v / -100.0f;
        }

        void UpdateArrow( LineRenderer lineRenderer, Vector3 position, Vector3 directionAndMagnitude )
        { 
            Vector3 endPosition = position + directionAndMagnitude;
            Vector3 startPosition = position;
                    
            float headLength = 0.15f;
            float headWidth = 0.1f;
            float stemWidth = headWidth / 4.0f;

            float minLength = headLength;
            float length = Vector3.Distance (startPosition,  endPosition);
                    
            lineRenderer.enabled = length >= minLength;
                    
            if(lineRenderer.enabled)
            {   
                //   .   _1.0
                //  / \
                // /. .\ _breakpoint
                //  | |  
                //  |_|  _0.0

                float breakpoint = headLength / length;
                        
                //Making an arrow using the line renderer.
                //Code adapted from an answer at the Unity Forum.
                //ShawnFeatherly (http://answers.unity.com/answers/1330338/view.html)
                lineRenderer.positionCount = 4;
                lineRenderer.SetPosition (0, startPosition);
                lineRenderer.SetPosition (1, Vector3.Lerp(startPosition,  endPosition, 0.999f - breakpoint));
                lineRenderer.SetPosition (2, Vector3.Lerp (startPosition,  endPosition, 1 - breakpoint));
                lineRenderer.SetPosition (3,  endPosition);
                lineRenderer.widthCurve = new AnimationCurve (
                        new Keyframe (0, stemWidth),
                        new Keyframe (0.999f - breakpoint, stemWidth),
                        new Keyframe (1 - breakpoint, headWidth),
                        new Keyframe (1, 0f));
            }
        }

        void Update()
        {
            forceVectorCached = RTClient.GetInstance().GetForceVector(forcePlateName);
            
            if (forcePlateCube) 
            {
                forcePlateCube.SetActive(forceVectorCached != null);
                if (forceVectorCached != null)
                {
                    // Adjust cube to fit force plate
                    var src = forceVectorCached.Transform;
                    var forcePlateThickness = 0.02f;
                    var destTransform = forcePlateCube.transform;

                    destTransform.localRotation = src.rotation;
                    destTransform.localScale = new Vector3(
                        Vector3.Distance(forceVectorCached.Corners[0], forceVectorCached.Corners[1]),
                        Vector3.Distance(forceVectorCached.Corners[1], forceVectorCached.Corners[2]),
                        forcePlateThickness
                    );

                    destTransform.position = src.ExtractPosition() - destTransform.forward * (forcePlateThickness / 2.0f);
                }
            }
            
            if (forceArrow) 
            {
                forceArrow.gameObject.SetActive(forceVectorCached != null);
                if (forceVectorCached != null)
                {
                    UpdateArrow(forceArrow, forceVectorCached.ApplicationPoint, VisualDownscaleForce(forceVectorCached.Force));
                }
            }

            if (momentArrow) 
            {
                momentArrow.gameObject.SetActive(forceVectorCached != null);
                if (forceVectorCached != null)
                {
                    UpdateArrow(momentArrow, forceVectorCached.ApplicationPoint, VisualDownscaleMoment(forceVectorCached.Moment));
                }
            }
        }

        private void OnDrawGizmos()
        {
            if(forceVectorCached != null)
            { 
                Vector3 zero = forceVectorCached.Transform.MultiplyPoint(Vector3.zero);
                Vector3 right = forceVectorCached.Transform.MultiplyPoint(Vector3.right);
                Vector3 up = forceVectorCached.Transform.MultiplyPoint(Vector3.up);
                Vector3 forward = forceVectorCached.Transform.MultiplyPoint(Vector3.forward);
                
                Gizmos.color = Color.green;
                Gizmos.DrawLine(zero, up);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(zero, right);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(zero, forward);

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(forceVectorCached.ApplicationPoint, forceVectorCached.ApplicationPoint + VisualDownscaleForce(forceVectorCached.Force));
                Gizmos.DrawSphere(forceVectorCached.ApplicationPoint, 0.01f);
                
                Gizmos.color = Color.red;
                Gizmos.DrawLine(forceVectorCached.Corners[0],forceVectorCached.Corners[1]);
                Gizmos.DrawLine(forceVectorCached.Corners[1],forceVectorCached.Corners[2]);
                Gizmos.DrawLine(forceVectorCached.Corners[2],forceVectorCached.Corners[3]);
                Gizmos.DrawLine(forceVectorCached.Corners[3],forceVectorCached.Corners[0]);

                int i = 1;
                foreach( var corner in forceVectorCached.Corners )
                { 
                    #if UNITY_EDITOR
                    UnityEditor.Handles.Label( corner, (i++).ToString() );
                    #endif
                    Gizmos.DrawSphere( corner, 0.01f );
                }

            }
        }
    }
}
