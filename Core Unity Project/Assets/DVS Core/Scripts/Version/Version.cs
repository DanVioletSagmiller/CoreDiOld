namespace Dvs.Core
{
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    [Serializable]
    public class Version : ScriptableObject
    {
        public int Major = 0;

        public int Minor = 1;

        public int Build = 1;

        public string User = "";

        public string Began = "";

        public string Completed = "";

        public Product Product;

        public string ChangeLog = "";

        [HideInInspector]
        public Version Next = null;

        [HideInInspector]
        public Version Previous = null;

        public bool IsGreaterThan(int major, int minor, int build)
        {
            if (Major > major) return true;
            if (Major < major) return false;
            if (Minor > minor) return true;
            if (Minor < minor) return false;
            if (Build > build) return true;
            if (Build < build) return false;
            return false;
        }

        public bool IsLessThan(int major, int minor, int build)
        {
            if (Major < major) return true;
            if (Major > major) return false;
            if (Minor < minor) return true;
            if (Minor > minor) return false;
            if (Build < build) return true;
            if (Build > build) return false;
            return false;
        }

        public bool IsEqualTo(int major, int minor, int build)
        {
            return Major == major
                && Minor == minor
                && Build == build;
        }

        public static bool operator <( Version v1, Version v2)
        {
            if (object.ReferenceEquals(v2, null)
                || object.ReferenceEquals(v1, null)) return false;
            return v1.IsLessThan(v2.Major, v2.Minor, v2.Build);
        }

        public static bool operator >(Version v1, Version v2)
        {
            if (object.ReferenceEquals(v2, null)
                || object.ReferenceEquals(v1, null)) return false;
            return v1.IsGreaterThan(v2.Major, v2.Minor, v2.Build);
        }

        public static bool operator ==(Version v1, Version v2)
        {

            if (object.ReferenceEquals(v1, null) 
                && object.ReferenceEquals(v2, null)) return true;
            if (object.ReferenceEquals(v2, null) 
                || object.ReferenceEquals(v1, null)) return false;

            return v1.IsEqualTo(v2.Major, v2.Minor, v2.Build);
        }

        public static bool operator !=(Version v1, Version v2)
        {
            if (object.ReferenceEquals(v1, null) && object.ReferenceEquals(v2, null)) return false;
            if (object.ReferenceEquals(v1, null) || object.ReferenceEquals(v2, null)) return true;

            return !v1.IsEqualTo(v2.Major, v2.Minor, v2.Build);
        }

        /// <summary>
        /// ProductName v[Major].[Minor].[Build]
        /// </summary>
        public string Formal
        {
            get
            {
                return Product.name + " v" + VersionNumbers;
            }
        }

        /// <summary>
        /// [Major].[Minor].[Build]
        /// </summary>
        public string VersionNumbers
        {
            get
            {
                return Major + "." + Minor + "." + Build;
            }
        }
    }

#if (UNITY_EDITOR)
    [CustomEditor(typeof(Version))]
    public class VersionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var version = (Version)target;
            version.Product.CleanHistory();
            GUILayout.BeginVertical();
            {
                //ManageTime(version);
                ManageVersion(version);
                ManageChangeLog(version);
                ManageUser(version);
                ManageProduct(version);
                ManageNextVersion(version);
            }
            EditorGUILayout.EndVertical();
        }

        private void ManageTime(Version version)
        {
            EditorGUILayout.BeginHorizontal();
            {
                var created = version.Began;
                var completed = version.Completed;
                if (completed == null) completed = "";
                var timeline = "Started: " + created;
                if (completed.Length > 0) timeline += ", Completed: " + completed;
                EditorGUILayout.LabelField(timeline);

                var notCompleted = string.IsNullOrEmpty(version.Completed);
                if (notCompleted)
                {
                    if (GUILayout.Button("Complete", GUILayout.Height(16), GUILayout.Width(70))) 
                    {
                        version.Completed = DateTime.Now.ToShortDateString() 
                            + " " 
                            + DateTime.Now.ToShortTimeString();
                        EditorUtility.SetDirty(target);
                    }
                }
                else
                {
                    if (version.Next == null)
                    {
                        if (GUILayout.Button("undo", GUILayout.Height(16), GUILayout.Width(40)))
                        {
                            version.Completed = "";
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ManageNextVersion(Version version)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Next", GUILayout.Height(40)))
                {

                    if (version.Next == null)
                    {
                        CreateNextVersion(version);
                    }
                    else
                    {
                        var v = version;
                        while (v.Next != null) v = v.Next;
                        Selection.activeObject = v;
                    }
                }

                if (GUILayout.Button("Copy Log", GUILayout.Height(40)))
                {
                    var end = version.Completed;
                    if (end == null) end = "[incomplete]";
                    GUIUtility.systemCopyBuffer
                        = version.Formal + ":\r\n" 
                        + version.Began + "-" + end + "\r\n"
                        + version.ChangeLog;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void CreateNextVersion(Version version)
        {
            var next = new Version();
            next.Major = version.Major;
            next.Minor = version.Minor;
            next.Build = version.Build + 1;
            next.ChangeLog = "";
            next.Began = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            next.Product = version.Product;

            version.Next = next;
            next.Previous = version;
            version.Completed = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();

            var path = AssetDatabase.GetAssetPath(version);
            var pathFolder = path.Substring(0, path.LastIndexOf("/") + 1);

            AssetDatabase.CreateAsset(next, pathFolder + next.Formal + ".asset");
            Selection.activeObject = next;
            next.Product.CurrentVersion = next;
            version.Product.AddVersionToHistory(version);

            EditorUtility.SetDirty(next.Product);
            EditorUtility.SetDirty(next);
            EditorUtility.SetDirty(version);
        }

        private void ManageChangeLog(Version version)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorStyles.textField.wordWrap = true;
                var v = EditorGUILayout.TextArea(version.ChangeLog, GUILayout.Height(100));
                if (v != version.ChangeLog)
                {
                    version.ChangeLog = v;
                    EditorUtility.SetDirty(target);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ManageVersion(Version version)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label("Version");
                var v = EditorGUILayout.IntField(version.Major);
                if (v != version.Major)
                {
                    version.Major = v;
                    EditorUtility.SetDirty(target);
                }

                v = EditorGUILayout.IntField(version.Minor);
                if (v != version.Minor)
                {
                    version.Minor = v;
                    EditorUtility.SetDirty(target);
                }

                v = EditorGUILayout.IntField(version.Build);
                if (v != version.Build)
                {
                    version.Build = v;
                    EditorUtility.SetDirty(target);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ManageUser(Version version)
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (string.IsNullOrEmpty(version.User))
                {
                    version.User = GetUserName();
                    EditorUtility.SetDirty(target);
                }

                var user = EditorGUILayout.TextField("User", version.User);
                if (user != version.User)
                {
                    version.User = user;
                    EditorUtility.SetDirty(target);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ManageProduct(Version version)
        {
            EditorGUILayout.BeginHorizontal();
            {
                var v = (Product)EditorGUILayout.ObjectField("Product", version.Product, typeof(Product), true);
                if (v != version.Product)
                {
                    version.Product = v;
                    EditorUtility.SetDirty(version);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        public string GetUserName()
        {
            var editorWindow = typeof(UnityEditor.EditorWindow);
            var editorAssembly = Assembly.GetAssembly(editorWindow);
            var connect = editorAssembly.CreateInstance(
                "UnityEditor.Connect.UnityConnect",
                false,
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                null,
                null,
                null);

            var connectType = connect.GetType();
            var userinfo = connectType.GetProperty("userInfo").GetValue(connect, null);
            var userinfoType = userinfo.GetType();
            var displayName = userinfoType.GetProperty("displayName").GetValue(userinfo, null) as string;
            return displayName;
        }
    }
#endif
}