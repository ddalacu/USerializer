using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace USerialization
{
    public class PinnableObject
    {
        public byte Pinnable;
    }

    /// <summary>
    /// this class is VERY hacky/unsafe but its ok
    /// </summary>
    public static unsafe class UnsafeUtils
    {
        private static readonly bool _isMono;

        public static bool IsMono => _isMono;

        static UnsafeUtils()
        {
            _isMono = Type.GetType("Mono.Runtime") != null;

            if (_isMono == false)
            {
                var sizeOfTest = GetClassHeapSize(typeof(PinnableObject));

                if (sizeOfTest != sizeof(void*))
                    throw new InvalidOperationException();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public static int GetFieldOffset(FieldInfo fi)
        {
            if (fi == null)
                throw new ArgumentNullException(nameof(fi));

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

        public static int GetStackSize(Type type)
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
                    return (int)(elementTwo.ToInt64() - elementOne.ToInt64());
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

        public static int HeaderSize()
        {
            switch (sizeof(void*))
            {
                case 8:
                    return 16;
                case 4:
                    return 8;
                default:
                    throw new NotSupportedException("Unsupported pointer size");
            }
        }

        public static unsafe int GetClassHeapSize(Type type)
        {
            if (type.IsClass == false)
                throw new InvalidOperationException();
            if (type.IsInterface)
                throw new InvalidOperationException();
            if (type.IsAbstract)
                throw new InvalidOperationException();

            // Get the Method Table pointer (the "handle")
            IntPtr handle = type.TypeHandle.Value;

            // The BaseSize is stored at an offset within the Method Table.
            // On modern 64-bit .NET (Core/5+), this is typically at offset 4.
            // It represents the total size of the object on the heap in bytes.
            var baseSize = Unsafe.Read<int>((void*)(handle + 4));
            return baseSize - HeaderSize();
        }
    }
}