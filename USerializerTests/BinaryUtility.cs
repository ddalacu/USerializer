using System;
using System.Buffers;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using USerialization;

namespace USerializerTests
{
    public interface ISerializationCallbacks
    {
        void OnBeforeSerialize(object context);

        void OnAfterSerialize(object context);
    }

    public class CustomClassSerializationProvider : ISerializationProvider
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

            if (serializer.DataTypesDatabase.TryGet(out ObjectDataSkipper objectDataTypeLogic) == false)
                return false;

            if (serializer.SerializationPolicy.ShouldSerialize(type) == false)
                return false;

            if (typeof(ISerializationCallbacks).IsAssignableFrom(type) == false)
                return false;

            serializationMethods = new CustomClassDataSerializer(type,
                (fieldInfo) => serializer.SerializationPolicy.ShouldSerialize(fieldInfo));

            return true;
        }
    }

    public sealed unsafe class CustomClassDataSerializer : DataSerializer
    {
        private readonly Type _type;
        private readonly Func<FieldInfo, bool> _shouldSerialize;

        private FieldsSerializer _fieldsSerializer;

        private readonly bool _haveCtor;

        private readonly int _dataSize;

        public override DataType DataType => DataType.Object;

        protected override void Initialize(USerializer serializer)
        {
            var (metas, serializationDatas) = FieldSerializationData.GetFields(_type, serializer,
                _shouldSerialize);

            _fieldsSerializer = new FieldsSerializer(metas, serializationDatas, serializer.DataTypesDatabase);
        }

        public CustomClassDataSerializer(Type type, Func<FieldInfo, bool> shouldSerialize)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsValueType)
                throw new ArgumentException(nameof(type));

            _type = type;
            _shouldSerialize = shouldSerialize;
            _dataSize = UnsafeUtils.GetClassHeapSize(type);

            var constructor = _type.GetConstructor(Type.EmptyTypes);
            _haveCtor = constructor != null;
        }

        private int _stack;

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

            Unsafe.As<ISerializationCallbacks>(obj).OnBeforeSerialize(context);

            fixed (byte* objectAddress = &obj.Pinnable)
            {
                _fieldsSerializer.Write(new Span<byte>(objectAddress, _dataSize), output, context);
            }

            output.WriteSizeTrack(track);

            _stack--;
        }

        public override void Read(Span<byte> span, SerializerInput input, object context)
        {
            ref var instance = ref Unsafe.As<byte, Object>(ref MemoryMarshal.GetReference(span));

            if (input.NotNull())
            {
                if (instance == null)
                {
                    if (_haveCtor)
                    {
                        instance = ObjectActivator.CreateInstance(_type);
                    }
                    else
                        instance = FormatterServices.GetUninitializedObject(_type);
                }

                ref var pinnable = ref Unsafe.As<byte, PinnableObject>(ref MemoryMarshal.GetReference(span));
                fixed (byte* objectAddress = &pinnable.Pinnable)
                {
                    _fieldsSerializer.Read(new Span<byte>(objectAddress, _dataSize), input, context);
                }

                Unsafe.As<ISerializationCallbacks>(instance).OnAfterSerialize(context);
            }
            else
            {
                instance = null;
            }
        }
    }

    public static class BinaryUtility
    {
        private static USerializer _uSerializer;

        public static USerializer USerializer => _uSerializer;

        private class ConsoleLogger : ILogger
        {
            public void Error(string error)
            {
                Console.WriteLine("Error:" + error);
                throw new Exception(error);
            }
        }

        static BinaryUtility()
        {
            var consoleLogger = new ConsoleLogger();

            ISerializationProvider[] providers =
            {
                new PrimitivesSerializerProvider(),
                new CustomSerializerProvider(consoleLogger),
                new EnumSerializer(),
                new ArraySerializer(),
                new ListSerializer(),
                new TupleSerializationProvider(),
                new KeyValuePairSerializationProvider(),
                new DictionarySerializerProvider(),
                new CustomClassSerializationProvider(),
                new ClassSerializationProvider(),
                new StructSerializationProvider(),
            };

            _uSerializer = new USerializer(new UnitySerializationPolicy(), providers,
                new DataTypesDatabase(), consoleLogger);
        }

        /// <summary>
        /// Serializes a object fields to a stream
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
        /// <param name="context"></param>
        /// <param name="bufferSize">buffer size used to copy to the stream</param>
        /// <returns>false if type is not serializable</returns>
        public static bool Serialize(object obj, Stream stream, object context = null, int bufferSize = 4096 * 2)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (_uSerializer.TryGetDataSerializer(obj.GetType(), out var data, true) == false)
                return false;

            using var output = new SerializerOutput(bufferSize, ArrayPool<byte>.Shared);

            data.Write(SpanUtils.GetByteSpan(ref obj), output, context);

            output.Flush(stream);
            return true;
        }

        /// <summary>
        /// Creates a object with data from the stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="result"></param>
        /// <param name="context"></param>
        /// <param name="bufferSize"></param>
        /// <returns>false if type is not serializable</returns>
        public static bool TryDeserialize<T>(Stream stream, ref T result, object context = null,
            int bufferSize = 512) where T : class
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (_uSerializer.TryGetDataSerializer(typeof(T), out var data, true) == false)
                return false;
            
            using var serializerInput = new SerializerInput(bufferSize, stream, ArrayPool<byte>.Shared);
            
            data.Read(SpanUtils.GetByteSpan(ref result), serializerInput, context);

            serializerInput.FinishRead();
            return true;
        }
    }
}