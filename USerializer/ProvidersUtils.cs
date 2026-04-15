namespace USerialization
{
    public static class ProvidersUtils
    {
        public static ISerializationProvider[] GetDefaultProviders(ILogger logger)
        {
            ISerializationProvider[] providers =
            {
                new PrimitivesSerializerProvider(),
                new CustomSerializerProvider(logger),
                new EnumSerializer(),
                new ArraySerializer(),
                new ListSerializer(),
                new TupleSerializationProvider(),
                new KeyValuePairSerializationProvider(),
                new DictionarySerializerProvider(),
                new ClassSerializationProvider(),
                new StructSerializationProvider(),
            };

            return providers;
        }
    }
}