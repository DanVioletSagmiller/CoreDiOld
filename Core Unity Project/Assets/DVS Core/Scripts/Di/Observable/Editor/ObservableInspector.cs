using System;
using System.Reflection;

using UnityEngine;
using UnityEditor;

using ObservableNamespace.Runtime;
using ObservableNamespace.Utilities;

namespace ObservableNamespace.CustomEditor
{
    /// <summary>
    /// Custom Inspector typeof(Observable)
    /// </summary>
    [UnityEditor.CustomEditor(typeof(Observable))]
    public class ObservableInspector : Editor
    {
        private PropertyInfo propertyInfo;      //the property Type of SerializableType of the Observable class
        private object targetObject;            //the SerializableType instance
        private EditorWindow window;
        private Rect buttonRect;

        #region Unity Callbacks
        private void OnEnable()
        {
            window = EditorWindow.focusedWindow;

            Type objType = target.GetType();
            Type serializedType_type = objType.GetField("serializableType", BindingFlags.Instance | BindingFlags.NonPublic).FieldType;
            propertyInfo = serializedType_type.GetProperty("Type", BindingFlags.Instance | BindingFlags.Public);
            targetObject = objType.GetField("serializableType", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        }

        public override void OnInspectorGUI()
        {
            if (targetObject == null)   //prevents the Inspector from drawing while initializing new Instance in Editor
                return;

            Type currentValue = propertyInfo.GetValue(targetObject) as Type;
            string valueString = currentValue == null ? "null" : ObservableUtility.NameWithParent(currentValue.FullName);

            GUILayout.Label("Type: " + valueString);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Search"))
                SearchType(buttonRect, OnTypeSelectedHandler);              //use for SearchWindow as EditorWindow
                                                                            //SearchTypePopup(buttonRect, OnTypeSelectedHandler);       //use for SearchWindow as Popup

            if (Event.current.type == EventType.Repaint)
            {
                buttonRect = GUILayoutUtility.GetLastRect();
            }

            if (GUILayout.Button("Clear"))
            {
                Clear();
            }

            if (GUILayout.Button("Fire"))
            {
                ((Observable)target).Trigger();
            }

            GUILayout.EndHorizontal();
        }


        #endregion
        private void SearchType(Rect rect, Action<Type> OnTypeSelectedHandler)
        {
            rect = EditorGUIUtility.GUIToScreenRect(rect);
            Vector2 offset = new Vector2(rect.size.x / 25, rect.size.y * 1.5f);
            rect = new Rect(rect.position + offset, rect.size);

            SearchWindow.Init(rect, OnTypeSelectedHandler);
        }

        private void SearchTypePopup(Rect rect,Action<Type> OnTypeSelectedHandler)
        {
            PopupWindow.Show(rect, new PopupSearchWindow(OnTypeSelectedHandler, rect.size.x));
        }

        private void Clear()
        {
            propertyInfo.SetValue(targetObject, null);
            EditorUtility.SetDirty(target);
        }

        protected void OnTypeSelectedHandler(Type type)
        {
            propertyInfo.SetValue(targetObject, type);
            window.Focus();
            EditorUtility.SetDirty(target);
        }
    }

}