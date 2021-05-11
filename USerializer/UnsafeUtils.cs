using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

[assembly:
    USerialization.LocalModuleInitialize(typeof(USerialization.UnsafeUtils),
        nameof(USerialization.UnsafeUtils.LocalInitialize))]


namespace USerialization
{
    public class PinnableObject
    {
        public byte Pinnable;
    }

    public static unsafe class UnsafeUtils
    {
        private static bool _isMono;

        internal static void LocalInitialize()
        {
            _isMono = Type.GetType("Mono.Runtime") != null;
        }

        public static int GetFieldOffset(FieldInfo fi)
        {
            if (fi == null)
                throw new ArgumentNullException(nameof(fi));

            var handle = fi.FieldHandle;

            if (_isMono)
            {
                MonoClassField* fd = (MonoClassField*) handle.Value;
                var offset = fd->Offset;
                offset -= 2 * sizeof(void*);
                return offset;
            }
            else
            {
                FieldDesc* fd = (FieldDesc*) handle.Value;
                return fd->Offset;
            }
        }

        public static int GetArrayElementSize(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsValueType)
            {
                var array = Array.CreateInstance(type, 2);
                var pinnable = Unsafe.As<Array, byte[]>(ref array);

                fixed (byte* address = pinnable)
                {
                    var elementOne = Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
                    var elementTwo = Marshal.UnsafeAddrOfPinnedArrayElement(array, 1);
                    return (int) (elementTwo.ToInt64() - elementOne.ToInt64());
                }
            }

            return sizeof(void*);
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MonoClassField
        {
            public void* Type;
            public void* Name;
            public void* Parent;
            public int Offset;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct FieldDesc
        {
            [FieldOffset(0)] private readonly void* m_pMTOfEnclosingClass;

            [FieldOffset(8)] private readonly uint m_dword1;

            [FieldOffset(12)] private readonly uint m_dword2;

            public int Offset => (int)(m_dword2 & 0x7FFFFFF);
        }

    }
}