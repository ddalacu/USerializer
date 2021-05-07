using System;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    public unsafe class StructSerializationProvider : ISerializationProvider
    {
        private USerializer _serializer;

        private FieldDataCache _fieldDataCache;

        public void Initialize(USerializer serializer)
        {
            _serializer = serializer;
            _fieldDataCache = new FieldDataCache(512);
        }

        public void Start(USerializer serializer)
        {

        }

        public bool TryGet(Type type, out DataSerializer serializationMethods)
        {
            serializationMethods = default;

            if (_serializer.DataTypesDatabase.TryGet(out ObjectDataTypeLogic dataTypeLogic) == false)
                return false;

            if (type.IsArray)
                return false;

            if (type.IsValueType == false)
                return false;

            if (type.IsPrimitive)
                return false;

            if (_serializer.SerializationPolicy.ShouldSerialize(type) == false)
                return false;

            if (_fieldDataCache.GetTypeData(type, _serializer, out var typeData) == false)
            {
                serializationMethods = default;
                return false;
            }

            var fieldsSerializer = new FieldsSerializer(typeData, _serializer.DataTypesDatabase);

            serializationMethods = new StructDataSerializer(fieldsSerializer, dataTypeLogic.Value);
            return true;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public class StructDataSerializer : DataSerializer
        {
            private readonly DataType _dataType;

            private FieldsSerializer _fieldsSerializer;

            public override DataType GetDataType() => _dataType;

            public StructDataSerializer(FieldsSerializer fieldsSerializer, DataType objectDataType)
            {
                _fieldsSerializer = fieldsSerializer;
                _dataType = objectDataType;
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
                    //"Changed from reference type?"
                }
            }
        }
    }
}