using System;
using System.Runtime.CompilerServices;

namespace USerialization
{
    public readonly unsafe struct TypedDataSerializer<T>
    {
        //private readonly DataSerializer _methods;
        private readonly InstanceWriteMethodPointer _write;
        private readonly InstanceReadMethodPointer _read;


        public TypedDataSerializer(DataSerializer methods)
        {
            //_methods = methods;
            _write = methods.WriteMethod;
            if (_write.IsValid == false)
                throw new Exception();
            _read = methods.ReadMethod;
            if (_read.IsValid == false)
                throw new Exception();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(T item, SerializerOutput output)
        {
            var childAddress = Unsafe.AsPointer(ref item);
            _write.Invoke(childAddress, output);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ref T item, SerializerOutput output)
        {
            var childAddress = Unsafe.AsPointer(ref item);
            _write.Invoke(childAddress, output);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize(SerializerInput input)
        {
            T item = default;
            var childAddress = Unsafe.AsPointer(ref item);
            _read.Invoke(childAddress, input);
            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(ref T item, SerializerInput input)
        {
            var childAddress = Unsafe.AsPointer(ref item);
            _read.Invoke(childAddress, input);
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