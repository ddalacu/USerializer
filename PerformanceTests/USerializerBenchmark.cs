using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using USerialization;

namespace PerformanceTests
{
    public class SerializeField : Attribute
    {

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

            if (type.IsGenericType)// Type<int>
                return false;

            if (type.IsGenericTypeDefinition)// Type<>
                return false;

            if (type.IsValueType)
                return true;

            if (type.IsClass)
            {
                if (type.GetCustomAttribute(_serializableAttributeType) != null)
                    return true;
            }

            return false;
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
            return null;
        }
    }

    public class USerializerBenchmark<T> : SerializerBenchmark<T> where T : class
    {
        private USerializer _uSerializer;

        private SerializerOutput _output;

        private SerializerInput _input;

        public USerializerBenchmark()
        {
            _uSerializer = new USerializer(new UnitySerializationPolicy(), ProvidersUtils.GetDefaultProviders(),
                new DataTypesDatabase());

            _output = new SerializerOutput(2048 * 10);
            _input = new SerializerInput(2048 * 10);

            _uSerializer.PreCacheType(typeof(T));
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        protected override void Serialize(T obj, Stream stream)
        {
            _output.SetStream(stream);
            _uSerializer.Serialize(_output, obj);
            _output.Flush();
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        protected override T Deserialize(Stream stream)
        {
            _input.SetStream(stream);
            _uSerializer.TryDeserialize(_input, out T result);
            _input.FinishRead();
            return result;
        }

    }
}