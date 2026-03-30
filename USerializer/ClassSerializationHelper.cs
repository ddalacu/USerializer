using System;

namespace USerialization
{
    public readonly struct ClassSerializationHelper
    {
        private readonly DataSerializer _dataSerializer;

        private readonly Type _serializedType;

        public ClassSerializationHelper(DataSerializer dataSerializer, Type serializedType)
        {
            _dataSerializer = dataSerializer;
            _serializedType = serializedType;

            if (serializedType.IsValueType)
                throw new ArgumentException("Should not be value type!");

            if (_dataSerializer.Initialized == false)
                throw new Exception($"{_dataSerializer} not initialized!");
        }

        public void SerializeObject(object item, SerializerOutput output, object context)
        {
            var itemType = item.GetType();
            
            if (itemType != _serializedType)
            {
                if (_serializedType.IsAssignableFrom(itemType) == false)
                    throw new ArgumentException($"{itemType} cannot be casted to {_serializedType}");
            }
            
            _dataSerializer.Write(SpanUtils.GetByteSpan(ref item), output, context);
        }

        public object DeserializeObject(SerializerInput input, object context)
        {
            object item = default;
            _dataSerializer.Read(SpanUtils.GetByteSpan(ref item), input, context);
            return item;
        }

        public void PopulateObject(object item, SerializerInput input, object context)
        {
            if (item == null)
                throw new Exception();

            var itemType = item.GetType();
            if (_serializedType.IsAssignableFrom(itemType) == false)
                throw new ArgumentException($"{itemType} cannot be casted to {_serializedType}");
            
            _dataSerializer.Read(SpanUtils.GetByteSpan(ref item), input, context);
        }
    }
}