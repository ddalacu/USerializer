using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace USerialization
{
    public unsafe class NETRuntimeUtils : IRuntimeUtils
    {
        public NETRuntimeUtils()
        {
            var sizeOfTest = GetClassHeapSizeInternal(typeof(PinnableObject));
            if (sizeOfTest != sizeof(void*))
                throw new InvalidOperationException("Incompatible runtime detected.");
        }

        public int GetFieldOffset(FieldInfo fi)
        {
            if (fi == null)
                throw new ArgumentNullException(nameof(fi));

            var handle = fi.FieldHandle;
            var fd = (FieldDesc*)handle.Value;
            return fd->Offset;
        }

        public int GetStackSize(Type type)
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

        [StructLayout(LayoutKind.Explicit)]
        public struct FieldDesc
        {
            [FieldOffset(0)]
            private readonly void* m_pMTOfEnclosingClass;

            [FieldOffset(8)]
            private readonly uint m_dword1;

            [FieldOffset(12)]
            private readonly uint m_dword2;

            public int Offset => (int)(m_dword2 & 0x7FFFFFF);
        }

        public int HeaderSize() => sizeof(void*) * 2;

        public int GetClassHeapSize(Type type)
        {
            return GetClassHeapSizeInternal(type);
        }

        private static int GetClassHeapSizeInternal(Type type)
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
            return baseSize - (sizeof(void*) * 2);
        }
    }
}