using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

            if (serializer.SerializationPolicy.ShouldSerialize(type) == false)
                return false;

            var activator = ObjectActivator.GetActivator(type);

            serializationMethods = new ClassDataSerializer(type,
                activator,
                (fieldInfo) => serializer.SerializationPolicy.ShouldSerialize(fieldInfo));

            return true;
        }
    }

    public unsafe class ClassDataSerializer : DataSerializer
    {
        private FieldsSerializer _fieldsSerializer;

        private int _heapSize;
        private readonly Func<object> _activator;

        public override DataType DataType => DataType.Object;

        protected override void Initialize(USerializer serializer)
        {
            var (metas, serializationDatas) = FieldSerializationData.GetFields(_type, serializer,
                _shouldSerialize);

            _fieldsSerializer = new FieldsSerializer(metas, serializationDatas, serializer.DataTypesDatabase);
            _heapSize = serializer.RuntimeUtils.GetClassHeapSize(_type);
        }

        public ClassDataSerializer(Type type, Func<object> activator, Func<FieldInfo, bool> shouldSerialize)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsValueType)
                throw new ArgumentException(nameof(type));

            _type = type;
            _activator = activator;
            _shouldSerialize = shouldSerialize;
        }

        private int _stack;

        private readonly Type _type;

        private readonly Func<FieldInfo, bool> _shouldSerialize;

        private const int MaxStack = 32;


        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output, object context)
        {
            Debug.Assert(span.Length == IntPtr.Size);

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
                var readOnlySpan = new Span<byte>(objectAddress, _heapSize);
                _fieldsSerializer.Write(readOnlySpan,ref output, context);
            }

            output.WriteSizeTrack(track);

            _stack--;
        }

        public override void Read(Span<byte> span, ref SerializerInput input, object context)
        {
            Debug.Assert(span.Length == IntPtr.Size);
            ref var instance = ref Unsafe.As<byte, Object>(ref MemoryMarshal.GetReference(span));

            if (input.NotNull())
            {
                if (instance == null)
                {
                    instance = _activator();
                }

                ref var pinnable = ref Unsafe.As<byte, PinnableObject>(ref MemoryMarshal.GetReference(span));
                fixed (byte* objectAddress = &pinnable.Pinnable)
                {
                    _fieldsSerializer.Read(new Span<byte>(objectAddress, _heapSize), ref input, context);
                }
            }
            else
            {
                instance = null;
            }
        }
    }
}