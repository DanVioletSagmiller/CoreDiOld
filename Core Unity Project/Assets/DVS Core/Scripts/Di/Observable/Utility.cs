using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ObservableNamespace.Utilities
{
    public class ObservableUtility
    {
        /// <summary>
        /// Get all Types from all Assemblies
        /// </summary>
        /// <returns></returns>
        public static List<Type> GetAllTypes()
        {
            return (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                    from assemblyType in domainAssembly.GetTypes()
                    select assemblyType).ToList();
        }

        public static List<System.Reflection.Assembly> GetAllAssemblies()
        {
            return (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                    select domainAssembly).ToList();
        }

        public static string NameWithParent(string fullName)
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

        private static GUIStyle _ButtonStyle;
        private static bool _ButtonStyleNotSet = true;

        public static GUIStyle ButtonStyle
        {
            get
            {
                if (_ButtonStyleNotSet)
                {
                    _ButtonStyleNotSet = false;
                    _ButtonStyle = new GUIStyle(GUI.skin.button);
                    _ButtonStyle.alignment = TextAnchor.MiddleLeft;
                }

                return _ButtonStyle;
            }
        }
    }
}