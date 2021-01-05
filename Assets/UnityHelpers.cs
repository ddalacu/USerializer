using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public static class UnityHelpers
{
    public static int _offsetOfInstanceId;
    public static int _cachedPtrOffset;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void AutoInit()
    {
        _offsetOfInstanceId = (int)typeof(Object)
            .GetMethod("GetOffsetOfInstanceIDInCPlusPlusObject", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new object[0]);

        _cachedPtrOffset =
            UnsafeUtility.GetFieldOffset(typeof(Object).GetField("m_CachedPtr",
                BindingFlags.Instance | BindingFlags.NonPublic));

    }


    public static unsafe int GetInstanceId(Object ob)
    {
        var obAddress = (byte*)(*(void**)Unsafe.AsPointer(ref ob));
        var fieldAddress = obAddress + _cachedPtrOffset;
        var cppAddress = *(void**)fieldAddress;
        if (cppAddress == null)
            return 0;
        var instanceId = (byte*)cppAddress + _offsetOfInstanceId;
        return *(int*)instanceId;
    }

}