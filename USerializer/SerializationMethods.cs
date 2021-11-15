namespace USerialization
{
    public abstract unsafe class DataSerializer
    {
        public bool Initialized { get; private set; }

        public abstract DataType GetDataType();

        public abstract void Write(void* fieldAddress, SerializerOutput output, object context);

        public abstract void Read(void* fieldAddress, SerializerInput input, object context);
        
        public void RootInitialize(USerializer serializer)
        {
            if (Initialized)
                return;

            Initialized = true;
            
            Initialize(serializer);

            var dataType = GetDataType();

            if (dataType == DataType.None)
                serializer.Logger.Error($"Data type is none {this}, something went wrong!");
        }

        protected abstract void Initialize(USerializer serializer);
    }
}