using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityMVC.Editor
{
    public class EditorReflection : MonoBehaviour
    {

        /// <summary>
        /// Get all Controller Classes that derived from MonoController
        /// </summary>
        /// <param name="assembly">Assembly to lookup</param>
        /// <returns>IEnurable of Types</returns>
        internal static IEnumerable<Type> GetMonoControllerClasses(Assembly assembly)
        {
            if (assembly == null)
                return null;

            Type baseType = typeof(MonoController);
            return assembly.GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t));
        }

        /// <summary>
        /// Get all view classes
        /// </summary>
        /// <param name="assembly">Assembly to lookup</param>
        /// <returns>IEnurable of Types</returns>
        internal static IEnumerable<Type> GetViewClasses(Assembly assembly)
        {
            if (assembly == null)
                return null;

            Type baseType = typeof(ViewContainer);
            return assembly.GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t));
        }

        internal static Type GetViewClass(Assembly assembly, string name)
        {
            if(assembly == null)
                return null;

            return assembly.GetType(name);
        }
    }
}