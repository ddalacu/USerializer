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

        public override unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var obj = Unsafe.Read<string>(fieldAddress);
            output.WriteString(obj);
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input)
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

            if (chars == -1)//null
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

        public override unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var array = Unsafe.Read<string[]>(fieldAddress);
            if (array != null)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    var count = array.Length;

                    if (count > 0)
                    {
                        output.EnsureNext(6);
                        output.Write7BitEncodedIntUnchecked(count);
                        output.WriteByteUnchecked((byte)_elementDataType);

                        for (var i = 0; i < count; i++)
                            output.WriteString(array[i]);
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
            ref var array = ref Unsafe.AsRef<string[]>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();

                if (count > 0)
                {
                    var type = (DataType)input.ReadByte();
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

        public override unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var list = Unsafe.Read<List<string>>(fieldAddress);
            if (list != null)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    var count = list.Count;

                    if (count > 0)
                    {
                        output.EnsureNext(6);
                        output.Write7BitEncodedIntUnchecked(count);

                        output.WriteByteUnchecked((byte)_elementDataType);

                        for (var i = 0; i < count; i++)
                            output.WriteString(list[i]);
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
            ref var list = ref Unsafe.AsRef<List<string>>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();

                list = new List<string>();

                if (count > 0)
                {
                    var array = new string[count];

                    ListHelpers.SetArray(list, array);

                    var type = (DataType)input.ReadByte();

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
                list = null;
            }
        }
    }

}