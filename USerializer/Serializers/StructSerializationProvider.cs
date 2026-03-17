using System;
using System.Diagnostics;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    public class StructSerializationProvider : ISerializationProvider
    {
        public bool TryGet(USerializer serializer, Type type, out DataSerializer serializationMethods)
        {
            serializationMethods = default;
            
            if (type.IsArray)
                return false;

            if (type.IsValueType == false)
                return false;

            if (type.IsPrimitive)
                return false;

            if (serializer.SerializationPolicy.ShouldSerialize(type) == false)
                return false;

            serializationMethods = new StructDataSerializer(type);
            return true;
        }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class StructDataSerializer : DataSerializer
    {
        private readonly Type _type;
        
        private FieldsSerializer _fieldsSerializer;

        public override DataType GetDataType() => DataType.Object;

        protected override void Initialize(USerializer serializer)
        {
            var (metas, serializationDatas) = FieldSerializationData.GetFields(_type, serializer);
            _fieldsSerializer = new FieldsSerializer(metas, serializationDatas, serializer.DataTypesDatabase);
        }

        private int _stackSize;
        
        public StructDataSerializer(Type type)
        {
            _type = type;
            _stackSize= UnsafeUtils.GetStackSize(type);
        }

        public override void Write(ReadOnlySpan<byte> span, SerializerOutput output, object context)
        {
            Debug.Assert(span.Length == _stackSize);
            
            var track = output.BeginSizeTrack();
            _fieldsSerializer.Write(span, output, context);
            output.WriteSizeTrack(track);
        }

        public override void Read(Span<byte> span, SerializerInput input, object context)
        {
            Debug.Assert(span.Length == _stackSize);
            
            if (input.BeginReadSize(out var end))
            {
                _fieldsSerializer.Read(span, input, context);
                input.EndObject(end);
            }
            else
            {
                //"Changed from reference type?"
            }
        }
    }
}