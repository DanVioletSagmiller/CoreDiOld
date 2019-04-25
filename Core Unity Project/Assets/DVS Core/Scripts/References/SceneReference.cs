namespace Dvs.Core
{
    using System;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
#endif
    using UnityEngine;


    public interface ISceneReference
    {
        string SceneName { get; }
    }

    [CreateAssetMenu(menuName = "DVS/References/Scene Reference", fileName = "Scene")]
    public class SceneReference : ScriptableObject, ISceneReference
    {
#if UNITY_EDITOR
        public SceneAsset TargetScene;
#endif
        public string SceneName { get; private set; }

        internal void ResetSceneName()
        {
            SceneName = TargetScene.name;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SceneReference))]
    public class SceneReferenceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var reference = (SceneReference)target;
            var scene = reference.TargetScene;
            base.OnInspectorGUI();
            if (scene != reference.TargetScene)
            {
                reference.ResetSceneName();
            }
        }
    }

 public class SceneReferenceBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }
        public void OnPreprocessBuild(BuildReport report)
        {
            var sceneReferences = GetAllInstances<SceneReference>();
            foreach(var sceneReference in sceneReferences)
            {
                sceneReference.ResetSceneName();
            }
        }

        public static T[] GetAllInstances<T>() where T : ScriptableObject
        {
            var assetGuids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            T[] assets = new T[assetGuids.Length];
            for (int i = 0; i < assetGuids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                assets[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return assets;

        }
    }
#endif
}