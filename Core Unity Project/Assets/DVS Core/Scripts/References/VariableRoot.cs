using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dvs.Core
{
    public class ReferencePropertyDrawerBase : PropertyDrawer
    {

        private float SelectorYOffset = 4f;
        private float SelectorWidth = 20;
        private string[] popupOptions;
        private GUIStyle popupStyle;
        private float PopupStyleYMinModifier;
        private float PopupStyleWidth;
        public ReferencePropertyDrawerBase()
        {
            popupOptions = new string[]
            {
                "Use Inspector",
                "Use Reference"
            };

            popupStyle = new GUIStyle(GUI.skin.GetStyle("PaneOptions"))
            {
                imagePosition = ImagePosition.ImageOnly
            };

            PopupStyleYMinModifier = popupStyle.margin.top;
            PopupStyleWidth = popupStyle.fixedWidth + popupStyle.margin.right;
        }

        private Rect GetButtonRect(Rect rect)
        {
            rect = new Rect(rect)
            {
                yMin = PopupStyleYMinModifier,
                width = PopupStyleWidth
            };
            return rect;
        }
        public void OnGui(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            EditorGUI.BeginChangeCheck();

            SerializedProperty useConstant = property.FindPropertyRelative("UseInspector");
            SerializedProperty constantValue = property.FindPropertyRelative("InspectorValue");
            SerializedProperty variable = property.FindPropertyRelative("Variable");

            int indent = EditorGUI.indentLevel;
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

            bool changed = EditorGUI.EndChangeCheck();
            if (changed)
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
    public abstract class VariableRoot : ScriptableObject
    {
        public Type ObjectType;
        public object CurrentValue;
        public object DefaultValue;
        public void OnEnable()
        {
            CurrentValue = DefaultValue;
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }
    }

    public abstract class VariableRoot<T> : VariableRoot where T : class
    {
        private Action<T> OnChange = (x) => { };
        public void WireupChangeListener(Action<T> onChange, bool wireup = true)
        {
            if (wireup) OnChange += onChange;
            else OnChange -= onChange;
        }

        public new T CurrentValue;
        public new T DefaultValue;
        
        public T Value
        {
            get
            {
                return CurrentValue;
            }
            set
            {
                if (CurrentValue == value)
                {
                    return;
                }

                CurrentValue = value;

                OnChange(value);
            }
        }
    }

    public abstract class StructRoot<T> : VariableRoot where T : struct
    {
        private Action<T> OnChange = (x) => { };
        public void WireupChangeListener(Action<T> onChange, bool wireup = true)
        {
            if (wireup) OnChange += onChange;
            else OnChange -= onChange;
        }

        public new T CurrentValue;
        public new T DefaultValue;

        public T Value
        {
            get
            {
                return CurrentValue;
            }
            set
            {
                if (CurrentValue.Equals(value))
                {
                    return;
                }

                CurrentValue = value;

                OnChange(value);
            }
        }
    }

    [System.Serializable]
    public class ReferenceRoot<T> where T : class
    {
        public bool UseInspector = true;
        public T InspectorValue;
        public VariableRoot<T> Variable;

        public T Value
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
                    return;
                }

                if (Variable.Value == value)
                {
                    return;
                }

                Variable.Value = value;
            }
        }
    }

    [System.Serializable]
    public class ReferenceStructRoot<T> where T : struct
    {
        public bool UseInspector = true;
        public T InspectorValue;
        public StructRoot<T> Variable;

        public T Value
        {
            get
            {
                return UseInspector ? InspectorValue : Variable.Value;
            }
            set
            {
                if (UseInspector)
                {
                    if (InspectorValue.Equals(value))
                    {
                        return;
                    }

                    InspectorValue = value;
                    return;
                }

                if (Variable.Value.Equals(value))
                {
                    return;
                }

                Variable.Value = value;
            }
        }
    }
}
