using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(IntSerializer))]
[assembly: CustomSerializer(typeof(IntArraySerializer))]
[assembly: CustomSerializer(typeof(IntListSerializer))]


namespace USerialization
{
    public sealed class Int32DataTypeLogic : UnmanagedDataTypeLogic<int>
    {

    }


    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class IntSerializer : CustomDataSerializer
    {
        public override Type SerializedType => typeof(int);

        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out Int32DataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var value = *(int*)(fieldAddress);
            output.WriteInt(value);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            var value = (int*)(fieldAddress);
            *value = input.ReadInt();
        }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class IntArraySerializer : CustomDataSerializer
    {
        private DataType _elementDataType;
        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override Type SerializedType => typeof(int[]);

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out Int32DataTypeLogic result) == false)
                return false;

            _elementDataType = result.Value;

            if (typeLogic.TryGet(out ArrayDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;

            return true;
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var array = Unsafe.Read<int[]>(fieldAddress);
            if (array != null)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    var count = array.Length;

                    if (count > 0)
                    {
                        output.EnsureNext(6 + (count * sizeof(int)));
                        output.Write7BitEncodedIntUnchecked(count);
                        output.WriteByteUnchecked((byte)_elementDataType);

                        for (var i = 0; i < count; i++)
                            output.WriteIntUnchecked(array[i]);
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

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            ref var array = ref Unsafe.AsRef<int[]>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();
                array = new int[count];

                if (count > 0)
                {
                    var type = (DataType)input.ReadByte();
                    if (type == _elementDataType)
                    {
                        for (var i = 0; i < count; i++)
                            array[i] = input.ReadInt();
                    }
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
    public sealed class IntListSerializer : CustomDataSerializer
    {
        public override Type SerializedType => typeof(List<int>);

        private DataType _elementDataType;

        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out Int32DataTypeLogic result) == false)
                return false;

            _elementDataType = result.Value;

            if (typeLogic.TryGet(out ArrayDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;

            return true;
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var list = Unsafe.Read<List<int>>(fieldAddress);
            if (list == null)
            {
                output.WriteNull();
                return;
            }

            var sizeTracker = output.BeginSizeTrack();
            {
                var count = list.Count;

                if (count > 0)
                {
                    output.EnsureNext(6 + (count * sizeof(int)));
                    output.Write7BitEncodedIntUnchecked(count);
                    output.WriteByteUnchecked((byte)_elementDataType);

                    for (var i = 0; i < count; i++)
                        output.WriteIntUnchecked(list[i]);
                }
                else
                {
                    output.WriteByte(0);
                }
            }

            output.WriteSizeTrack(sizeTracker);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            ref var list = ref Unsafe.AsRef<List<int>>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();
                list = new List<int>();

                if (count > 0)
                {
                    var array = new int[count];
                    ListHelpers.SetArray(list, array);

                    var type = (DataType) input.ReadByte();

                    if (type == _elementDataType)
                    {
                        input.EnsureNext(count * sizeof(int));

                        for (var i = 0; i < count; i++)
                            array[i] = input.ReadIntUnchecked();
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