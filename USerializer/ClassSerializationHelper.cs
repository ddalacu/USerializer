using System;

namespace USerialization
{
    public readonly struct ClassSerializationHelper
    {
        private readonly DataSerializer _dataSerializer;

        private readonly Type _serializedType;

        internal ClassSerializationHelper(DataSerializer dataSerializer, Type serializedType)
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
            if (item == null)
                throw new ArgumentException(nameof(item));
            if (output == null)
                throw new ArgumentException(nameof(output));

            var itemType = item.GetType();

            if (itemType.IsValueType == false)
            {
                if (_serializedType.IsAssignableFrom(itemType) == false)
                    throw new ArgumentException($"{itemType} cannot be casted to {_serializedType}");
                
                _dataSerializer.Write(SpanUtils.GetByteSpan(ref item), output, context);
                return;
            }

            if (itemType != _serializedType)
                throw new ArgumentException(
                    $"Type of the object {itemType} is not the same as the serializer type {_serializedType}");

            throw new NotImplementedException();
            // var pinnable = Unsafe.As<object, PinnableObject>(ref item);
            // fixed (byte* objectAddress = &pinnable.Pinnable)
            // {
            //     _dataSerializer.Write(objectAddress, output, context);
            // }
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

    public static class ClassSerializationHelperExtensions
    {
        public static bool TryGetClassHelper(this USerializer serializer, out ClassSerializationHelper dataSerializer,
            Type type, bool initializeDataSerializer = true)
        {
            if (serializer.TryGetDataSerializer(type, out var data, initializeDataSerializer) == false)
            {
                dataSerializer = default;
                return false;
            }

            dataSerializer = new ClassSerializationHelper(data, type);
            return true;
        }
    }
}