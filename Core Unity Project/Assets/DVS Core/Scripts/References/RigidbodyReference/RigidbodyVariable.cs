using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dvs.Core
{
    [CreateAssetMenu(menuName = "DVS/Model/Rigidbody Reference")]
    public class RigidbodyVariable : ScriptableObject
    {
        private Action<Rigidbody> OnChange = (x) => { };
        public Rigidbody CurrentValue;
        private void OnEnable()
        {
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }
        public Rigidbody Value
        {
            get
            {
                return CurrentValue;
            }
            set
            {
                if (CurrentValue == value)
                {
                    return;
                }

                CurrentValue = value;

                OnChange(value);
            }
        }
    }
}
