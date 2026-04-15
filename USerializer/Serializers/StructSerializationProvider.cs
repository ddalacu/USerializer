using System;
using System.Diagnostics;
using System.Reflection;

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

            serializationMethods = new StructDataSerializer(type,
                (fieldInfo) => serializer.SerializationPolicy.ShouldSerialize(fieldInfo));
            return true;
        }
    }

    public sealed class StructDataSerializer : DataSerializer
    {
        private readonly Type _type;

        private FieldsSerializer _fieldsSerializer;

        private Func<FieldInfo, bool> _shouldSerialize;

        public override DataType DataType => DataType.Object;

        protected override void Initialize(USerializer serializer)
        {
            var (metas, serializationDatas) = FieldSerializationData.GetFields(_type, serializer,
                _shouldSerialize);

            _fieldsSerializer = new FieldsSerializer(metas, serializationDatas, serializer.DataTypesDatabase);
        }

        private int _stackSize;

        public StructDataSerializer(Type type, Func<FieldInfo, bool> shouldSerialize)
        {
            _type = type;
            _stackSize = UnsafeUtils.GetStackSize(type);
            _shouldSerialize = shouldSerialize;
        }

        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output, object context)
        {
            Debug.Assert(span.Length == _stackSize);

            var track = output.BeginSizeTrack();
            _fieldsSerializer.Write(span, ref output, context);
            output.WriteSizeTrack(track);
        }

        public override void Read(Span<byte> span, ref SerializerInput input, object context)
        {
            Debug.Assert(span.Length == _stackSize);

            if (input.NotNull())
            {
                _fieldsSerializer.Read(span, ref input, context);
            }
            else
            {
                //"Changed from reference type?"
            }
        }
    }
}