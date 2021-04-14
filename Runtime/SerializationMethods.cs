using System.Runtime.CompilerServices;

namespace USerialization
{
    public abstract unsafe class DataSerializer
    {
        public readonly DataType DataType;

        protected DataSerializer(DataType dataType)
        {
            DataType = dataType;
        }

        public abstract void WriteDelegate(void* fieldAddress, SerializerOutput output);

        public abstract void ReadDelegate(void* fieldAddress, SerializerInput input);
    }


    /// <summary>
    /// Groups together <see cref="WriteDelegate"/> and <see cref="ReadDelegate"/> together
    /// </summary>
    public readonly struct SerializationMethods
    {
        public readonly DataSerializer DataSerializer;

        public readonly DataType DataType;

        public SerializationMethods(DataSerializer dataSerializer, DataType dataType)
        {
            DataSerializer = dataSerializer;
            DataType = dataType;
        }
    }

    public readonly unsafe struct TypedSerializationMethods<T>
    {
        private readonly DataSerializer _methods;

        public DataType DataType => _methods.DataType;

        public TypedSerializationMethods(DataSerializer methods)
        {
            _methods = methods;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(T item, SerializerOutput output)
        {
            var childAddress = Unsafe.AsPointer(ref item);
            _methods.WriteDelegate(childAddress, output);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ref T item, SerializerOutput output)
        {
            var childAddress = Unsafe.AsPointer(ref item);
            _methods.WriteDelegate(childAddress, output);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize(SerializerInput input)
        {
            T item = default;
            var childAddress = Unsafe.AsPointer(ref item);
            _methods.ReadDelegate(childAddress, input);
            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(ref T item, SerializerInput input)
        {
            var childAddress = Unsafe.AsPointer(ref item);
            _methods.ReadDelegate(childAddress, input);
        }
    }

}