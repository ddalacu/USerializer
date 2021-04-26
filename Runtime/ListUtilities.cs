using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

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

            _itemsFieldOffset = UnsafeUtility.GetFieldOffset(itemsMember);
            _sizeFieldOffset = UnsafeUtility.GetFieldOffset(sizeMember);
        }

        public static unsafe void SetArray<T>(List<T> list, T[] array)
        {
            byte* listAddress;
            UnsafeUtility.CopyObjectAddressToPtr(list, &listAddress);

            Unsafe.Write(listAddress + _itemsFieldOffset, array);
            Unsafe.Write(listAddress + _sizeFieldOffset, array.Length);
        }

        public static List<T> Create<T>(T[] array)
        {
            var newInstance = new List<T>();
            SetArray(newInstance, array);
            return newInstance;
        }

        public static unsafe T[] GetArray<T>(List<T> list, out int count)
        {
            byte* listAddress;
            UnsafeUtility.CopyObjectAddressToPtr(list, &listAddress);
            count = *(int*)(listAddress + _sizeFieldOffset);
            return Unsafe.Read<T[]>(listAddress + _itemsFieldOffset);
        }
        public static unsafe void SetArray(object list, Array array)
        {
            byte* listAddress;
            UnsafeUtility.CopyObjectAddressToPtr(list, &listAddress);

            Unsafe.Write(listAddress + _itemsFieldOffset, array);
            Unsafe.Write(listAddress + _sizeFieldOffset, array.Length);
        }

        public static unsafe void SetArray(object list, Array array, int count)
        {
            byte* listAddress;
            UnsafeUtility.CopyObjectAddressToPtr(list, &listAddress);

            Unsafe.Write(listAddress + _itemsFieldOffset, array);
            Unsafe.Write(listAddress + _sizeFieldOffset, count);
        }

        public static unsafe void SetCount(object list, int count)
        {
            byte* listAddress;
            UnsafeUtility.CopyObjectAddressToPtr(list, &listAddress);
            Unsafe.Write(listAddress + _sizeFieldOffset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Array GetArray(object list, out int count)
        {
            byte* listAddress;
            UnsafeUtility.CopyObjectAddressToPtr(list, &listAddress);

            count = *(int*)(listAddress + _sizeFieldOffset);
            return Unsafe.Read<Array>(listAddress + _itemsFieldOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe T[] GetArray<T>(object list, out int count)
        {
            byte* listAddress;
            UnsafeUtility.CopyObjectAddressToPtr(list, &listAddress);

            count = *(int*)(listAddress + _sizeFieldOffset);
            return Unsafe.Read<T[]>(listAddress + _itemsFieldOffset);
        }
    }
}