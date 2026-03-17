using System;
using System.Runtime.CompilerServices;

namespace USerialization
{
    public readonly struct ValueSerializationHelper<T> where T : struct
    {
        private readonly DataSerializer _dataSerializer;

        internal ValueSerializationHelper(DataSerializer dataSerializer)
        {
            _dataSerializer = dataSerializer;

            if (_dataSerializer.Initialized == false)
                throw new Exception($"{_dataSerializer} not initialized!");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ref T item, SerializerOutput output, object context)
        {
            if (output == null)
                throw new NullReferenceException(nameof(output));
            
            _dataSerializer.Write(SpanUtils.GetByteSpan(ref item), output, context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Populate(ref T item, SerializerInput input, object context)
        {
            _dataSerializer.Read(SpanUtils.GetByteSpan(ref item), input, context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize(SerializerInput input, object context)
        {
            T item = default;
            _dataSerializer.Read(SpanUtils.GetByteSpan(ref item), input, context);
            return item;
        }
    }

    public static class ValueSerializationHelperExtensions
    {
        public static bool TryGetValueHelper<T>(this USerializer serializer,
            out ValueSerializationHelper<T> dataSerializer, bool initializeDataSerializer = true) where T : struct
        {
            var type = typeof(T);

            if (serializer.TryGetDataSerializer(type, out var data, initializeDataSerializer) == false)
            {
                dataSerializer = default;
                return false;
            }

            dataSerializer = new ValueSerializationHelper<T>(data);
            return true;
        }
    }
}