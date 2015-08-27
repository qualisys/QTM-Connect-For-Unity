using UnityEngine;
using UnityEditor;

namespace QualisysRealTime.Unity.Skeleton
{ 

    [CustomEditor(typeof(RTCharacterStream))]
    [CanEditMultipleObjects]
    public class RTCharacterStreamEditor : Editor
    {
        RTCharacterStream c;
        SerializedObject cSerializedObject;
        SerializedProperty boneRotation;

        void OnEnable()
        {
            c = (RTCharacterStream)target;
            cSerializedObject = new SerializedObject(target);
            boneRotation = cSerializedObject.FindProperty("boneRotatation");
        }
        public override void OnInspectorGUI()
        {
            cSerializedObject.Update();
            if (!c.jointsFound)
            {
                GUILayout.Space(5);
                GUIStyle warningStyle = new GUIStyle();
                warningStyle.richText = true;
                GUILayout.Label("<color=maroon>Warning: Not all character joints was found!</color>", warningStyle);
                GUILayout.Label("<color=maroon>              Animation may look strange or not work at all.</color>", warningStyle);
                GUILayout.Space(5);
            }

            var text = EditorGUILayout.TextField("Actor Markers Prefix", c.ActorMarkersPrefix);
            if (text != c.ActorMarkersPrefix)
            {
                c.ActorMarkersPrefix = text;
                c.ResetSkeleton();
            }
            if (EditorGUILayout.Toggle("Use IK", c.UseIK) != c.UseIK)
            {
                c.UseIK = !c.UseIK;
                c.ResetSkeleton();
            }
            if (EditorGUILayout.Toggle("Scale Movement To Size", c.ScaleMovementToSize) != c.ScaleMovementToSize)
            {
                c.ScaleMovementToSize = !c.ScaleMovementToSize;
                c.ResetSkeleton();
            }


            GUILayout.Space(5);
            CharactersModel m = (CharactersModel)EditorGUILayout.EnumPopup("Unity Character set",c.model);
            if (m != c.model)
            {
                c.model = m;
                c.SetModelRotation();
            }
            EditorGUI.indentLevel++;
            {
                EditorGUILayout.PropertyField(boneRotation, 
                    new GUIContent("Joint Rotations Fix", "Unity characters have different rotation on each limb"), true);
                cSerializedObject.ApplyModifiedProperties();
            }
            EditorGUI.indentLevel--;
            GUILayout.Space(5);

            EditorGUILayout.BeginVertical();
            c.headCam.UseHeadCamera = EditorGUILayout.BeginToggleGroup("Use Head Camera", c.headCam.UseHeadCamera);
            if (c.headCam.UseHeadCamera && !c.headCamera) c.GetCamera();
            else if (!c.headCam.UseHeadCamera && c.headCamera) c.DestroyCamera();

            c.headCam.CameraOffset = EditorGUILayout.Vector3Field("Offset from head", c.headCam.CameraOffset);

            c.headCam.UseVRHeadSetRotation = EditorGUILayout.Toggle("Use VR device rotation for head", c.headCam.UseVRHeadSetRotation);

            if (GUILayout.Button("Recenter camera"))
            {
                c.Recenter();
            }
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();
        }
    }
}