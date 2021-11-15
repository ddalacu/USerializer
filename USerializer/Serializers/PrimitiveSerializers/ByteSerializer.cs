﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(byte), typeof(ByteSerializer))]
[assembly: CustomSerializer(typeof(byte[]), typeof(ByteArraySerializer))]
[assembly: CustomSerializer(typeof(List<byte>), typeof(ByteListSerializer))]


namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class ByteSerializer : CustomDataSerializer
    {
        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out ByteDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void Write(void* fieldAddress, SerializerOutput output, object context)
        {
            var value = *(byte*) (fieldAddress);
            output.WriteByte(value);
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input, object context)
        {
            var value = (byte*) (fieldAddress);
            *value = input.ReadByte();
        }
    }

    public sealed class ByteDataTypeLogic : UnmanagedDataTypeLogic<byte>
    {
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class ByteArraySerializer : CustomDataSerializer
    {
        private DataType _elementDataType;


        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;
            if (typeLogic.TryGet(out ByteDataTypeLogic result) == false)
                return false;

            _elementDataType = result.Value;

            if (typeLogic.TryGet(out ArrayDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void Write(void* fieldAddress, SerializerOutput output, object context)
        {
            var array = Unsafe.Read<byte[]>(fieldAddress);
            if (array == null)
            {
                output.WriteNull();
                return;
            }

            var count = array.Length;

            if (count > 0)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    output.EnsureNext(6 + count);
                    output.Write7BitEncodedIntUnchecked(count);
                    output.WriteByteUnchecked((byte) _elementDataType);
                    output.WriteBytesUnchecked(array, count);
                }
                output.WriteSizeTrack(sizeTracker);
            }
            else
            {
                output.EnsureNext(5);
                output.WriteIntUnchecked(1); //size tracker
                output.WriteByteUnchecked(0);
            }
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input, object context)
        {
            ref var array = ref Unsafe.AsRef<byte[]>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();

                if (count > 0)
                {
                    var type = (DataType) input.ReadByte();

                    if (type == _elementDataType)
                    {
                        array = input.ReadBytes(count);
                    }
                    else
                    {
                        array = new byte[count];
                    }
                }
                else
                {
                    array = Array.Empty<byte>();
                }

                input.EndObject(end);
            }
            else
            {
                array = null;
            }
        }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class ByteListSerializer : CustomDataSerializer
    {
        private DataType _elementDataType;


        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;
            if (typeLogic.TryGet(out ByteDataTypeLogic result) == false)
                return false;

            _elementDataType = result.Value;

            if (typeLogic.TryGet(out ArrayDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void Write(void* fieldAddress, SerializerOutput output, object context)
        {
            var list = Unsafe.Read<List<byte>>(fieldAddress);
            if (list == null)
            {
                output.WriteNull();
                return;
            }

            var array = ListHelpers.GetArray(list, out var count);

            if (count > 0)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    output.EnsureNext(6 + count);
                    output.Write7BitEncodedIntUnchecked(count);
                    output.WriteByteUnchecked((byte) _elementDataType);
                    output.WriteBytesUnchecked(array, count);
                }
                output.WriteSizeTrack(sizeTracker);
            }
            else
            {
                output.EnsureNext(5);
                output.WriteIntUnchecked(1); //size tracker
                output.WriteByteUnchecked(0);
            }
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input, object context)
        {
            ref var list = ref Unsafe.AsRef<List<byte>>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();

                if (count > 0)
                {
                    var array = ListHelpers.PrepareArray(ref list, count);

                    var type = (DataType) input.ReadByte();

                    if (type == _elementDataType)
                    {
                        input.ReadBytes(array, count);
                    }
                    else
                    {
                        ArrayHelpers.CleanArray(array, 0, (uint) count);
                    }
                }
                else
                {
                    if (list == null)
                        list = new List<byte>();
                    else
                        ListHelpers.SetCount(list, 0);
                }

                input.EndObject(end);
            }
            else
            {
                list = null;
            }
        }
    }
}