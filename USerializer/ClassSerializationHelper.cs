using System;

namespace USerialization
{
    public readonly struct ClassSerializationHelper<T> where T : class
    {
        private readonly DataSerializer _dataSerializer;

        private readonly Type _serializedType;
        

    

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