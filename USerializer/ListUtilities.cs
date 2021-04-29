using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace USerialization
{
    public static class ListHelpers
    {
        private static readonly int _itemsFieldOffset;
        private static readonly int _sizeFieldOffset;

        static ListHelpers()
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
}