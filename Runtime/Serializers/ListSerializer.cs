﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public unsafe class ListSerializer : ISerializationProvider
    {
        private USerializer _serializer;

        public void Initialize(USerializer serializer)
        {
            _serializer = serializer;
        }

        public void Start(USerializer serializer)
        {

        }

        private static WriteDelegate GetValueTypeWriter(WriteDelegate elementWriter, Type fieldType, Type elementType, DataType dataType)
        {
            var listHelper = ListHelper.Create(fieldType);
            var size = UnsafeUtility.SizeOf(elementType);

            void ValueTypeWriter(void* fieldAddress, SerializerOutput output)
            {
                var list = Unsafe.Read<object>(fieldAddress);

                if (list == null)
                {
                    output.WriteNull();
                    return;
                }

                var array = listHelper.GetArray(list, out var count);

                var sizeTracker = output.BeginSizeTrack();
                {
                    output.EnsureNext(6);
                    output.WriteByteUnchecked((byte)dataType);
                    output.Write7BitEncodedIntUnchecked(count);

                    var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                    for (var index = 0; index < count; index++)
                    {
                        elementWriter(address, output);
                        address += size;
                    }

                    UnsafeUtility.ReleaseGCObject(handle);
                }

                output.WriteSizeTrack(sizeTracker);
            }

            return ValueTypeWriter;
        }

        private static ReadDelegate GetValueTypeReader(ReadDelegate elementReader, Type fieldType, Type elementType, DataType dataType)
        {
            var listHelper = ListHelper.Create(fieldType);
            var size = UnsafeUtility.SizeOf(elementType);

            return delegate (void* fieldAddress, SerializerInput input)
            {
                ref var list = ref Unsafe.AsRef<object>(fieldAddress);

                if (input.BeginReadSize(out var end))
                {
                    var type = (DataType)input.ReadByte();
                    var count = input.Read7BitEncodedInt();

                    Array array;

                    if (list == null)
                    {
                        list = FormatterServices.GetUninitializedObject(fieldType);
                        array = Array.CreateInstance(elementType, count);
                        listHelper.SetArray(list, array, count);
                    }
                    else
                    {
                        array = listHelper.GetArray(list, out var currentCount);

                        if (currentCount < count)//if we need more elements in the array then we allocate a array
                        {
                            array = Array.CreateInstance(elementType, count);
                            listHelper.SetArray(list, array, count);
                        }
                        else
                        {
                            if (currentCount != count)//if we need less elements in the array then we clear the items we don't need
                            {
                                var remaining = currentCount - count;
                                Array.Clear(array, count, remaining);
                                listHelper.SetCount(list, count);
                            }
                        }
                    }

                    if (type == dataType)
                    {
                        var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                        for (int i = 0; i < count; i++)
                        {
                            elementReader(address, input);
                            address += size;
                        }

                        UnsafeUtility.ReleaseGCObject(handle);
                    }

                    input.EndObject(end);
                }
                else
                {
                    list = null;
                }
            };
        }

        private static WriteDelegate GetReferenceTypeWriter(WriteDelegate elementWriter, Type fieldType, DataType dataType)
        {
            var listHelper = ListHelper.Create(fieldType);

            return delegate (void* fieldAddress, SerializerOutput output)
            {
                var list = Unsafe.Read<IList>(fieldAddress);

                if (list == null)
                {
                    output.WriteNull();
                    return;
                }

                var array = listHelper.GetArray(list, out var count);

                var sizeTracker = output.BeginSizeTrack();
                {
                    output.EnsureNext(6);
                    output.WriteByteUnchecked((byte)dataType);
                    output.Write7BitEncodedIntUnchecked(count);

                    var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                    for (var index = 0; index < count; index++)
                    {
                        elementWriter(address, output);
                        address += sizeof(void*);
                    }

                    UnsafeUtility.ReleaseGCObject(handle);
                }
                output.WriteSizeTrack(sizeTracker);
            };
        }

        private static ReadDelegate GetReferenceTypeReader(ReadDelegate elementReader, Type fieldType, Type elementType, DataType dataType)
        {
            var listHelper = ListHelper.Create(fieldType);

            return delegate (void* fieldAddress, SerializerInput input)
            {
                ref var list = ref Unsafe.AsRef<object>(fieldAddress);

                if (input.BeginReadSize(out var end))
                {
                    var type = (DataType)input.ReadByte();
                    var count = input.Read7BitEncodedInt();
                    Array array;

                    if (list == null)
                    {
                        list = FormatterServices.GetUninitializedObject(fieldType);
                        array = Array.CreateInstance(elementType, count);
                        listHelper.SetArray(list, array, count);
                    }
                    else
                    {
                        array = listHelper.GetArray(list, out var currentCount);

                        if (currentCount < count)//if we need more elements in the array then we allocate a array
                        {
                            array = Array.CreateInstance(elementType, count);
                            listHelper.SetArray(list, array, count);
                        }
                        else
                        {
                            if (currentCount != count)//if we need less elements in the array then we clear the items we don't need
                            {
                                var remaining = currentCount - count;
                                Array.Clear(array, count, remaining);
                                listHelper.SetCount(list, count);
                            }
                        }
                    }

                    if (type == dataType)
                    {
                        var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                        for (var i = 0; i < count; i++)
                        {
                            elementReader(address, input);
                            address += sizeof(void*);
                        }

                        UnsafeUtility.ReleaseGCObject(handle);
                    }

                    input.EndObject(end);
                }
                else
                {
                    list = null;
                }
            };
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public class ListDataSerializer : DataSerializer
        {
            private readonly Type _elementType;
            private readonly DataSerializer _elementSerializer;
            private ListHelper _listHelper;
            private int _size;
            private Type _fieldType;

            public ListDataSerializer(Type fieldType, Type elementType, DataSerializer elementSerializer) : base(DataType.Array)
            {
                _fieldType = fieldType;
                _elementType = elementType;
                _elementSerializer = elementSerializer;
                _listHelper = ListHelper.Create(fieldType);

                if (elementType.IsValueType)
                    _size = UnsafeUtility.SizeOf(elementType);
                else
                    _size = UnsafeUtility.SizeOf(typeof(IntPtr));
            }

            public override void WriteDelegate(void* fieldAddress, SerializerOutput output)
            {
                var list = Unsafe.Read<object>(fieldAddress);

                if (list == null)
                {
                    output.WriteNull();
                    return;
                }

                var array = _listHelper.GetArray(list, out var count);

                var sizeTracker = output.BeginSizeTrack();
                {
                    output.EnsureNext(6);
                    output.WriteByteUnchecked((byte)_elementSerializer.DataType);
                    output.Write7BitEncodedIntUnchecked(count);

                    var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                    for (var index = 0; index < count; index++)
                    {
                        _elementSerializer.WriteDelegate(address, output);
                        address += _size;
                    }

                    UnsafeUtility.ReleaseGCObject(handle);
                }

                output.WriteSizeTrack(sizeTracker);
            }

            public override void ReadDelegate(void* fieldAddress, SerializerInput input)
            {
                ref var list = ref Unsafe.AsRef<object>(fieldAddress);

                if (input.BeginReadSize(out var end))
                {
                    var type = (DataType)input.ReadByte();
                    var count = input.Read7BitEncodedInt();

                    Array array;

                    if (list == null)
                    {
                        list = FormatterServices.GetUninitializedObject(_fieldType);
                        array = Array.CreateInstance(_elementType, count);
                        _listHelper.SetArray(list, array, count);
                    }
                    else
                    {
                        array = _listHelper.GetArray(list, out var currentCount);

                        if (currentCount < count)//if we need more elements in the array then we allocate a array
                        {
                            array = Array.CreateInstance(_elementType, count);
                            _listHelper.SetArray(list, array, count);
                        }
                        else
                        {
                            if (currentCount != count)//if we need less elements in the array then we clear the items we don't need
                            {
                                var remaining = currentCount - count;
                                Array.Clear(array, count, remaining);
                                _listHelper.SetCount(list, count);
                            }
                        }
                    }

                    if (type == _elementSerializer.DataType)
                    {
                        var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                        for (int i = 0; i < count; i++)
                        {
                            _elementSerializer.ReadDelegate(address, input);
                            address += _size;
                        }

                        UnsafeUtility.ReleaseGCObject(handle);
                    }

                    input.EndObject(end);
                }
                else
                {
                    list = null;
                }
            }
        }

        public bool TryGetSerializationMethods(Type type, out DataSerializer serializationMethods)
        {

            if (type.IsConstructedGenericType == false)
            {
                serializationMethods = default;
                return false;
            }

            if (type.GetGenericTypeDefinition() != typeof(List<>))
            {
                serializationMethods = default;
                return false;
            }

            var elementType = type.GetGenericArguments()[0];

            if (_serializer.TryGetSerializationMethods(elementType, out var elementDataSerializer) == false)
            {
                serializationMethods = default;
                return false;
            }

            serializationMethods = new ListDataSerializer(type, elementType, elementDataSerializer);
            return true;
        }
    }
}