using Dvs.Core.IoC;
using System;
using System.Reflection;

using UnityEngine;

namespace ObservableNamespace.Runtime
{
    [CreateAssetMenu(fileName = "", menuName = "Observable")]
    public class Observable : ScriptableObject
    {
#pragma warning disable 649
        private Type Type { get { return serializableType.Type; } }

        [SerializeField]
        [HideInInspector]
        private SerializableType serializableType;

        public void Trigger()
        {
            Di.Accessor.Trigger(this.Type);
        }

    }
}