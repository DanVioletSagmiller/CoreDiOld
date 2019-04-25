using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dvs.Core
{


    public interface IBoolReference
    {
        bool Value { get; set; }
        void WireupChangeListener(Action<bool> listener, bool wireup = true);
    }

    [System.Serializable]
    public class BoolReference : IBoolReference
    {
        public bool UseInspector = true;
        public bool InspectorValue;
        public BoolVariable Variable;
        private Action<bool> _Listener = (x) => { };
        public IBoolReference Interface { get { return this; } }


        public bool Value
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

        public BoolReference() { }
        public BoolReference(bool value)
        {
            UseInspector = true;
            InspectorValue = value;
        }
        public BoolReference(BoolVariable boolVariable)
        {
            UseInspector = false;
            Variable = boolVariable;
        }

        public void WireupChangeListener(Action<bool> listener, bool wireup = true)
        {
            if (wireup)
            {
                _Listener += listener;
                return;
            }

            _Listener -= listener;
        }

        public void SwitchState()
        {
            Interface.Value = !Interface.Value;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(BoolReference))]
    public class BoolReferenceDrawer : PropertyDrawer
    {
        private ReferencePropertyDrawerHelper helper = new ReferencePropertyDrawerHelper();

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            helper.OnGuiHelper(position, property, label);
        }

       
    }
#endif
}
