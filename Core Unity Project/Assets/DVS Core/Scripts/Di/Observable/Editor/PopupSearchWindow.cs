using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using ObservableNamespace.Utilities;

namespace ObservableNamespace.CustomEditor
{

    public class PopupSearchWindow : PopupWindowContent
    {
        private Action<Type> OnTypeSelectedHandler;

        //options
        private int resultShowCount = 20;
        private int minSizeX = 100;
        private int maxSizeX = 250;
        private int elementCountOffset = 9;
        private bool simpleNames = true;

        private float windowSizeX;
        private List<Type> allTypes;
        private string searchString = "";
        private Type[] filteredList;
        private bool searchStringChanged;

        #region ctor
        public PopupSearchWindow(Action<Type> OnTypeSelectedHandler, float windowSizeX)
        {



            this.windowSizeX = Mathf.Clamp(windowSizeX, minSizeX,maxSizeX);
            this.OnTypeSelectedHandler = OnTypeSelectedHandler;
            allTypes = ObservableUtility.GetAllTypes();
            filteredList = new Type[resultShowCount];
            UpdateFilterdList("");
        }
        #endregion

        private string NameWithParent(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return fullName;
            }

            var hasNoLineage = !fullName.Contains(".");

            if (hasNoLineage)
            {
                return fullName;
            }

            var pathParts = fullName.Split('.');

            var lastIndex = pathParts.Length - 1;
            return pathParts[lastIndex - 1] + '.' + pathParts[lastIndex];
        }

        #region PopupWindow/ Unity Callbacks

        public override void OnGUI(Rect rect)
        {
            GUILayout.Label("Search Typesss: ");
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
                    var name = ObservableUtility.NameWithParent(t.FullName);
                    if (GUILayout.Button(name, ObservableUtility.ButtonStyle))
                    {
                        OnTypeSelectedHandler(t);
                    }
                }
            }
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(windowSizeX, EditorGUIUtility.singleLineHeight * (resultShowCount + elementCountOffset));
        }

        public override void OnClose()
        {
            filteredList = null;
            allTypes = null;
            OnTypeSelectedHandler = null;
            GC.Collect();
            base.OnClose();
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