using System;
using System.Runtime.CompilerServices;

namespace USerialization
{
    public readonly unsafe struct TypedDataSerializer<T>
    {
        //private readonly DataSerializer _methods;
        private readonly DataSerializer _dataSerializer;


        public TypedDataSerializer(DataSerializer dataSerializer)
        {
            _dataSerializer = dataSerializer;
            if (_dataSerializer.Initialized == false)
                throw new Exception($"{_dataSerializer} not initialized!");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(T item, SerializerOutput output, object context)
        {
            var childAddress = Unsafe.AsPointer(ref item);
            _dataSerializer.Write(childAddress, output, context);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ref T item, SerializerOutput output, object context)
        {
            var childAddress = Unsafe.AsPointer(ref item);
            _dataSerializer.Write(childAddress, output, context);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize(SerializerInput input, object context)
        {
            T item = default;
            var childAddress = Unsafe.AsPointer(ref item);
            _dataSerializer.Read(childAddress, input, context);
            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(ref T item, SerializerInput input, object context)
        {
            var childAddress = Unsafe.AsPointer(ref item);
            _dataSerializer.Read(childAddress, input, context);
        }
    }

    public static class TypedDataSerializerExtensions
    {
        public static TypedDataSerializer<T> GetTyped<T>(this DataSerializer dataSerializer)
        {
            return new TypedDataSerializer<T>(dataSerializer);
        }
    }
}