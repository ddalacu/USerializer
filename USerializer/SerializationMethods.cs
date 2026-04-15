using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace USerialization
{
    public abstract class DataSerializer
    {
        public bool Initialized { get; private set; }

        public abstract DataType DataType { get; }

        public abstract void Write(ReadOnlySpan<byte> span, ref SerializerOutput output);

        public abstract void Read(Span<byte> span, ref SerializerInput input);

        public void RootInitialize(USerializer serializer)
        {
            if (Initialized)
                return;

            Initialized = true;

            Initialize(serializer);

            var dataType = DataType;

            if (dataType == DataType.None)
                serializer.Logger.Error($"Data type is none {this}, something went wrong!");
        }

        protected abstract void Initialize(USerializer serializer);

        public void Serialize<T>(ref T value, ref SerializerOutput output)
        {
            ref var data = ref Unsafe.As<T, byte>(ref value);
            Write(MemoryMarshal.CreateSpan(ref data, Unsafe.SizeOf<T>()), ref output);
        }

        public void Deserialize<T>(ref T value, ref SerializerInput input)
        {
            ref var data = ref Unsafe.As<T, byte>(ref value);
            Read(MemoryMarshal.CreateSpan(ref data, Unsafe.SizeOf<T>()), ref input);
        }
    }
}