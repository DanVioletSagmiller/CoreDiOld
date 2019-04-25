using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DVS.Core
{

    public interface IActionVariable
    {
        void WireupListener(Action onChange, bool wireup = true);
        void Call();
    }

    [CreateAssetMenu(menuName = "DVS/Variable/Action")]
    public class ActionVariable : ScriptableObject, IActionVariable
    {
        private Action OnChange = () => { };

        public void OnEnable()
        {
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }

        public void WireupListener(Action onChange, bool wireup = true)
        {
            if (wireup)
            {
                OnChange += onChange;
            }
            else
            {
                OnChange -= onChange;
            }
        }

        public void Call()
        {
            OnChange();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ActionVariable))]
    public class ActionVariableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var pressedCallButton = GUILayout.Button("Call");

            if (pressedCallButton)
            {
                var actionVariable = (ActionVariable)target;
                actionVariable.Call();
            }
        }
    }
#endif
}
