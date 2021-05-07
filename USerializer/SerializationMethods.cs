using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace USerialization
{
    public abstract unsafe class DataSerializer
    {
        public abstract DataType GetDataType();

        public abstract void WriteDelegate(void* fieldAddress, SerializerOutput output);

        public abstract void ReadDelegate(void* fieldAddress, SerializerInput input);


        private bool _initialized;

        public void RootInitialize(USerializer serializer)
        {
            if(_initialized)
                return;
            _initialized = true;
            Initialize(serializer);
        }

        protected virtual void Initialize(USerializer serializer)
        {

        }
    }

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