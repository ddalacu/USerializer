using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using USerialization;

namespace PerformanceTests
{
    public class SerializeField : Attribute
    {
    }

    public class FormerlySerializedAsAttribute : Attribute
    {
        public string OldName { get; }

        public FormerlySerializedAsAttribute(string oldName)
        {
            OldName = oldName;
        }
    }

    public class UnitySerializationPolicy : ISerializationPolicy
    {
        private readonly Type _serializeFieldAttributeType = typeof(SerializeField);

        private readonly Type _nonSerializedAttributeType = typeof(NonSerializedAttribute);

        private readonly Type _serializableAttributeType = typeof(SerializableAttribute);

        public Func<FieldInfo, bool> ShouldSerializeField;

        public bool ShouldSerialize(Type type)
        {
            if (type.IsAbstract)
                return false;

            if (type.IsGenericTypeDefinition) // Type<>
                return false;

            if (type.IsValueType)
                return true;

            if (type.IsClass)
            {
                if (Attribute.IsDefined(type, _serializableAttributeType))
                {
                    if (type.IsGenericType)
                    {
                        if (ShouldTryToSerialize(type) == false)
                            return false;
                    }

                    return true;
                }
            }

            return false;
        }

        private bool ShouldTryToSerialize(Type type)
        {
            foreach (var genericArgument in type.GetGenericArguments())
            {
                if (ShouldSerialize(genericArgument) == false)
                    return false;
            }

            var genericTypeDefinition = type.GetGenericTypeDefinition();

            if (genericTypeDefinition == typeof(Dictionary<,>))
                return false;
            if (genericTypeDefinition == typeof(HashSet<>))
                return false;
            if (genericTypeDefinition == typeof(Stack<>))
                return false;
            if (genericTypeDefinition == typeof(Queue<>))
                return false;
            if (genericTypeDefinition == typeof(List<>))
                return false;

            return true;
        }

        public bool ShouldSerialize(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsPrivate)
            {
                if (Attribute.IsDefined(fieldInfo, _serializeFieldAttributeType) == false)
                    return false;
            }
            else
            {
                if (Attribute.IsDefined(fieldInfo, _nonSerializedAttributeType))
                    return false;
            }

            if (ShouldSerializeField != null &&
                ShouldSerializeField(fieldInfo) == false)
                return false;

            return true;
        }

        public string[] GetAlternateNames(FieldInfo fieldInfo)
        {
            var formerly =
                (FormerlySerializedAsAttribute[])Attribute.GetCustomAttributes(fieldInfo,
                    typeof(FormerlySerializedAsAttribute));

            var length = formerly.Length;

            if (length == 0)
                return null;

            var old = new string[length];

            for (var index = 0; index < length; index++)
            {
                old[index] = formerly[index].OldName;
            }

            return old;
        }
    }

    public class USerializerBenchmark<T> : SerializerBenchmark<T> where T : class
    {
        private USerializer _uSerializer;

        private class ConsoleLogger : ILogger
        {
            public void Error(string error)
            {
                Console.WriteLine("Error:" + error);
                throw new Exception(error);
            }
        }

        public USerializerBenchmark()
        {
            var consoleLogger = new ConsoleLogger();
            
            var serializationProviders = ProvidersUtils.GetDefaultProviders(consoleLogger);

            _uSerializer = new USerializer(new UnitySerializationPolicy(), serializationProviders,
                new DataTypesDatabase(), consoleLogger);

            if (_uSerializer.TryGetDataSerializer(typeof(T), out var data, true) == false)
            {
                throw new Exception($"Cannot serialize {typeof(T)}");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected override void Serialize(T obj, Stream stream)
        {
            if (_uSerializer.TryGetDataSerializer(typeof(T), out var dataSerializer) == false)
                throw new Exception($"Cannot serialize {typeof(T)}");

            var output = new SerializerOutput(2048 * 20, ArrayPool<byte>.Shared);
            dataSerializer.Serialize(ref obj, ref output);
            output.Flush(stream);
            output.Dispose();
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        protected override T Deserialize(Stream stream)
        {
            var input = new SerializerInput(2048 * 20, stream, ArrayPool<byte>.Shared);
     
            if (_uSerializer.TryGetDataSerializer(typeof(T), out var dataSerializer, true) == false)
                throw new Exception($"Cannot serialize {typeof(T)}");

            T result = default;
            dataSerializer.Deserialize(ref result, ref input);
            input.FinishRead();
            input.Dispose();
            return result;
        }
    }
}