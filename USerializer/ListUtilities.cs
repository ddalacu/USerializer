using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;


namespace USerialization
{
    public static class ArrayHelpers
    {
        public static unsafe void Clear<T>(T[] array, int start, int count) where T : unmanaged
        {
            fixed (void* addr = array)
            {
                Unsafe.InitBlock(((byte*)addr) + (start * sizeof(T)), 0, (uint)(count * sizeof(T)));
            }
        }

        public static unsafe void Clear(Array array, int start, int count, int elementSize)
        {
            var pinnable = Unsafe.As<Array, byte[]>(ref array);

            fixed (byte* addr = pinnable)
            {
                Unsafe.InitBlock(addr + (start * elementSize), 0, (uint)(count * elementSize));
            }
        }
    }
}