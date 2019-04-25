using System;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DVS.Core
{
    public interface IActionReference
    {
        IActionVariable Variable { get; }
    }

    [System.Serializable]
    public class ActionReference : IActionReference
    {
        [SerializeField]
        private ActionVariable _Variable = null;
        public IActionVariable Variable { get { return _Variable; } }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ActionReference))]
    public class ActionReferenceDrawer : PropertyDrawer
    {
        
    }
#endif
}
