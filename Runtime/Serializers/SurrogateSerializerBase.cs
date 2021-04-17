using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public abstract class SurrogateSerializerBase<T, TSurrogate> : CustomDataSerializer
    {
        public override Type SerializedType => typeof(T);

        private USerializer _serializer;

        protected USerializer Serializer => _serializer;

        private DataSerializer _dataSerializer;

        protected SurrogateSerializerBase() : base(DataType.None)
        {

        }

        public override void Initialize(USerializer serializer)
        {
            _serializer = serializer;

            var type = typeof(TSurrogate);
            if (_serializer.TryGetDataSerializer(type, out _dataSerializer))
                DataType = _dataSerializer.DataType;
            else
                Debug.LogError($"Could not get serialization data for {type}");
        }

        public abstract void CopyToSurrogate(ref T from, ref TSurrogate to);

        public abstract void CopyFromSurrogate(ref TSurrogate from, ref T to);


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