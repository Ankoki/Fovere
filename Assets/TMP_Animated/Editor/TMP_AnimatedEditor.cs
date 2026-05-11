using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
    [CustomEditor(typeof(TMP_Animated), true)]
    [CanEditMultipleObjects]
    public class TMP_AnimatedEditor : TMP_BaseEditorPanel
    {
        private SerializedProperty _speedProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            _speedProperty = serializedObject.FindProperty("speed");
        }

        protected override void DrawExtraSettings()
        {
            EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_speedProperty, new GUIContent("Default Speed"));
        }

        //Unused but necessary overrides.
        protected override void OnUndoRedo()
        {
        }

        protected override bool IsMixSelectionTypes()
        {
            return false;
        }
    }
}