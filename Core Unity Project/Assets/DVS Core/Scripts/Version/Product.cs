namespace Dvs.Core
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [CreateAssetMenu(menuName = "DVS/Product")]
    public class Product : ScriptableObject
    {
        public string Url;
        public Version CurrentVersion;
        public string Description;
        public Version[] History = new Version[] { };
        public void AddVersionToHistory(Version version)
        {
            Array.Resize<Version>(ref History, History.Length + 1);
            History[History.Length - 1] = version;
        }
        public void CleanHistory()
        {
            // add the current version to the main history list, so we can check each item in one pass
            List<Version> history = new List<Version>(History);
            if (CurrentVersion != null)
            {
                history.Add(CurrentVersion);
                CurrentVersion = null; // set null, so we don't have to put it back if we find it null.
            }

            // check each item, and remove records with null versions. (files were deleted)
            for (int i = history.Count - 1; i > -1; i--)
            {
                if (history[i] == null)
                {
                    history.RemoveAt(i);
                }
            }

            // if there are no items in the history, Exit now, since there is nothing to sort out.
            if (history.Count == 0) return; 

            // Cleanup any broken links found.
            for (int i = 0; i < history.Count - 1; i++)
            {
                history[i].Next = history[i + 1];
                history[i + 1].Previous = history[i];
            }

            // get the last item
            var last = history[history.Count - 1];

            CurrentVersion = last;
            history.Remove(last);

            History = history.ToArray();
        }
    }

#if (UNITY_EDITOR)
    [CustomEditor(typeof(Product))]
    public class ProductEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var product = (Product)target;

            ManageUrl(product);
            ManageCurrentVersion(product);
            CreateFirstVersion(product);
            ShowHistory(product);
        }

        private void ManageDescription(Product product)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorStyles.textField.wordWrap = true;
                var v = EditorGUILayout.TextArea(product.Description, GUILayout.Height(100));
                if (v != product.Description)
                {
                    product.Description = v;
                    EditorUtility.SetDirty(target);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        

        private void ShowHistory(Product product)
        {
            product.CleanHistory();

            foreach (var version in product.History)
            {
                var content = new GUIContent("v" + version.VersionNumbers,
                    version.User + version.Completed + "\r\n" + version.ChangeLog);
                if (GUILayout.Button(content, GUILayout.Height(20)))
                {
                    EditorUtility.RevealInFinder(AssetDatabase.GetAssetPath(version));
                }
            }
        }

        private void CreateFirstVersion(Product product)
        {
            if (product.CurrentVersion == null)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Create v0.1.1", GUILayout.Height(40)))
                    {
                        var v = new Version();
                        v.Major = 0;
                        v.Minor = 1;
                        v.Build = 1;
                        v.Product = product;
                        v.Began = DateTime.Now.ToShortDateString()
                            + " "
                            + DateTime.Now.ToShortTimeString();

                        var pathCurrent = AssetDatabase.GetAssetPath(product);
                        var pathFolder = pathCurrent.Substring(0, pathCurrent.LastIndexOf("/") + 1);

                        AssetDatabase.CreateAsset(v, pathFolder + v.Formal + ".asset");
                        Selection.activeObject = v;
                        product.CurrentVersion = v;
                        EditorUtility.SetDirty(v.Product);
                        EditorUtility.SetDirty(product);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void ManageCurrentVersion(Product product)
        {
            EditorGUILayout.BeginHorizontal();
            {
                var v = (Version)EditorGUILayout.ObjectField("Current", product.CurrentVersion, typeof(Version), true);
                if (v != product.CurrentVersion)
                {
                    product.CurrentVersion = v;
                    EditorUtility.SetDirty(product);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ManageUrl(Product product)
        {
            EditorGUILayout.BeginHorizontal();
            {
                var url = EditorGUILayout.TextField("URL", product.Url);
                if (product.Url != url)
                {
                    product.Url = url;
                    EditorUtility.SetDirty(target);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}