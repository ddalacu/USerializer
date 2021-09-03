using System;
using System.Reflection;

namespace USerialization
{
    public static class ILUtils
    {
        public static IntPtr GetAddress<T>(string methodName)
        {
            var type = typeof(T);
            var methodInfo = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);


            if (methodInfo == null)
                throw new Exception($"Could not find method {methodName} on {type}");


            return methodInfo.MethodHandle.GetFunctionPointer();
        }
    }
}