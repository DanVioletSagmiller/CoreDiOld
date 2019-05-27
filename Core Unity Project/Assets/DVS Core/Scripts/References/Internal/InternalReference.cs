using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dvs.Core
{


    public interface IInternalReference
    {
        object Value { get; set; }
        void WireupChangeListener(Action<object> listener, bool wireup = true);
    }

    [System.Serializable]
    public class InternalReference : IInternalReference
    {
        public object InternalValue;
        private Action<object> _Listener = (x) => { };
        public IInternalReference Interface { get { return this; } }


        public object Value
        {
            get
            {
                return InternalValue;
            }
            set
            { 
                if (InternalValue == value)
                {
                    return;
                }

                InternalValue = value;
                _Listener(value);
                return;
            }
        }

        public InternalReference() { }
        public InternalReference(object value)
        {
            InternalValue = value;
        }

        public void WireupChangeListener(Action<object> listener, bool wireup = true)
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
    [CustomPropertyDrawer(typeof(InternalReference))]
    public class InternalReferenceDrawer : PropertyDrawer
    {
        private ReferencePropertyDrawerHelper helper = new ReferencePropertyDrawerHelper();

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            //TODO: Display/Filter/Select Type to use

            if (GUI.Button(new Rect(new Vector2(0, 0), new Vector2(100, 25)), "Call"))
            {
                this.GetType().GetMethod(nameof(CallMe)).MakeGenericMethod(typeof(string)).Invoke(null, null);
            }
        }

        private void CallMe<T>()
        {
            // does something cool
        }

       
    }
#endif
}
