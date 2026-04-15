using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace USerialization
{
    public abstract class SurrogateSerializerBase<T, TSurrogate> : CustomDataSerializer
    {
        private USerializer _serializer;

        protected USerializer Serializer => _serializer;

        private DataSerializer _dataSerializer;

        public override DataType DataType => _dataSerializer.DataType;

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
        

        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output, object context)
        {
            Debug.Assert(span.Length == Unsafe.SizeOf<T>());

            ref var instance = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span));

            var to = default(TSurrogate);
            CopyToSurrogate(ref instance, ref to);
            ref var data = ref Unsafe.As<TSurrogate, byte>(ref to);
            _dataSerializer.Write(MemoryMarshal.CreateSpan(ref data, Unsafe.SizeOf<TSurrogate>()), ref output, context);
        }

        public override void Read(Span<byte> span, ref SerializerInput input, object context)
        {
            Debug.Assert(span.Length == Unsafe.SizeOf<T>());

            ref var instance = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span));
            var from = default(TSurrogate);

            ref var data = ref Unsafe.As<TSurrogate, byte>(ref from);
            _dataSerializer.Read(MemoryMarshal.CreateSpan(ref data, Unsafe.SizeOf<TSurrogate>()), ref input, context);
            CopyFromSurrogate(ref from, ref instance);
        }
    }
}