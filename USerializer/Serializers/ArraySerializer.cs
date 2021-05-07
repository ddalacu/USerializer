using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public unsafe class ArraySerializer : ISerializationProvider
    {

        public void Initialize(USerializer serializer)
        {

        }

        public void Start(USerializer serializer)
        {

        }

        public bool TryGet(USerializer serializer, Type type, out DataSerializer serializationMethods)
        {
            serializationMethods = default;

            if (serializer.DataTypesDatabase.TryGet(out ArrayDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            if (type.IsArray == false)
            {
                serializationMethods = default;
                return false;
            }

            if (type.GetArrayRank() > 1)
            {
                serializationMethods = default;
                return false;
            }

            var elementType = type.GetElementType();

            if (serializer.TryGetDataSerializer(elementType, out var elementSerializer) == false)
            {
                serializationMethods = default;
                return false;
            }

            serializationMethods = new ArrayDataSerializer(elementType, elementSerializer, arrayDataTypeLogic.Value);
            return true;
        }


        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public class ArrayDataSerializer : DataSerializer
        {
            private readonly Type _elementType;

            private readonly DataSerializer _elementSerializer;

            private readonly int _size;

            private readonly DataType _dataType;

            public override DataType GetDataType() => _dataType;

            protected override void Initialize(USerializer serializer)
            {
                _elementSerializer.RootInitialize(serializer);
            }

            public ArrayDataSerializer(Type elementType, DataSerializer elementSerializer, DataType arrayDataType)
            {
                _size = UnsafeUtils.GetArrayElementSize(elementType);
                _elementType = elementType;
                _elementSerializer = elementSerializer;
                _dataType = arrayDataType;
            }

            public override void WriteDelegate(void* fieldAddress, SerializerOutput output)
            {
                var array = Unsafe.Read<Array>(fieldAddress);

                if (array == null)
                {
                    output.WriteNull();
                    return;
                }

                var count = array.Length;

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
                ref var array = ref Unsafe.AsRef<Array>(fieldAddress);

                if (input.BeginReadSize(out var end))
                {
                    var count = input.Read7BitEncodedInt();

                    if (array == null || array.Length != count)
                        array = Array.CreateInstance(_elementType, count);

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
                    array = null;
                }
            }
        }

    }


}