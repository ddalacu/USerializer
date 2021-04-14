using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public abstract class SurrogateSerializerBase<T, TSurrogate> : DataSerializer, ICustomSerializer
    {
        public Type SerializedType => _type;

        private USerializer _serializer;

        private Type _type;

        protected USerializer Serializer => _serializer;

        private DataSerializer _dataSerializer;

        protected SurrogateSerializerBase() : base(DataType.Object)
        {
            _type = typeof(T);
        }

        public void Initialize(USerializer serializer)
        {
            _serializer = serializer;
            _serializer.TryGetSerializationMethods(typeof(TSurrogate), out _dataSerializer);
        }

        public abstract void CopyToSurrogate(ref T from, ref TSurrogate to);

        public abstract void CopyFromSurrogate(ref TSurrogate from, ref T to);

        public DataSerializer GetMethods()
        {
            return this;
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            ref var instance = ref Unsafe.AsRef<T>(fieldAddress);
            var to = default(TSurrogate);
            CopyToSurrogate(ref instance, ref to);

            _dataSerializer.WriteDelegate(Unsafe.AsPointer(ref to), output);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            ref var instance = ref Unsafe.AsRef<T>(fieldAddress);
            var from = default(TSurrogate);

            _dataSerializer.ReadDelegate(Unsafe.AsPointer(ref from), input);
            CopyFromSurrogate(ref from, ref instance);
        }
    }
}