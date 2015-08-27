using UnityEngine;
using System.Collections;
using UnityEditor;

namespace QualisysRealTime.Unity.Skeleton
{ 

    [CustomEditor(typeof(RTCharacterStreamDebug))]
    public class RTCharacterStreamDebugEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            RTCharacterStreamDebug characterStream = (RTCharacterStreamDebug)target;
            if (GUILayout.Button("Reset Skeleton"))
            {
                characterStream.ResetSkeleton();
            }
            if (GUILayout.Button("Set model Skeleton"))
            {
                characterStream.SetModelRotation();
            }
        }
    }
}