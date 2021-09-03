using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(bool), typeof(BoolSerializer))]
[assembly: CustomSerializer(typeof(bool[]), typeof(BoolArraySerializer))]
[assembly: CustomSerializer(typeof(List<bool>), typeof(BoolListSerializer))]


namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class BoolSerializer : CustomDataSerializer
    {
        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out BooleanDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        protected override unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(byte*)(fieldAddress);
            output.WriteByte(value);
        }

        protected override unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            var value = (byte*)(fieldAddress);
            *value = input.ReadByte();
        }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class BoolArraySerializer : CustomDataSerializer
    {
        private DataType _elementDataType;


        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;
            if (typeLogic.TryGet(out BooleanDataTypeLogic result) == false)
                return false;

            _elementDataType = result.Value;

            if (typeLogic.TryGet(out ArrayDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        protected override unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var array = Unsafe.Read<bool[]>(fieldAddress);
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
                    output.EnsureNext(6 + (count * sizeof(bool)));
                    output.Write7BitEncodedIntUnchecked(count);
                    output.WriteByteUnchecked((byte)_elementDataType);
                    output.WriteBoolsUnchecked(array, count);
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

        protected override unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var array = ref Unsafe.AsRef<bool[]>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();

                if (count > 0)
                {
                    var type = (DataType)input.ReadByte();

                    if (type == _elementDataType)
                    {
                        array = input.ReadBools(count);
                    }
                    else
                    {
                        array = new bool[count];
                    }
                }
                else
                {
                    array = Array.Empty<bool>();
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
    public sealed class BoolListSerializer : CustomDataSerializer
    {
        private DataType _elementDataType;


        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;
            if (typeLogic.TryGet(out BooleanDataTypeLogic result) == false)
                return false;

            _elementDataType = result.Value;

            if (typeLogic.TryGet(out ArrayDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        protected override unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var list = Unsafe.Read<List<bool>>(fieldAddress);
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
                    output.EnsureNext(6 + (count * sizeof(bool)));

                    output.Write7BitEncodedIntUnchecked(count);
                    output.WriteByteUnchecked((byte)_elementDataType);
                    output.WriteBoolsUnchecked(array, count);
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

        protected override unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var list = ref Unsafe.AsRef<List<bool>>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();
                list = new List<bool>();

                if (count > 0)
                {
                    var type = (DataType)input.ReadByte();

                    if (type == _elementDataType)
                    {
                        ListHelpers.SetArray(list, input.ReadBools(count));
                    }
                    else
                    {
                        ListHelpers.SetArray(list, new bool[count]);
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

    public sealed class BooleanDataTypeLogic : UnmanagedDataTypeLogic<bool>
    {
    }
}