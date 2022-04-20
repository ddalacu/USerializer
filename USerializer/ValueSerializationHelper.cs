using System;
using System.Runtime.CompilerServices;

namespace USerialization
{
    public readonly unsafe struct ValueSerializationHelper<T> where T : struct
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

            var childAddress = Unsafe.AsPointer(ref item);
            _dataSerializer.Write(childAddress, output, context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Populate(ref T item, SerializerInput input, object context)
        {
            var childAddress = Unsafe.AsPointer(ref item);
            _dataSerializer.Read(childAddress, input, context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize(SerializerInput input, object context)
        {
            T item = default;
            var childAddress = Unsafe.AsPointer(ref item);
            _dataSerializer.Read(childAddress, input, context);
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