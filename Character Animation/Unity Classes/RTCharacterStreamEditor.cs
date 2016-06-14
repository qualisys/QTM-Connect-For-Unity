#if UNITY_EDITOR
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
			boneRotation = cSerializedObject.FindProperty("boneRotation");
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

            GUILayout.Space(10);
            var guiContent = new GUIContent();

            guiContent.text = "Prefix for actors markers";
            guiContent.tooltip = "To use multiple actors, differient them from each other with a specific prefix on each marker and put the prefix here, e.g. \"Actor1_\"";

            var prefixText = EditorGUILayout.TextField(guiContent, c.ActorMarkerPrefix);
            if (prefixText != c.ActorMarkerPrefix)
            {
                c.ActorMarkerPrefix = prefixText;
                ResetIfActive(c);
            }

            GUILayout.Space(5);

            guiContent.text = "Actor Height";
            guiContent.tooltip = "... in cm";
            var actorHeight = EditorGUILayout.IntField(guiContent, c.actorHeight);
            if (actorHeight != c.actorHeight)
            {
                c.actorHeight = actorHeight;
                //ResetIfActive(c);
            }

            guiContent.text = "Actor Mass";
            guiContent.tooltip = "... in kg";
            var actorMass = EditorGUILayout.IntField(guiContent, c.actorMass);
            if (actorMass != c.actorMass)
            {
                c.actorMass = actorMass;
                //ResetIfActive(c);
            }

            guiContent.text = "Calibrate Character";
            guiContent.tooltip = "...";
            if (GUILayout.Button(guiContent))
            {
                c.Calibrate();
            }

            guiContent.text = "Reset Character";
            guiContent.tooltip = "...";
            if (GUILayout.Button(guiContent))
            {
                c.ResetSkeleton();
            }

            GUILayout.Space(5);

            guiContent.text = "Solve gaps using IK";
            guiContent.tooltip = "When markers are missing, the rotation and position of bones will be unknown.\nCheck this to use IK to predict bone positions in the skeleton";
            if (EditorGUILayout.Toggle(guiContent, c.UseIK) != c.UseIK)
            {
                c.UseIK = !c.UseIK;
                ResetIfActive(c);
            }

            guiContent.text = "Use Tracking Markers";
            //guiContent.tooltip = "When markers are missing, the rotation and position of bones will be unknown.\nCheck this to use IK to predict bone positions in the skeleton";
            if (EditorGUILayout.Toggle(guiContent, c.UseTrackingMarkers) != c.UseTrackingMarkers)
            {
                c.UseTrackingMarkers = !c.UseTrackingMarkers;
                ResetIfActive(c);
            }

            guiContent.text = "Use finger rotation";
            guiContent.tooltip = "Map the rotation of the actors fingers to the character in Unity";
            if (EditorGUILayout.Toggle(guiContent, c.UseFingers) != c.UseFingers)
            {
                c.UseFingers = !c.UseFingers;
                ResetIfActive(c);
            }

            guiContent.text = "Scale movement to character size";
            guiContent.tooltip = "If the character is smaller or bigger then the actor, the character will float in the air or sink into the ground and move to much or to little.\nCheck this to scale the movement to the size of the character model.";
            if (EditorGUILayout.Toggle(guiContent, c.ScaleMovementToSize) != c.ScaleMovementToSize)
            {
                c.ScaleMovementToSize = !c.ScaleMovementToSize;
                ResetIfActive(c);
            }

            GUILayout.Space(5);
            guiContent.text = "Character rotation model";
            guiContent.tooltip = "Characters have different definition of how each bone rotation, test different models if the character looks strange.\n\nHint: Change in Play mode will not be saved!";
            CharacterModels m = (CharacterModels)EditorGUILayout.EnumPopup(guiContent, c.model);
			if (m != c.model)
            {
                c.model = m;
                c.SetModelRotation();
            }
            EditorGUI.indentLevel++;
            {
                guiContent.text = "Character rotations";
                guiContent.tooltip = "Fix each limb rotation by defining the euler angels here.";
                EditorGUILayout.PropertyField(boneRotation, guiContent, true);
                cSerializedObject.ApplyModifiedProperties();
            }
            EditorGUI.indentLevel--;
            GUILayout.Space(5);

            EditorGUILayout.BeginVertical();

            guiContent.text = "Add camera to head";
            guiContent.tooltip = "Attach a camera to the head of the character.";
            c.headCam.UseHeadCamera = EditorGUILayout.BeginToggleGroup(guiContent, c.headCam.UseHeadCamera);

            EditorGUI.indentLevel++;

            guiContent.text = "Camera offset from head";
            guiContent.tooltip = "The vector offset from the head joint to the camera, if zero, the camera will be placed in the middle of the head.\n\nHint: Change the Field of View in the camera settings in the Inspector under character -> CameraAnchor -> Camera";
            c.headCam.CameraOffset = EditorGUILayout.Vector3Field(guiContent, c.headCam.CameraOffset);

            guiContent.text = "Set head rotation to VR device";
            guiContent.tooltip = "Have this checked if using a Oculus Rift or other VR device who rotates the camera, otherwise rotation will be doubled.\n\nHint: No markers on the head is then necessary.";
            c.headCam.UseVRHeadSetRotation = EditorGUILayout.Toggle(guiContent, c.headCam.UseVRHeadSetRotation);

            guiContent.text = "Recenter camera";
            guiContent.tooltip = "Recenter camera so that it looks the same way as the character.";
            if (GUILayout.Button(guiContent))
            {
                c.Recenter();
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();

            guiContent.text = "DEBUG";
            if (EditorGUILayout.Toggle(guiContent, DEBUG.enabled) != DEBUG.enabled)
            {
                DEBUG.enabled = !DEBUG.enabled;
            }
        }

        void ResetIfActive(RTCharacterStream c)
        {
            if (c.isActiveAndEnabled)
                c.ResetSkeleton();
        }
    }
}
#endif