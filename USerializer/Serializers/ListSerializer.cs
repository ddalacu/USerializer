﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Unity.IL2CPP.CompilerServices;


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

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public class ListDataSerializer : DataSerializer
        {
            private readonly Type _elementType;
            private readonly DataSerializer _elementSerializer;
            private int _size;
            private Type _fieldType;

            private DataType _dataType;

            public override DataType GetDataType() => _dataType;

            public ListDataSerializer(Type fieldType, Type elementType, DataSerializer elementSerializer, USerializer serializer)
            {
                _fieldType = fieldType;
                _elementType = elementType;
                _elementSerializer = elementSerializer;


                if (elementType.IsValueType)
                {
                    var array = Array.CreateInstance(elementType, 2);
                    var pinnable = Unsafe.As<Array, byte[]>(ref array);

                    fixed (byte* address = pinnable)
                    {
                        var elementOne = Marshal.UnsafeAddrOfPinnedArrayElement(array, 0);
                        var elementTwo = Marshal.UnsafeAddrOfPinnedArrayElement(array, 1);
                        _size = (int)(elementTwo.ToInt64() - elementOne.ToInt64());
                    }
                }
                else
                    _size = sizeof(void*);

                if (serializer.DataTypesDatabase.TryGet(out ArrayDataTypeLogic arrayDataTypeLogic))
                    _dataType = arrayDataTypeLogic.Value;
            }

            public override void WriteDelegate(void* fieldAddress, SerializerOutput output)
            {
                var list = Unsafe.Read<object>(fieldAddress);

                if (list == null)
                {
                    output.WriteNull();
                    return;
                }

                var array = ListHelpers.GetArray(list, out var count);

                var sizeTracker = output.BeginSizeTrack();
                {
                    if (count > 0)
                    {
                        output.EnsureNext(6);
                        output.Write7BitEncodedIntUnchecked(count);
                        output.WriteByteUnchecked((byte)_elementSerializer.GetDataType());

                        var pinnable = Unsafe.As<Array, byte[]>(ref array);

                        fixed (byte* address = pinnable)
                        {
                            var tempAddress = address;

                            for (var index = 0; index < count; index++)
                            {
                                _elementSerializer.WriteDelegate(tempAddress, output);
                                tempAddress += _size;
                            }
                        }
                    }
                    else
                    {
                        output.WriteByte(0);
                    }
                }

                output.WriteSizeTrack(sizeTracker);
            }

            public override void ReadDelegate(void* fieldAddress, SerializerInput input)
            {
                ref var list = ref Unsafe.AsRef<object>(fieldAddress);

                if (input.BeginReadSize(out var end))
                {
                    var count = input.Read7BitEncodedInt();

                    Array array;

                    if (list == null)
                    {
                        list = FormatterServices.GetUninitializedObject(_fieldType);
                        array = Array.CreateInstance(_elementType, count);
                        ListHelpers.SetArray(list, array, count);
                    }
                    else
                    {
                        array = ListHelpers.GetArray(list, out var currentCount);

                        if (currentCount < count)//if we need more elements in the array then we allocate a array
                        {
                            array = Array.CreateInstance(_elementType, count);
                            ListHelpers.SetArray(list, array, count);
                        }
                        else
                        {
                            if (currentCount != count)//if we need less elements in the array then we clear the items we don't need
                            {
                                var remaining = currentCount - count;
                                Array.Clear(array, count, remaining);
                                ListHelpers.SetCount(list, count);
                            }
                        }
                    }

                    if (count > 0)
                    {
                        var type = (DataType)input.ReadByte();
                        if (type == _elementSerializer.GetDataType())
                        {
                            var pinnable = Unsafe.As<Array, byte[]>(ref array);

                            fixed (byte* address = pinnable)
                            {
                                var tempAddress = address;

                                for (var i = 0; i < count; i++)
                                {
                                    _elementSerializer.ReadDelegate(tempAddress, input);
                                    tempAddress += _size;
                                }
                            }
                        }
                    }

                    input.EndObject(end);
                }
                else
                {
                    list = null;
                }
            }
        }

        public bool TryGet(Type type, out DataSerializer serializationMethods)
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

            if (_serializer.TryGetDataSerializer(elementType, out var elementDataSerializer) == false)
            {
                serializationMethods = default;
                return false;
            }

            serializationMethods = new ListDataSerializer(type, elementType, elementDataSerializer, _serializer);
            return true;
        }
    }
}