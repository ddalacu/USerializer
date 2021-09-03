using System;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    public class StructSerializationProvider : ISerializationProvider
    {
        public bool TryGet(USerializer serializer, Type type, out DataSerializer serializationMethods)
        {
            serializationMethods = default;

            if (serializer.DataTypesDatabase.TryGet(out ObjectDataTypeLogic dataTypeLogic) == false)
                return false;

            if (type.IsArray)
                return false;

            if (type.IsValueType == false)
                return false;

            if (type.IsPrimitive)
                return false;

            if (serializer.SerializationPolicy.ShouldSerialize(type) == false)
                return false;

            serializationMethods = new StructDataSerializer(type, dataTypeLogic.Value);
            return true;
        }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed unsafe class StructDataSerializer : DataSerializer
    {
        private readonly Type _type;

        private readonly DataType _dataType;

        private FieldsSerializer _fieldsSerializer;

        public override DataType GetDataType() => _dataType;

        protected override void Initialize(USerializer serializer)
        {
            var (metas, serializationDatas) = FieldSerializationData.GetFields(_type, serializer);
            _fieldsSerializer = new FieldsSerializer(metas, serializationDatas, serializer.DataTypesDatabase);

            var writeAddress = ILUtils.GetAddress<StructDataSerializer>(nameof(Write));
            WriteMethod = new InstanceWriteMethodPointer(writeAddress, this);

            var readAddress = ILUtils.GetAddress<StructDataSerializer>(nameof(Read));
            ReadMethod = new InstanceReadMethodPointer(readAddress, this);
        }

        public StructDataSerializer(Type type, DataType objectDataType)
        {
            _type = type;
            _dataType = objectDataType;
        }

        protected override void Write(void* fieldAddress, SerializerOutput output)
        {
            var track = output.BeginSizeTrack();

            _fieldsSerializer.Write((byte*)fieldAddress, output);

            output.WriteSizeTrack(track);
        }

        protected override void Read(void* fieldAddress, SerializerInput input)
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