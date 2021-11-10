using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;


[assembly:
    USerialization.LocalModuleInitialize(typeof(USerialization.ListHelpers),
        nameof(USerialization.ListHelpers.LocalInitialize))]

namespace USerialization
{
    public static class ListHelpers
    {
        private static int _itemsFieldOffset;
        private static int _sizeFieldOffset;

        internal static void LocalInitialize()
        {
            var listType = typeof(List<object>);
            var itemsMember = listType.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
            var sizeMember = listType.GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic);

            _itemsFieldOffset = UnsafeUtils.GetFieldOffset(itemsMember);
            _sizeFieldOffset = UnsafeUtils.GetFieldOffset(sizeMember);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static List<T> Create<T>(T[] array)
        {
            var newInstance = new List<T>();
            SetArray(newInstance, array);
            return newInstance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T[] GetArray<T>(List<T> list, out int count)
        {
            var pinnable = Unsafe.As<List<T>, PinnableObject>(ref list);
            fixed (byte* listAddress = &pinnable.Pinnable)
            {
                count = *(int*) (listAddress + _sizeFieldOffset);
                return Unsafe.Read<T[]>(listAddress + _itemsFieldOffset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] PrepareArray<T>(ref List<T> list, int count)
        {
            T[] array;

            if (list == null)
            {
                list = new List<T>();
                array = new T[count];
                SetArray(list, array, count);
            }
            else
            {
                array = GetArray(list, out _);

                if (array.Length < count)
                {
                    array = new T[count];
                    SetArray(list, array, count);
                }
                else
                {
                    SetCount(list, count);
                }
            }

            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void SetArray(object list, Array array)
        {
            var pinnable = Unsafe.As<object, PinnableObject>(ref list);
            fixed (byte* listAddress = &pinnable.Pinnable)
            {
                Unsafe.Write(listAddress + _itemsFieldOffset, array);
                Unsafe.Write(listAddress + _sizeFieldOffset, array.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void SetArray(object list, Array array, int count)
        {
            var pinnable = Unsafe.As<object, PinnableObject>(ref list);

            fixed (byte* listAddress = &pinnable.Pinnable)
            {
                Unsafe.Write(listAddress + _itemsFieldOffset, array);
                Unsafe.Write(listAddress + _sizeFieldOffset, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void SetCount(object list, int count)
        {
            var pinnable = Unsafe.As<object, PinnableObject>(ref list);
            fixed (byte* listAddress = &pinnable.Pinnable)
            {
                Unsafe.Write(listAddress + _sizeFieldOffset, count);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Array GetArray(object list, out int count)
        {
            var pinnable = Unsafe.As<object, PinnableObject>(ref list);
            fixed (byte* listAddress = &pinnable.Pinnable)
            {
                count = *(int*) (listAddress + _sizeFieldOffset);
                return Unsafe.Read<Array>(listAddress + _itemsFieldOffset);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T[] GetArray<T>(object list, out int count)
        {
            var pinnable = Unsafe.As<object, PinnableObject>(ref list);
            fixed (byte* listAddress = &pinnable.Pinnable)
            {
                count = *(int*) (listAddress + _sizeFieldOffset);
                return Unsafe.Read<T[]>(listAddress + _itemsFieldOffset);
            }
        }
    }

    public static class ArrayHelpers
    {
        public static unsafe void CleanArray<T>(T[] array, uint start, uint count) where T : unmanaged
        {
            fixed (void* addr = array)
            {
                Unsafe.InitBlock(((byte*) addr) + (start * sizeof(T)), 0, (uint) (count * sizeof(T)));
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