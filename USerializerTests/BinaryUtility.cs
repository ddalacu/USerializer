using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using Unity.IL2CPP.CompilerServices;
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

            if (serializer.DataTypesDatabase.TryGet(out ObjectDataTypeLogic objectDataTypeLogic) == false)
                return false;

            if (serializer.SerializationPolicy.ShouldSerialize(type) == false)
                return false;

            if (typeof(ISerializationCallbacks).IsAssignableFrom(type) == false)
                return false;

            serializationMethods = new CustomClassDataSerializer(type, objectDataTypeLogic.Value);

            return true;
        }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed unsafe class CustomClassDataSerializer : DataSerializer
    {
        private readonly Type _type;

        private FieldsSerializer _fieldsSerializer;

        private readonly bool _haveCtor;

        private readonly DataType _dataType;

        public override DataType GetDataType() => _dataType;

        protected override void Initialize(USerializer serializer)
        {
            var (metas, serializationDatas) = FieldSerializationData.GetFields(_type, serializer);
            _fieldsSerializer = new FieldsSerializer(metas, serializationDatas, serializer.DataTypesDatabase);
        }

        public CustomClassDataSerializer(Type type, DataType objectDataType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsValueType)
                throw new ArgumentException(nameof(type));

            _type = type;
            var constructor = _type.GetConstructor(Type.EmptyTypes);
            _haveCtor = constructor != null;
            _dataType = objectDataType;
        }

        private int _stack;

        private const int MaxStack = 32;

        public override void Write(void* fieldAddress, SerializerOutput output, object context)
        {
            var obj = Unsafe.Read<object>(fieldAddress);

            if (obj == null)
            {
                output.WriteNull();
                return;
            }

            var value = Interlocked.Increment(ref _stack);

            if (value >= MaxStack)
                throw new CircularReferenceException("The system does not support circular references!");

            var track = output.BeginSizeTrack();

            Unsafe.As<ISerializationCallbacks>(obj).OnBeforeSerialize(context);
            
            var pinnable = Unsafe.As<object, PinnableObject>(ref obj);

            fixed (byte* objectAddress = &pinnable.Pinnable)
            {
                _fieldsSerializer.Write(objectAddress, output, context);
            }

            output.WriteSizeTrack(track);

            Interlocked.Decrement(ref _stack);
        }

        public override void Read(void* fieldAddress, SerializerInput input, object context)
        {
            ref var instance = ref Unsafe.AsRef<object>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                if (instance == null)
                {
                    if (_haveCtor)
                    {
                        instance = Activator.CreateInstance(_type);
                    }
                    else
                        instance = FormatterServices.GetUninitializedObject(_type);
                }

                var pinnable = Unsafe.As<object, PinnableObject>(ref instance);
                fixed (byte* objectAddress = &pinnable.Pinnable)
                {
                    _fieldsSerializer.Read(objectAddress, input, context);
                }

                input.EndObject(end);
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
                new CustomSerializerProvider(consoleLogger),
                new EnumSerializer(),
                new ArraySerializer(),
                new ListSerializer(),
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

            var output = new SerializerOutput(bufferSize, stream);
            var serialize = _uSerializer.Serialize(output, obj, context);
            output.Flush();

            return serialize;
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
            int bufferSize = 4096 * 2)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var serializerInput = new SerializerInput(bufferSize, stream);
            var tryDeserialize = _uSerializer.TryDeserialize(serializerInput, ref result, context);
            serializerInput.FinishRead();
            return tryDeserialize;
        }

        /// <summary>
        /// Populates a object fields with data from the stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="ob"></param>
        /// <param name="context"></param>
        /// <param name="bufferSize"></param>
        /// <returns>false if type is not serializable</returns>
        public static bool TryPopulateObject<T>(Stream stream, ref T ob, object context = null,
            int bufferSize = 4096 * 2)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var serializerInput = new SerializerInput(bufferSize, stream);
            var tryPopulateObject = _uSerializer.TryDeserialize(serializerInput, ref ob, context);
            serializerInput.FinishRead();
            return tryPopulateObject;
        }
    }
}