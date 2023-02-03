using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
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
                (FormerlySerializedAsAttribute[]) Attribute.GetCustomAttributes(fieldInfo,
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

        private SerializerOutput _output;

        private SerializerInput _input;

        private ClassSerializationHelper _serializer;

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

            _output = new SerializerOutput(2048 * 10);
            _input = new SerializerInput(2048 * 10);

            if (_uSerializer.TryGetClassHelper(out _serializer, typeof(T)) == false)
                throw new Exception($"Cannot serialize {typeof(T)}");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected override void Serialize(T obj, Stream stream)
        {
            _output.SetStream(stream);
            _serializer.SerializeObject(obj, _output, null);
            _output.Flush();
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        protected override T Deserialize(Stream stream)
        {
            _input.SetStream(stream);
            var result = (T)_serializer.DeserializeObject(_input, null);
            _input.FinishRead();
            return result;
        }
    }
}