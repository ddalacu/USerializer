using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    public class CircularReferenceException : Exception
    {
        public CircularReferenceException(string message) : base(message)
        {
        }
    }

    public class ClassSerializationProvider : ISerializationProvider
    {
        public bool TryGet(USerializer serializer, Type type, out DataSerializer serializationMethods)
        {
            serializationMethods = default;

            if (type.IsArray)
                return false;

            if (type.IsValueType)
                return false;

            if (type.IsPrimitive)
                return false;

            if (serializer.DataTypesDatabase.TryGet(out ObjectDataTypeLogic objectDataTypeLogic) == false)
                return false;

            if (serializer.SerializationPolicy.ShouldSerialize(type) == false)
                return false;

            serializationMethods = new ClassDataSerializer(type, objectDataTypeLogic.Value);

            return true;
        }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed unsafe class ClassDataSerializer : DataSerializer
    {
        private FieldsSerializer _fieldsSerializer;

        private readonly DataType _dataType;

        private readonly int _heapSize;

        public override DataType GetDataType() => _dataType;

        protected override void Initialize(USerializer serializer)
        {
            var (metas, serializationDatas) = FieldSerializationData.GetFields(_type, serializer);
            _fieldsSerializer = new FieldsSerializer(metas, serializationDatas, serializer.DataTypesDatabase);
        }

        public ClassDataSerializer(Type type, DataType objectDataType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsValueType)
                throw new ArgumentException(nameof(type));

            _type = type;
            _dataType = objectDataType;
            _heapSize = UnsafeUtils.GetClassHeapSize(type);
        }

        private int _stack;

        private readonly Type _type;

        private const int MaxStack = 32;

        public override void Write(ReadOnlySpan<byte> span, SerializerOutput output, object context)
        {
            ref var obj = ref Unsafe.As<byte, PinnableObject>(ref MemoryMarshal.GetReference(span));

            if (obj == null)
            {
                output.WriteNull();
                return;
            }

            _stack++;

            if (_stack >= MaxStack)
                throw new CircularReferenceException("The system does not support circular references!");

            var track = output.BeginSizeTrack();

            fixed (byte* objectAddress = &obj.Pinnable)
            {
                _fieldsSerializer.Write(new Span<byte>(objectAddress, _heapSize), output, context);
            }

            output.WriteSizeTrack(track);

            _stack--;
        }

        public override void Read(Span<byte> fieldAddress, SerializerInput input, object context)
        {
            ref var instance = ref Unsafe.As<byte, Object>(ref MemoryMarshal.GetReference(fieldAddress));

            if (input.BeginReadSize(out var end))
            {
                if (instance == null)
                {
                    instance = Activator.CreateInstance(_type);
                }
                
                ref var pinnable = ref Unsafe.As<byte, PinnableObject>(ref MemoryMarshal.GetReference(fieldAddress));
                fixed (byte* objectAddress = &pinnable.Pinnable)
                {
                    _fieldsSerializer.Read(new Span<byte>(objectAddress, _heapSize), input, context);
                }

                input.EndObject(end);
            }
            else
            {
                instance = null;
            }
        }
    }
}