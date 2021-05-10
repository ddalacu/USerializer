using System.Runtime.CompilerServices;

namespace USerialization
{
    public readonly unsafe struct TypedDataSerializer<T>
    {
        private readonly DataSerializer _methods;

        public TypedDataSerializer(DataSerializer methods)
        {
            _methods = methods;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(T item, SerializerOutput output)
        {
            var childAddress = Unsafe.AsPointer(ref item);
            _methods.Write(childAddress, output);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ref T item, SerializerOutput output)
        {
            var childAddress = Unsafe.AsPointer(ref item);
            _methods.Write(childAddress, output);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize(SerializerInput input)
        {
            T item = default;
            var childAddress = Unsafe.AsPointer(ref item);
            _methods.Read(childAddress, input);
            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(ref T item, SerializerInput input)
        {
            var childAddress = Unsafe.AsPointer(ref item);
            _methods.Read(childAddress, input);
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