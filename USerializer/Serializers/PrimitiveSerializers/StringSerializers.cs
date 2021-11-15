using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(string), typeof(StringSerializer))]
[assembly: CustomSerializer(typeof(string[]), typeof(StringArraySerializer))]
[assembly: CustomSerializer(typeof(List<string>), typeof(StringListSerializer))]


namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class StringSerializer : CustomDataSerializer
    {
        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out StringDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void Write(void* fieldAddress, SerializerOutput output, object context)
        {
            var obj = Unsafe.Read<string>(fieldAddress);
            output.WriteString(obj);
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input, object context)
        {
            ref var value = ref Unsafe.AsRef<string>(fieldAddress);
            value = input.ReadString();
        }
    }

    public sealed class StringDataTypeLogic : IDataTypeLogic
    {
        public DataType Value { get; set; }

        public void Skip(SerializerInput input)
        {
            var chars = input.Read7BitEncodedInt();

            chars -= 1;

            if (chars == -1) //null
                return;

            input.Skip(chars * sizeof(char));
        }
    }


    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class StringArraySerializer : CustomDataSerializer
    {
        private DataType _elementDataType;

        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;
            if (typeLogic.TryGet(out StringDataTypeLogic result) == false)
                return false;

            _elementDataType = result.Value;

            if (typeLogic.TryGet(out ArrayDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void Write(void* fieldAddress, SerializerOutput output, object context)
        {
            var array = Unsafe.Read<string[]>(fieldAddress);
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
                    output.EnsureNext(6);
                    output.Write7BitEncodedIntUnchecked(count);
                    output.WriteByteUnchecked((byte) _elementDataType);

                    for (var i = 0; i < count; i++)
                        output.WriteString(array[i]);
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
            ref var array = ref Unsafe.AsRef<string[]>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();

                if (count > 0)
                {
                    var type = (DataType) input.ReadByte();
                    array = new string[count];

                    if (type == _elementDataType)
                    {
                        for (var i = 0; i < count; i++)
                            array[i] = input.ReadString();
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
    public sealed class StringListSerializer : CustomDataSerializer
    {
        private DataType _elementDataType;

        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;
            if (typeLogic.TryGet(out StringDataTypeLogic result) == false)
                return false;

            _elementDataType = result.Value;

            if (typeLogic.TryGet(out ArrayDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void Write(void* fieldAddress, SerializerOutput output, object context)
        {
            var list = Unsafe.Read<List<string>>(fieldAddress);
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
                    output.EnsureNext(6);
                    output.Write7BitEncodedIntUnchecked(count);

                    output.WriteByteUnchecked((byte) _elementDataType);

                    for (var i = 0; i < count; i++)
                        output.WriteString(list[i]);
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
            ref var list = ref Unsafe.AsRef<List<string>>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();

                if (count > 0)
                {
                    var array = ListHelpers.PrepareArray(ref list, count);

                    var type = (DataType) input.ReadByte();

                    if (type == _elementDataType)
                    {
                        for (var i = 0; i < count; i++)
                            array[i] = input.ReadString();
                    }
                    else
                    {
                        for (var i = 0; i < count; i++)
                            array[i] = default;
                    }
                }
                else
                {
                    if (list == null)
                        list = new List<string>();
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