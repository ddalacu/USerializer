using System.Runtime.CompilerServices;

namespace USerialization
{
    public abstract unsafe class DataSerializer
    {
        private DataType _dataType;

        public DataType DataType
        {
            get => _dataType;
            protected set => _dataType = value;
        }

        protected DataSerializer(DataType dataType)
        {
            _dataType = dataType;
        }

        public abstract void WriteDelegate(void* fieldAddress, SerializerOutput output);

        public abstract void ReadDelegate(void* fieldAddress, SerializerInput input);
    }

    public readonly unsafe struct TypedDataSerializer<T>
    {
        private readonly DataSerializer _methods;

        public DataType DataType => _methods.DataType;

        public TypedDataSerializer(DataSerializer methods)
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