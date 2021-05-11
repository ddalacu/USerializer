﻿using System;
using System.Reflection;

namespace USerialization
{
    public static class ModuleInitializer
    {
        public static void Initialize()
        {
            var localInits = (LocalModuleInitializeAttribute[])typeof(ModuleInitializer).Assembly.GetCustomAttributes(typeof(LocalModuleInitializeAttribute), true);

            Array.Sort(localInits, (a, b) => b.Order - a.Order);

            foreach (var localInit in localInits)
            {
                var initializerMethod = localInit.TargetType.GetMethod(localInit.Method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                if (initializerMethod == null)
                    continue;

                initializerMethod.Invoke(null, Array.Empty<object>());
            }
        }
    }
}