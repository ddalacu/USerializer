using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public readonly struct ObjectInternals//use this in custom collections accessing get instance id
{
    public readonly int OffsetOfInstanceId;
    public readonly int CachedPtrOffset;

    public ObjectInternals(int offsetOfInstanceId, int cachedPtrOffset)
    {
        OffsetOfInstanceId = offsetOfInstanceId;
        CachedPtrOffset = cachedPtrOffset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void* CachedPtr(Object ob)
    {
        var obAddress = (byte*)(*(void**)Unsafe.AsPointer(ref ob));
        var fieldAddress = obAddress + CachedPtrOffset;
        return *(void**)fieldAddress;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe int GetInstanceId(Object ob)
    {
        var cppAddress = CachedPtr(ob);
        if (cppAddress == null)
            return 0;
        var instanceId = (byte*)cppAddress + OffsetOfInstanceId;
        return *(int*)instanceId;
    }

    public static ObjectInternals Create()
    {
        var offsetOfInstanceId = (int)typeof(Object)
            .GetMethod("GetOffsetOfInstanceIDInCPlusPlusObject", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new object[0]);

        var cachedPtrOffset =
            UnsafeUtility.GetFieldOffset(typeof(Object).GetField("m_CachedPtr",
                BindingFlags.Instance | BindingFlags.NonPublic));

        return new ObjectInternals(offsetOfInstanceId, cachedPtrOffset);
    }
}