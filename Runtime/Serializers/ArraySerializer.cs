using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public unsafe class ArraySerializer : ISerializationProvider
    {
        private USerializer _serializer;

        public void Initialize(USerializer serializer)
        {
            _serializer = serializer;
        }

        public void Start(USerializer serializer)
        {

        }

        public bool TryGet(Type type, out DataSerializer serializationMethods)
        {
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

            if (_serializer.TryGetDataSerializer(elementType, out var elementSerializer) == false)
            {
                serializationMethods = default;
                return false;
            }

            serializationMethods = new ArrayDataSerializer(elementType, elementSerializer, _serializer);
            return true;
        }


        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public class ArrayDataSerializer : DataSerializer
        {
            private readonly Type _elementType;
            private readonly DataSerializer _elementSerializer;
            private readonly int _size;

            private DataType _dataType;

            public override DataType GetDataType() => _dataType;


            public ArrayDataSerializer(Type elementType, DataSerializer elementSerializer, USerializer serializer)
            {
                if (elementType.IsValueType)
                    _size = UnsafeUtility.SizeOf(elementType);
                else
                    _size = UnsafeUtility.SizeOf(typeof(IntPtr));

                _elementType = elementType;
                _elementSerializer = elementSerializer;

                var typeLogic = serializer.DataTypesDatabase;

                if (typeLogic.TryGet(out ArrayDataTypeLogic arrayDataTypeLogic))
                    _dataType = arrayDataTypeLogic.Value;
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
                    output.EnsureNext(6);
                    output.WriteByteUnchecked((byte)_elementSerializer.GetDataType());
                    output.Write7BitEncodedIntUnchecked(count);

                    var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                    for (var index = 0; index < count; index++)
                    {
                        _elementSerializer.WriteDelegate(address, output);
                        address += _size;
                    }

                    UnsafeUtility.ReleaseGCObject(handle);
                }
                output.WriteSizeTrack(sizeTracker);
            }

            public override void ReadDelegate(void* fieldAddress, SerializerInput input)
            {
                ref var array = ref Unsafe.AsRef<Array>(fieldAddress);

                if (input.BeginReadSize(out var end))
                {
                    var type = (DataType)input.ReadByte();

                    var count = input.Read7BitEncodedInt();

                    if (array == null || array.Length != count)
                        array = Array.CreateInstance(_elementType, count);

                    if (type == _elementSerializer.GetDataType())
                    {
                        var address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out var handle);

                        for (var i = 0; i < count; i++)
                        {
                            _elementSerializer.ReadDelegate(address, input);
                            address += _size;
                        }

                        UnsafeUtility.ReleaseGCObject(handle);
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