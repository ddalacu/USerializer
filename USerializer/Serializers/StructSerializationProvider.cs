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
            private FieldData[] _fields;
            private DataTypesDatabase _dataTypesDatabase;
            private DataType _dataType;

            public override DataType GetDataType() => _dataType;

            public StructDataSerializer(TypeData typeData, USerializer serializer)
            {
                _fields = typeData.Fields;
                _dataTypesDatabase = serializer.DataTypesDatabase;

                if (_dataTypesDatabase.TryGet(out ObjectDataTypeLogic arrayDataTypeLogic))
                    _dataType = arrayDataTypeLogic.Value;
            }

            public override void WriteDelegate(void* fieldAddress, SerializerOutput output)
            {
                var track = output.BeginSizeTrack();

                _fields.WriteFields((byte*)fieldAddress, output);

                output.WriteSizeTrack(track);
            }

            public override void ReadDelegate(void* fieldAddress, SerializerInput input)
            {
                if (input.BeginReadSize(out var end))
                {
                    _fields.ReadFields((byte*)fieldAddress, input, _dataTypesDatabase);

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