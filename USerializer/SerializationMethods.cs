namespace USerialization
{
    public abstract unsafe class DataSerializer
    {
        private bool _initialized;

        public abstract DataType GetDataType();

        public abstract void WriteDelegate(void* fieldAddress, SerializerOutput output);

        public abstract void ReadDelegate(void* fieldAddress, SerializerInput input);


        public void RootInitialize(USerializer serializer)
        {
            if (_initialized)
                return;

            _initialized = true;

            Initialize(serializer);

            var dataType = GetDataType();

            if (dataType == DataType.None)
                serializer.Logger.Error($"Data type is none {this}, something went wrong!");
        }

        protected abstract void Initialize(USerializer serializer);
    }

}