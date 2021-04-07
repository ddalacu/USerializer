using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public abstract class SurrogateSerializerBase<T, TSurrogate> : ICustomSerializer
    {
        public Type SerializedType => _type;

        private USerializer _serializer;

        private Type _type;

        protected USerializer Serializer => _serializer;

        private SerializationMethods _methods;

        protected SurrogateSerializerBase()
        {
            _type = typeof(T);
        }

        public void Initialize(USerializer serializer)
        {
            _serializer = serializer;
            _serializer.TryGetSerializationMethods(typeof(TSurrogate), out _methods);
        }

        public abstract void CopyToSurrogate(ref T from, ref TSurrogate to);

        public abstract void CopyFromSurrogate(ref TSurrogate from, ref T to);

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            ref var instance = ref Unsafe.AsRef<T>(fieldAddress);
            var to = default(TSurrogate);
            CopyToSurrogate(ref instance, ref to);

            _methods.Serialize(Unsafe.AsPointer(ref to), output);
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var instance = ref Unsafe.AsRef<T>(fieldAddress);
            var from = default(TSurrogate);

            _methods.Deserialize(Unsafe.AsPointer(ref from), input);
            CopyFromSurrogate(ref from, ref instance);
        }

        public unsafe SerializationMethods GetMethods()
        {
            return new SerializationMethods(Write, Read, DataType.Object);
        }
    }
}