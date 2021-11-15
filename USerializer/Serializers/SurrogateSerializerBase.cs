﻿using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public abstract class SurrogateSerializerBase<T, TSurrogate> : CustomDataSerializer
    {
        private USerializer _serializer;

        protected USerializer Serializer => _serializer;

        private DataSerializer _dataSerializer;

        public override DataType GetDataType() => _dataSerializer.GetDataType();

        protected override void Initialize(USerializer serializer)
        {
            _dataSerializer.RootInitialize(serializer);
        }

        public override bool TryInitialize(USerializer serializer)
        {
            _serializer = serializer;
            var type = typeof(TSurrogate);
            if (_serializer.TryGetDataSerializer(type, out _dataSerializer, false))
                return true;

            _serializer.Logger.Error($"Could not get serialization data for {type}");
            return false;
        }

        public abstract void CopyToSurrogate(ref T from, ref TSurrogate to);

        public abstract void CopyFromSurrogate(ref TSurrogate from, ref T to);


        public override unsafe void Write(void* fieldAddress, SerializerOutput output, object context)
        {
            ref var instance = ref Unsafe.AsRef<T>(fieldAddress);
            var to = default(TSurrogate);
            CopyToSurrogate(ref instance, ref to);

            _dataSerializer.Write(Unsafe.AsPointer(ref to), output, context);
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input, object context)
        {
            ref var instance = ref Unsafe.AsRef<T>(fieldAddress);
            var from = default(TSurrogate);

            _dataSerializer.Read(Unsafe.AsPointer(ref from), input, context);
            CopyFromSurrogate(ref from, ref instance);
        }
    }
}