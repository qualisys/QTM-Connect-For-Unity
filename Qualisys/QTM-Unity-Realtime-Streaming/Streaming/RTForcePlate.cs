// Unity SDK for Qualisys Track Manager. Copyright 2015-2022 Qualisys AB
//
using UnityEditor;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace QualisysRealTime.Unity
{
    class RTForcePlate : MonoBehaviour
    {
        public string ForcePlateName = "ForcePlateName";
        private ForceVector forceVectorCached;
        void Start()
        {
            RTClient.GetInstance().StartConnecting("127.0.0.1", -1, false, true, false, false, false, false, true);
        }

        void Update()
        {
            var rtClient = RTClient.GetInstance();
            var forceVector = rtClient.GetForceVector(ForcePlateName);
            if (forceVector != null) 
            {
                forceVectorCached = forceVector;
            }
        }

        private void OnDrawGizmos()
        {
            if(forceVectorCached != null)
            { 
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine( forceVectorCached.ApplicationPoint, forceVectorCached.Force );

                Vector3 zero = forceVectorCached.Transform.MultiplyPoint(Vector3.zero);
                Vector3 right = forceVectorCached.Transform.MultiplyPoint(Vector3.right);
                Vector3 up = forceVectorCached.Transform.MultiplyPoint(Vector3.up);
                Vector3 forward = forceVectorCached.Transform.MultiplyPoint(Vector3.forward);
                
                Gizmos.color = Color.red;
                Gizmos.DrawSphere( forceVectorCached.ApplicationPoint, 0.01f );

                Gizmos.DrawLine(forceVectorCached.Corners[0],forceVectorCached.Corners[1]);
                Gizmos.DrawLine(forceVectorCached.Corners[1],forceVectorCached.Corners[2]);
                Gizmos.DrawLine(forceVectorCached.Corners[2],forceVectorCached.Corners[3]);
                Gizmos.DrawLine(forceVectorCached.Corners[3],forceVectorCached.Corners[0]);

                int x = 0;
                foreach( var corner in forceVectorCached.Corners )
                { 
                    Gizmos.DrawSphere( corner, 0.01f );
                    // Handles.Label( corner, x.ToString()); // Draw corner numbers
                    x++;
                }
            }
        }
    }
}