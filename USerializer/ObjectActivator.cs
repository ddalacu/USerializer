using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace USerialization
{
    public static class ObjectActivator
    {
        private static readonly ConcurrentDictionary<Type, Func<object>> Activators = new();

        public static object CreateInstance(Type type)
        {
            if (RuntimeFeature.IsDynamicCodeSupported)
                return GetActivator(type)();

            return Activator.CreateInstance(type);
        }

        public static Func<object> GetActivator(Type type)
        {
            if (Activators.TryGetValue(type, out var activator))
                return activator;
            
            if (RuntimeFeature.IsDynamicCodeSupported == false)
            {
                activator= () => Activator.CreateInstance(type);
                Activators.TryAdd(type, CreateActivator(type));
                return activator;
            }

            activator = CreateActivator(type);
            Activators.TryAdd(type, activator);

            return activator;
        }

        private static Func<object> CreateActivator(Type type)
        {
            var constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, Type.EmptyTypes, null);

            if (constructor == null)
            {
                return () => Activator.CreateInstance(type);
            }

            var dynamicMethod =
                new DynamicMethod("Create_" + type.FullName, typeof(object), Type.EmptyTypes, type, true);
            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Newobj, constructor);
            il.Emit(OpCodes.Ret);

            return (Func<object>)dynamicMethod.CreateDelegate(typeof(Func<object>));
        }
    }
}