using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dvs.Core
{
    public interface IRigidbodyReference
    {
        Rigidbody Value { get; set; }
        void WireupChangeListener(Action<Rigidbody> listener, bool wireup = true);
    }

    [System.Serializable]
    public class RigidbodyReference : IRigidbodyReference
    {
        public bool UseInspector = true;
        public Rigidbody InspectorValue;
        public RigidbodyVariable Variable;
        private Action<Rigidbody> _Listener = (x) => { };
        public IRigidbodyReference Interface { get { return this; } }


        public Rigidbody Value
        {
            get
            {
                return UseInspector ? InspectorValue : Variable.Value;
            }
            set
            {
                if (UseInspector)
                {
                    if (InspectorValue == value)
                    {
                        return;
                    }

                    InspectorValue = value;
                    _Listener(value);
                    return;
                }

                if (Variable.Value == value)
                {
                    return;
                }

                Variable.Value = value;
                _Listener(value);
            }
        }

        public RigidbodyReference() { }
        public RigidbodyReference(Rigidbody value)
        {
            UseInspector = true;
            InspectorValue = value;
        }
        public RigidbodyReference(RigidbodyVariable boolVariable)
        {
            UseInspector = false;
            Variable = boolVariable;
        }

        public void WireupChangeListener(Action<Rigidbody> listener, bool wireup = true)
        {
            if (wireup)
            {
                _Listener += listener;
                return;
            }

            _Listener -= listener;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(RigidbodyReference))]
    public class RigidbodyReferenceDrawer : PropertyDrawer
    {

        private float SelectorYOffset = 4f;
        private float SelectorWidth = 20;

        private readonly string[] popupOptions =
        {
            "Use Inspector",
            "Use Reference"
        };

        private GUIStyle popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"))
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

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
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
    }
#endif
}
