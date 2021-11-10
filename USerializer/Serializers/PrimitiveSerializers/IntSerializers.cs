using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(int), typeof(IntSerializer))]
[assembly: CustomSerializer(typeof(int[]), typeof(IntArraySerializer))]
[assembly: CustomSerializer(typeof(List<int>), typeof(IntListSerializer))]


namespace USerialization
{
    public sealed class Int32DataTypeLogic : UnmanagedDataTypeLogic<int>
    {
    }


    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class IntSerializer : CustomDataSerializer
    {
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

        protected override unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(int*) (fieldAddress);
            output.WriteInt(value);
        }

        protected override unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            var value = (int*) (fieldAddress);
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

        protected override unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var array = Unsafe.Read<int[]>(fieldAddress);
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
                    output.EnsureNext(6 + (count * sizeof(int)));
                    output.Write7BitEncodedIntUnchecked(count);
                    output.WriteByteUnchecked((byte) _elementDataType);

                    for (var i = 0; i < count; i++)
                        output.WriteIntUnchecked(array[i]);
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
            ref var array = ref Unsafe.AsRef<int[]>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();
                array = new int[count];

                if (count > 0)
                {
                    var type = (DataType) input.ReadByte();
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

        protected override unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var list = Unsafe.Read<List<int>>(fieldAddress);
            if (list == null)
            {
                output.WriteNull();
                return;
            }

            var count = list.Count;

            if (count > 0)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    output.EnsureNext(6 + (count * sizeof(int)));
                    output.Write7BitEncodedIntUnchecked(count);
                    output.WriteByteUnchecked((byte) _elementDataType);

                    for (var i = 0; i < count; i++)
                        output.WriteIntUnchecked(list[i]);
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
            ref var list = ref Unsafe.AsRef<List<int>>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();

                if (count > 0)
                {
                    var array = ListHelpers.PrepareArray(ref list, count);

                    var type = (DataType) input.ReadByte();

                    if (type == _elementDataType)
                    {
                        input.EnsureNext(count * sizeof(int));
                        for (var i = 0; i < count; i++)
                            array[i] = input.ReadIntUnchecked();
                    }
                    else
                    {
                        ArrayHelpers.CleanArray(array, 0, (uint) count);
                    }
                }
                else
                {
                    if (list == null)
                        list = new List<int>();
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