using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    public class ArraySerializer : ISerializationProvider
    {
        public bool TryGet(USerializer serializer, Type type, out DataSerializer serializationMethods)
        {
            serializationMethods = default;

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

            if (serializer.DataTypesDatabase.TryGet(out ArrayDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            var elementType = type.GetElementType();

            if (serializer.TryGetDataSerializer(elementType, out var elementSerializer, false) == false)
            {
                serializationMethods = default;
                return false;
            }

            serializationMethods = new ArrayDataSerializer(elementType, elementSerializer, arrayDataTypeLogic.Value);
            return true;
        }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed unsafe class ArrayDataSerializer : DataSerializer
    {
        private readonly Type _elementType;

        private readonly DataSerializer _elementSerializer;

        private InstanceWriteMethodPointer _elementWriter;

        private InstanceReadMethodPointer _elementReader;

        private readonly int _size;

        private readonly DataType _dataType;

        public override DataType GetDataType() => _dataType;

        private DataType _elementDataType;

        protected override void Initialize(USerializer serializer)
        {
            _elementSerializer.RootInitialize(serializer);

            _elementDataType = _elementSerializer.GetDataType();

            _elementWriter = _elementSerializer.WriteMethod;
            _elementReader = _elementSerializer.ReadMethod;

            if (_elementDataType == DataType.None)
            {
                serializer.Logger.Error("Element data type is none, something went wrong!");
            }

            //var writeAddress = ILUtils.GetAddress<ArrayDataSerializer>(nameof(Write));
            //WriteMethod = new InstanceWriteMethodPointer(writeAddress, this);

            //var readAddress = ILUtils.GetAddress<ArrayDataSerializer>(nameof(Read));
            //ReadMethod = new InstanceReadMethodPointer(readAddress, this);
        }

        public ArrayDataSerializer(Type elementType, DataSerializer elementSerializer, DataType arrayDataType)
        {
            _size = UnsafeUtils.GetArrayElementSize(elementType);
            _elementType = elementType;
            _elementSerializer = elementSerializer;
            _dataType = arrayDataType;
        }

        protected override void Write(void* fieldAddress, SerializerOutput output)
        {
            var array = Unsafe.Read<Array>(fieldAddress);

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
                    output.WriteByteUnchecked((byte)_elementDataType);

                    var pinnable = Unsafe.As<Array, byte[]>(ref array);
                    var writer = _elementWriter;
                    fixed (byte* address = pinnable)
                    {
                        var tempAddress = address;

                        for (var index = 0; index < count; index++)
                        {
                            writer.Invoke(tempAddress, output);
                            tempAddress += _size;
                        }
                    }
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

        protected override void Read(void* fieldAddress, SerializerInput input)
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

                    if (type == _elementDataType)
                    {
                        var pinnable = Unsafe.As<Array, byte[]>(ref array);
                        var reader = _elementReader;
                        fixed (byte* address = pinnable)
                        {
                            var tempAddress = address;

                            for (var i = 0; i < count; i++)
                            {
                                reader.Invoke(tempAddress, input);
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