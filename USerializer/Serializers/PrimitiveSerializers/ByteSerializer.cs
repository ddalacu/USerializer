using System;
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

        public override unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(byte*)(fieldAddress);
            output.WriteByte(value);
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            var value = (byte*)(fieldAddress);
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

        public override unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var array = Unsafe.Read<byte[]>(fieldAddress);
            if (array != null)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    var count = array.Length;
                    if (count > 0)
                    {
                        output.EnsureNext(6 + (count * sizeof(byte)));
                        output.Write7BitEncodedIntUnchecked(count);
                        output.WriteByteUnchecked((byte)_elementDataType);
                        output.WriteBytesUnchecked(array, count);
                    }
                    else
                    {
                        output.WriteByte(0);
                    }
                }
                output.WriteSizeTrack(sizeTracker);
            }
            else
            {
                output.WriteNull();
            }
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var array = ref Unsafe.AsRef<byte[]>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();

                if (count > 0)
                {
                    var type = (DataType)input.ReadByte();

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

        public override unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var list = Unsafe.Read<List<byte>>(fieldAddress);
            if (list == null)
            {
                output.WriteNull();
                return;
            }

            var sizeTracker = output.BeginSizeTrack();
            {
                var array = ListHelpers.GetArray(list, out var count);

                if (count > 0)
                {
                    output.EnsureNext(6 + (count * sizeof(byte)));

                    output.Write7BitEncodedIntUnchecked(count);
                    output.WriteByteUnchecked((byte)_elementDataType);
                    output.WriteBytesUnchecked(array, count);
                }
                else
                {
                    output.WriteByte(0);
                }
            }

            output.WriteSizeTrack(sizeTracker);
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var list = ref Unsafe.AsRef<List<byte>>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();
                list = new List<byte>();

                if (count > 0)
                {
                    var type = (DataType)input.ReadByte();

                    if (type == _elementDataType)
                    {
                        ListHelpers.SetArray(list, input.ReadBytes(count));
                    }
                    else
                    {
                        ListHelpers.SetArray(list, new byte[count]);
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
}
