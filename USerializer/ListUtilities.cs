using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;


namespace USerialization
{
    public static class ListHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> Create<T>(T[] array, int itemsOffset, int sizeOffset)
        {
            var newInstance = new List<T>();
            SetArray(newInstance, array, itemsOffset, sizeOffset);
            return newInstance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T[] GetArray<T>(List<T> list, int itemsOffset, int sizeOffset, out int count)
        {
            var pinnable = Unsafe.As<List<T>, PinnableObject>(ref list);
            fixed (byte* listAddress = &pinnable.Pinnable)
            {
                count = Unsafe.Read<int>(listAddress + sizeOffset);
                return Unsafe.Read<T[]>(listAddress + itemsOffset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] PrepareArray<T>(ref List<T> list, int count, int itemsOffset, int sizeOffset)
        {
            T[] array;

            if (list == null)
            {
                list = new List<T>();
                array = new T[count];
                SetArray(list, array, count, itemsOffset, sizeOffset);
            }
            else
            {
                array = GetArray(list, itemsOffset, sizeOffset, out _);

                if (array.Length < count)
                {
                    array = new T[count];
                    SetArray(list, array, count, itemsOffset, sizeOffset);
                }
                else
                {
                    SetCount(list, count, sizeOffset);
                }
            }

            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void SetArray(object list, Array array, int itemsOffset, int sizeOffset)
        {
            var pinnable = Unsafe.As<object, PinnableObject>(ref list);
            fixed (byte* listAddress = &pinnable.Pinnable)
            {
                Unsafe.WriteUnaligned(listAddress + itemsOffset, array);
                Unsafe.WriteUnaligned(listAddress + sizeOffset, array.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void SetArray(object list, Array array, int count, int itemsOffset, int sizeOffset)
        {
            var pinnable = Unsafe.As<object, PinnableObject>(ref list);

            fixed (byte* listAddress = &pinnable.Pinnable)
            {
                Unsafe.WriteUnaligned(listAddress + itemsOffset, array);
                Unsafe.WriteUnaligned(listAddress + sizeOffset, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void SetCount(object list, int count, int sizeOffset)
        {
            var pinnable = Unsafe.As<object, PinnableObject>(ref list);
            fixed (byte* listAddress = &pinnable.Pinnable)
            {
                Unsafe.WriteUnaligned(listAddress + sizeOffset, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Array GetArray(object list, int itemsOffset, int sizeOffset, out int count)
        {
            var pinnable = Unsafe.As<object, PinnableObject>(ref list);
            fixed (byte* listAddress = &pinnable.Pinnable)
            {
                count = Unsafe.Read<int>(listAddress + sizeOffset);
                return Unsafe.Read<Array>(listAddress + itemsOffset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T[] GetArray<T>(object list, int itemsOffset, int sizeOffset, out int count)
        {
            var pinnable = Unsafe.As<object, PinnableObject>(ref list);
            fixed (byte* listAddress = &pinnable.Pinnable)
            {
                count = Unsafe.Read<int>(listAddress + sizeOffset);
                return Unsafe.Read<T[]>(listAddress + itemsOffset);
            }
        }
    }

    public static class ArrayHelpers
    {
        public static unsafe void CleanArray<T>(T[] array, uint start, uint count) where T : unmanaged
        {
            fixed (void* addr = array)
            {
                Unsafe.InitBlock(((byte*)addr) + (start * sizeof(T)), 0, (uint)(count * sizeof(T)));
            }
        }

        public static unsafe void CleanArray(Array array, uint start, uint count, uint elementSize)
        {
            var pinnable = Unsafe.As<Array, byte[]>(ref array);

            fixed (byte* addr = pinnable)
            {
                Unsafe.InitBlock(addr + (start * elementSize), 0, (count * elementSize));
            }
        }
    }
}