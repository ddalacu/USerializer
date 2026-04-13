namespace USerialization
{
    public static class ProvidersUtils
    {
        public static ISerializationProvider[] GetDefaultProviders()
        {
            ISerializationProvider[] providers =
            {
                new PrimitivesSerializerProvider(),
                new EnumSerializer(),
                new ArraySerializer(),
                new ListSerializer(),
                new TupleSerializationProvider(),
                new ClassSerializationProvider(),
                new StructSerializationProvider(),
                new KeyValuePairSerializationProvider(),
                new DictionarySerializerProvider(),
            };

            return providers;
        }
    }
}