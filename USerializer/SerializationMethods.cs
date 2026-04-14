using System;

namespace USerialization
{
    public abstract class DataSerializer
    {
        public bool Initialized { get; private set; }

        public abstract DataType DataType { get; }

        public abstract void Write(ReadOnlySpan<byte> span, SerializerOutput output, object context);

        public abstract void Read(Span<byte> span, ref SerializerInput input, object context);
        
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
    }
}