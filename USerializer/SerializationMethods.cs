namespace USerialization
{
    public abstract unsafe class DataSerializer
    {
        public abstract DataType GetDataType();

        public abstract void WriteDelegate(void* fieldAddress, SerializerOutput output);

        public abstract void ReadDelegate(void* fieldAddress, SerializerInput input);


        private bool _initialized;

        public void RootInitialize(USerializer serializer)
        {
            if(_initialized)
                return;
            _initialized = true;
            Initialize(serializer);
        }

        protected abstract void Initialize(USerializer serializer);
    }

}