// Unity SDK for Qualisys Track Manager. Copyright 2015 Qualisys AB
//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using QTMRealTimeSDK;

namespace QualisysRealTime.Unity
{
    /// Stream bones from QTM
    public class RTBones : MonoBehaviour
    {
        public bool visibleBones = true;

        // Use this for initialization
        void Start()
        {
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            RTClient rtClient = RTClient.GetInstance();
            if (rtClient != null && rtClient.Bones != null)
            {
                var boneData = rtClient.Bones;
                Gizmos.color = Color.yellow;
                for (int i = 0; i < boneData.Count; i++)
                {
                    if (visibleBones)
                    {
                        Gizmos.DrawLine(boneData[i].FromMarker.Position, boneData[i].ToMarker.Position);
                    }
                }
            }
        }
    }
}

