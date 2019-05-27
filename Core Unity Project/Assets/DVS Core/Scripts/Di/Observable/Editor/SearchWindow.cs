using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using ObservableNamespace.Utilities;

namespace ObservableNamespace.CustomEditor
{
    /// <summary>
    /// SearchWindow for Type objects
    /// </summary>
    public class SearchWindow : EditorWindow
    {
        private static Action<Type> OnTypeSelectedHandler;

        //options
        private static int resultShowCount = 20;
        private static int minSizeX = 100;
        private static int maxSizeX = 250;
        private static int elementCountOffset = 9;
        private static bool simpleNames = true;

        private List<Type> allTypes;
        private string searchString = "";
        private Type[] filteredList;
        private bool searchStringChanged;

        /// <summary>
        /// Initialize SearchWindow for Type objects
        /// </summary>
        /// <param name="buttonRect"></param>
        /// <param name="OnTypeSelectedHandler"></param>
        /// <returns></returns>
        public static SearchWindow Init(Rect buttonRect, Action<Type> OnTypeSelectedHandler)
        {
            SearchWindow window = GetWindow<SearchWindow>();

            window.titleContent = new GUIContent("Select Type");
            SearchWindow.OnTypeSelectedHandler = OnTypeSelectedHandler;
            window.ShowAsDropDown(buttonRect, new Vector2(
                Mathf.Clamp(buttonRect.size.x, minSizeX, maxSizeX), EditorGUIUtility.singleLineHeight * (resultShowCount + elementCountOffset)));
            window.ShowPopup();
            return window;
        }

        #region Unity Callbacks
        private void OnEnable()
        {
            allTypes = ObservableUtility.GetAllTypes();
            filteredList = new Type[resultShowCount];
            UpdateFilterdList("");
        }

        private void OnGUI()
        {
            GUILayout.Label("Search: ");
            string currentSearchString = EditorGUILayout.TextField(searchString);

            if (currentSearchString != searchString)
            {
                UpdateFilterdList(currentSearchString);
                searchString = currentSearchString;
            }

            foreach (var t in filteredList)
            {
                if (t != null)
                {
                    var name =ObservableUtility.NameWithParent(t.FullName);
                    if (GUILayout.Button(name, ObservableUtility.ButtonStyle))
                    {
                        OnTypeSelectedHandler(t);
                    }
                }
            }
        }

        private void OnLostFocus()
        {
            OnTypeSelectedHandler = null;       //Unity will destroy the target of the delegate regardless
            filteredList = null;                //can be removed
            allTypes = null;                    //can be removed
            GC.Collect();                       //can be removed    but memory will get released later
            Close();
        }

        #endregion

        private void UpdateFilterdList(string searchString)
        {
            int count = 0;
            int index = 0;
            do
            {
                if (index > allTypes.Count - 1)
                    break;

                if (searchString == "" || allTypes[index].FullName.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    filteredList[count] = allTypes[index];
                    count++;
                }
                index++;

            } while (count < resultShowCount);

            if (count < resultShowCount - 1)
                ClearFilteredList(count);
        }

        private void ClearFilteredList(int fromIndex)
        {
            for (int i = fromIndex; i < filteredList.Length; i++)
                filteredList[i] = null;
        }
    }
}