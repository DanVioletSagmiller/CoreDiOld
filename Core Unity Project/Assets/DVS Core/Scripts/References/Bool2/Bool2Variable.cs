
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dvs.Core
{
    [CreateAssetMenu(menuName = "DVS/Variable/Boolean2")]
    public class Bool2Variable : StructRoot<bool> { }
    public interface IBool2Reference
    {
        bool Value { get; set; }
    }



    [System.Serializable]
    public class Bool2Reference : ReferenceStructRoot<bool>, IBool2Reference
    {
        public void SwitchState()
        {
            Value = !Value;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(Bool2Reference))]
    public class Bool2ReferenceDrawer : PropertyDrawer
    {
        private ReferencePropertyDrawerHelper helper = new ReferencePropertyDrawerHelper();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            helper.OnGuiHelper(position, property, label);
        }
    }
#endif
}
