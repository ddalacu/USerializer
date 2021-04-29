using System;
using System.Reflection;
using System.Runtime.InteropServices;

public class PinnableObject
{
    public byte Pinnable;
}

public static class UnsafeUtils
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct MonoClassField
    {
        public void* Type;
        public void* Name;
        public void* Parent;
        public int Offset;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct FieldDesc
    {
        [FieldOffset(0)] private readonly void* m_pMTOfEnclosingClass;

        [FieldOffset(8)] private readonly uint m_dword1;

        [FieldOffset(12)] private readonly uint m_dword2;

        public int Offset => (int)(m_dword2 & 0x7FFFFFF);
    }

    private static bool _isMono = Type.GetType("Mono.Runtime") != null;

    public static unsafe int GetFieldOffset(FieldInfo fi)
    {
        var handle = fi.FieldHandle;

        if (_isMono)
        {
            MonoClassField* fd = (MonoClassField*)handle.Value;
            var offset = fd->Offset;
            offset -= 2 * sizeof(void*);
            return offset;
        }
        else
        {
            FieldDesc* fd = (FieldDesc*)handle.Value;
            return fd->Offset;
        }
    }

}