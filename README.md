# USerializer
USerializer is a version tolerant binary serializer
This serializer was made for Unity3D but will run on net core, net framework and mono (at least it should :))
It uses no code generation so it works aot and it supports versioning


![Performance image](../gh-pages/output.png)

![HtmlPerformance](../gh-pages/index.html)

 ```csharp
	public class SerializeFieldAttribute : Attribute
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
        private readonly Type _serializeFieldAttributeType = typeof(SerializeFieldAttribute);

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
            var formerly = (FormerlySerializedAsAttribute[])Attribute.GetCustomAttributes(fieldInfo,typeof(FormerlySerializedAsAttribute));

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
	
	public static class BinaryUtility
    {
        private static USerializer _uSerializer;

        public static USerializer USerializer => _uSerializer;

        static BinaryUtility()
        {
            var providers = ProvidersUtils.GetDefaultProviders();
            _uSerializer = new USerializer(new UnitySerializationPolicy(), providers, new DataTypesDatabase());
        }

        /// <summary>
        /// Serializes a object fields to a stream
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="stream"></param>
        /// <param name="bufferSize">buffer size used to copy to the stream</param>
        /// <returns>false if type is not serializable</returns>
        public static bool Serialize(object obj, Stream stream, int bufferSize = 4096 * 2)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var output = new SerializerOutput(bufferSize, stream);
            var serialize = _uSerializer.Serialize(output, obj);
            output.Flush();

            return serialize;
        }

        /// <summary>
        /// Creates a object with data from the stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="result"></param>
        /// <param name="bufferSize"></param>
        /// <returns>false if type is not serializable</returns>
        public static bool TryDeserialize<T>(Stream stream, out T result, int bufferSize = 4096 * 2)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var serializerInput = new SerializerInput(bufferSize, stream);
            var tryDeserialize = _uSerializer.TryDeserialize<T>(serializerInput, out result);
            serializerInput.FinishRead();
            return tryDeserialize;
        }

        /// <summary>
        /// Populates a object fields with data from the stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="ob"></param>
        /// <param name="bufferSize"></param>
        /// <returns>false if type is not serializable</returns>
        public static bool TryPopulateObject<T>(Stream stream, ref T ob, int bufferSize = 4096 * 2)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var serializerInput = new SerializerInput(bufferSize, stream);
            var tryPopulateObject = _uSerializer.TryPopulateObject(serializerInput, ref ob);
            serializerInput.FinishRead();
            return tryPopulateObject;
        }
    }
  ```