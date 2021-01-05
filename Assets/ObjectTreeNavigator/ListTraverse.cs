using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Traverse
{
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

        public unsafe Array GetArray(object list, out int count)
        {
            byte* listAddress;
            UnsafeUtility.CopyObjectAddressToPtr(list, &listAddress);

            count = *(int*)(listAddress + _sizeFieldOffset);
            var array = Unsafe.Read<Array>(listAddress + _itemsFieldOffset);
            return array;
        }

        public unsafe T[] GetArray<T>(object list, out int count)
        {
            byte* listAddress;
            UnsafeUtility.CopyObjectAddressToPtr(list, &listAddress);

            count = *(int*)(listAddress + _sizeFieldOffset);
            var array = Unsafe.Read<T[]>(listAddress + _itemsFieldOffset);
            return array;
        }
    }


    public unsafe class ListTraverse : IObjectTraverse
    {
        private ObjectTreeNavigator _navigator;

        public void Initialize(ObjectTreeNavigator serializer)
        {
            _navigator = serializer;
        }

        public void Start(ObjectTreeNavigator serializer)
        {

        }

        private static TraverseDelegate GetValueTypeWriter(TraverseDelegate elementWriter, Type fieldType, Type elementType)
        {
            var listHelper = ListHelper.Create(fieldType);
            var size = UnsafeUtility.SizeOf(elementType);

            void ValueTypeWriter(void* fieldAddress, TraverseContext context)
            {
                var list = Unsafe.Read<object>(fieldAddress);

                if (list == null)
                {
                    return;
                }

                var array = listHelper.GetArray(list, out var count);

                var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                for (var index = 0; index < count; index++)
                {
                    elementWriter(address, context);
                    address += size;
                }

                UnsafeUtility.ReleaseGCObject(handle);
            }

            return ValueTypeWriter;
        }

        private static TraverseDelegate GetReferenceTypeWriter(TraverseDelegate elementWriter, Type fieldType)
        {
            var listHelper = ListHelper.Create(fieldType);

            return delegate (void* fieldAddress, TraverseContext context)
            {
                var list = Unsafe.Read<IList>(fieldAddress);

                if (list == null)
                {
                    return;
                }

                var array = listHelper.GetArray<object>(list, out var count);

                var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                for (var index = 0; index < count; index++)
                {
                    var o = array[index];
                    if (o != null)
                    {
                        elementWriter(address, context);
                    }
                    else
                    {

                    }

                    address += sizeof(void*);
                }

                UnsafeUtility.ReleaseGCObject(handle);
            };
        }

        public bool TryGetTraverseMethod(Type fieldType, out TraverseDelegate serializationMethods)
        {

            if (fieldType.IsConstructedGenericType == false)
            {
                serializationMethods = default;
                return false;
            }

            if (fieldType.GetGenericTypeDefinition() != typeof(List<>))
            {
                serializationMethods = default;
                return false;
            }

            var elementType = fieldType.GetGenericArguments()[0];

            if (_navigator.TryGetSerializationMethods(elementType, out var elementSerializationMethods) == false)
            {
                serializationMethods = default;
                return false;
            }

            if (elementType.IsValueType)
            {
                serializationMethods = GetValueTypeWriter(elementSerializationMethods, fieldType, elementType);
            }
            else
            {
                serializationMethods = GetReferenceTypeWriter(elementSerializationMethods, fieldType);
            }

            return true;
        }
    }
}