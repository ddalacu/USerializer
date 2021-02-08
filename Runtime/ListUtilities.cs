using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace USerialization
{
    public readonly struct ListHelper<T>
    {
        private readonly int _itemsFieldOffset;
        private readonly int _sizeFieldOffset;

        private ListHelper(int itemsFieldOffset, int sizeFieldOffset)
        {
            _itemsFieldOffset = itemsFieldOffset;
            _sizeFieldOffset = sizeFieldOffset;
        }

        public static ListHelper<T> Create()
        {
            var listType = typeof(List<T>);
            var itemsMember = listType.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
            var sizeMember = listType.GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic);

            var itemsFieldOffset = UnsafeUtility.GetFieldOffset(itemsMember);
            var sizeFieldOffset = UnsafeUtility.GetFieldOffset(sizeMember);

            return new ListHelper<T>(itemsFieldOffset, sizeFieldOffset);
        }

        public unsafe void SetArray(List<T> list, T[] array)
        {
            byte* listAddress;
            UnsafeUtility.CopyObjectAddressToPtr(list, &listAddress);

            Unsafe.Write(listAddress + _itemsFieldOffset, array);
            Unsafe.Write(listAddress + _sizeFieldOffset, array.Length);
        }

        public List<T> Create(T[] array)
        {
            var newInstance = new List<T>();
            SetArray(newInstance, array);
            return newInstance;
        }

        public unsafe T[] GetArray(List<T> list, out int count)
        {
            byte* listAddress;
            UnsafeUtility.CopyObjectAddressToPtr(list, &listAddress);
            count = *(int*)(listAddress + _sizeFieldOffset);
            return Unsafe.Read<T[]>(listAddress + _itemsFieldOffset);
        }
    }

    public readonly struct ListHelper
    {
        private readonly int _itemsFieldOffset;
        private readonly int _sizeFieldOffset;

        private ListHelper(int itemsFieldOffset, int sizeFieldOffset)
        {
            _itemsFieldOffset = itemsFieldOffset;
            _sizeFieldOffset = sizeFieldOffset;
        }

        public static ListHelper Create(Type listType)
        {
            var itemsMember = listType.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
            var sizeMember = listType.GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic);

            var itemsFieldOffset = UnsafeUtility.GetFieldOffset(itemsMember);
            var sizeFieldOffset = UnsafeUtility.GetFieldOffset(sizeMember);

            return new ListHelper(itemsFieldOffset, sizeFieldOffset);
        }

        public unsafe void SetArray(object list, Array array)
        {
            byte* listAddress;
            UnsafeUtility.CopyObjectAddressToPtr(list, &listAddress);

            Unsafe.Write(listAddress + _itemsFieldOffset, array);
            Unsafe.Write(listAddress + _sizeFieldOffset, array.Length);
        }

        public unsafe void SetArray(object list, Array array, int count)
        {
            byte* listAddress;
            UnsafeUtility.CopyObjectAddressToPtr(list, &listAddress);

            Unsafe.Write(listAddress + _itemsFieldOffset, array);
            Unsafe.Write(listAddress + _sizeFieldOffset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Array GetArray(object list, out int count)
        {
            byte* listAddress;
            UnsafeUtility.CopyObjectAddressToPtr(list, &listAddress);

            count = *(int*)(listAddress + _sizeFieldOffset);
            return Unsafe.Read<Array>(listAddress + _itemsFieldOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T[] GetArray<T>(object list, out int count)
        {
            byte* listAddress;
            UnsafeUtility.CopyObjectAddressToPtr(list, &listAddress);

            count = *(int*)(listAddress + _sizeFieldOffset);
            return Unsafe.Read<T[]>(listAddress + _itemsFieldOffset);
        }
    }


    public static class ListUtilities
    {
        public static void GetListOffsets(Type listType, out int itemsFieldOffset, out int sizeFieldOffset)
        {
            var itemsMember = listType.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
            var sizeMember = listType.GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic);

            itemsFieldOffset = UnsafeUtility.GetFieldOffset(itemsMember);
            sizeFieldOffset = UnsafeUtility.GetFieldOffset(sizeMember);
        }
    }
}