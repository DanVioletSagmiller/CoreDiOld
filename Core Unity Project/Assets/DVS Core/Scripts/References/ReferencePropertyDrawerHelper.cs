using System;
using UnityEditor;
using UnityEngine;

namespace Dvs.Core
{
    public class ReferencePropertyDrawerHelper
    {
        public float SelectorYOffset { get; set; } = 4f;
        public float SelectorWidth { get; set; } = 20;

        internal void OnGuiHelper(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            var useConstant = property.FindPropertyRelative("UseInspector");
            var constantValue = property.FindPropertyRelative("InspectorValue");
            var variable = property.FindPropertyRelative("Variable");

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect popupPosition = new Rect(position);
            popupPosition.y += SelectorYOffset;
            popupPosition.xMax = popupPosition.xMin + SelectorWidth;
            position.xMin += SelectorWidth;

            int isConstantNumeric = EditorGUI.Popup(
                popupPosition,
                useConstant.boolValue ? 0 : 1,
                popupOptions,
                popupStyle);

            useConstant.boolValue = isConstantNumeric == 0;

            EditorGUI.PropertyField(
                position,
                useConstant.boolValue ? constantValue : variable,
                GUIContent.none);

            var changed = EditorGUI.EndChangeCheck();
            if (changed)
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        private readonly string[] popupOptions =
        {
            "Use Inspector",
            "Use Reference"
        };

        public GUIStyle popupStyle { get; set; } = new GUIStyle(GUI.skin.GetStyle("PaneOptions"))
        {
            imagePosition = ImagePosition.ImageOnly
        };

        private Rect GetButtonRect(Rect rect)
        {
            rect = new Rect(rect);
            rect.yMin = popupStyle.margin.top;
            rect.width = popupStyle.fixedWidth + popupStyle.margin.right;
            return rect;

        }


    }
}