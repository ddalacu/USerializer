using System;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    public unsafe class StructSerializationProvider : ISerializationProvider
    {
        private USerializer _serializer;

        private TypeDataCache _typeDataCache;

        public void Initialize(USerializer serializer)
        {
            _serializer = serializer;
            _typeDataCache = new TypeDataCache(512);
        }

        public void Start(USerializer serializer)
        {

        }

        public bool TryGet(Type type, out DataSerializer serializationMethods)
        {
            if (type.IsArray)
            {
                serializationMethods = default;
                return false;
            }

            if (type.IsValueType == false)
            {
                serializationMethods = default;
                return false;
            }

            if (type.IsPrimitive)
            {
                serializationMethods = default;
                return false;
            }

            if (_typeDataCache.GetTypeData(type, _serializer, out var typeData) == false)
            {
                serializationMethods = default;
                return false;
            }

            serializationMethods = new StructDataSerializer(typeData, _serializer);
            return true;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public class StructDataSerializer : DataSerializer
        {
            private DataType _dataType;

            private FieldsSerializer _fieldsSerializer;

            public override DataType GetDataType() => _dataType;

            public StructDataSerializer(FieldsData typeData, USerializer serializer)
            {
                _fieldsSerializer = new FieldsSerializer(typeData, serializer.DataTypesDatabase);

                if (serializer.DataTypesDatabase.TryGet(out ObjectDataTypeLogic arrayDataTypeLogic))
                    _dataType = arrayDataTypeLogic.Value;
            }

            public override void WriteDelegate(void* fieldAddress, SerializerOutput output)
            {
                var track = output.BeginSizeTrack();

                _fieldsSerializer.Write((byte*)fieldAddress, output);

                output.WriteSizeTrack(track);
            }

            public override void ReadDelegate(void* fieldAddress, SerializerInput input)
            {
                if (input.BeginReadSize(out var end))
                {
                    _fieldsSerializer.Read((byte*)fieldAddress, input);
                    input.EndObject(end);
                }
                else
                {
                    //"Changed from nullable?"
                }
            }
        }
    }
}