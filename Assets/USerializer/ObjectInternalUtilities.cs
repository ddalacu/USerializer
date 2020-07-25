using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace USerialization
{
    /// <summary>
    /// Prevides access to internal methods in <see cref="Object"/>
    /// </summary>
    public static class ObjectInternalUtilities
    {
        public delegate bool IsObjectAliveDelegate(int instanceId);
        public delegate Object FindObjectFromInstanceIdDelegate(int instanceId);

        public static IsObjectAliveDelegate IsObjectAlive;
        public static FindObjectFromInstanceIdDelegate FindObjectFromInstanceId;


        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            Type type = typeof(Object);

            var isObjectAliveMethod = type.GetMethod("DoesObjectWithInstanceIDExist", BindingFlags.Static | BindingFlags.NonPublic);
            IsObjectAlive = (IsObjectAliveDelegate)Delegate.CreateDelegate(typeof(IsObjectAliveDelegate), null, isObjectAliveMethod);

            var findObjectFromInstanceIdMethod = type.GetMethod("FindObjectFromInstanceID", BindingFlags.Static | BindingFlags.NonPublic);
            FindObjectFromInstanceId = (FindObjectFromInstanceIdDelegate)Delegate.CreateDelegate(typeof(FindObjectFromInstanceIdDelegate), null, findObjectFromInstanceIdMethod);
        }
    }
}