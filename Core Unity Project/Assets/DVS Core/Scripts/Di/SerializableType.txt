using System;

using UnityEngine;

namespace ObservableNamespace.Runtime
{
    [System.Serializable]
    public class SerializableType : ISerializationCallbackReceiver
    {

        [SerializeField]
        [HideInInspector]
        private string assemblyQualifiedName;

        public Type Type { get; private set; }

        #region ISerializationCallBacks
        public void OnBeforeSerialize()
        {
            if (Type != null)
                assemblyQualifiedName = Type.AssemblyQualifiedName;
            else
                assemblyQualifiedName = "";
        }

        public void OnAfterDeserialize()
        {
            Type t = Type.GetType(assemblyQualifiedName);
            if (t != null)
                Type = t;

            assemblyQualifiedName = "";
        }
        #endregion
    }
}