using System.Runtime.CompilerServices;
using UnityEngine;

namespace USerialization
{
    /// <summary>
    /// Groups together <see cref="WriteDelegate"/> and <see cref="ReadDelegate"/> together
    /// </summary>
    public readonly struct SerializationMethods
    {
        public readonly WriteDelegate Serialize;

        public readonly ReadDelegate Deserialize;

        public readonly DataType DataType;

        public SerializationMethods(WriteDelegate serialize, ReadDelegate deserialize, DataType dataType)
        {
            Serialize = serialize;
            Deserialize = deserialize;
            DataType = dataType;
        }
    }

    public readonly unsafe struct TypedSerializationMethods<T>
    {
        private readonly SerializationMethods _methods;

        public DataType DataType => _methods.DataType;

        public TypedSerializationMethods(SerializationMethods methods)
        {
            _methods = methods;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(T item, SerializerOutput output)
        {
            var childAddress = Unsafe.AsPointer(ref item);
            _methods.Serialize(childAddress, output);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Serialize(ref T item, SerializerOutput output)
        {
            var childAddress = Unsafe.AsPointer(ref item);
            _methods.Serialize(childAddress, output);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Deserialize(SerializerInput input)
        {
            T item = default;
            var childAddress = Unsafe.AsPointer(ref item);
            _methods.Deserialize(childAddress, input);
            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deserialize(ref T item, SerializerInput input)
        {
            var childAddress = Unsafe.AsPointer(ref item);
            _methods.Deserialize(childAddress, input);
        }
    }

}