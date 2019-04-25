using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dvs.Core
{
    [CreateAssetMenu(menuName = "DVS/Variable/Boolean")]
    public class BoolVariable : ScriptableObject
    {
        private Action<bool> OnChange = (x) => { };
        public bool DefaultValue;
        public bool CurrentValue;
        public bool Value
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
        public void OnEnable()
        {
            CurrentValue = DefaultValue;
            hideFlags = HideFlags.DontUnloadUnusedAsset;
        }
    }
}
