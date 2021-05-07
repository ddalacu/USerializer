namespace USerialization
{
    public static class ProvidersUtils
    {
        public static ISerializationProvider[] GetDefaultProviders(ILogger logger)
        {
            ISerializationProvider[] providers =
            {
                new CustomSerializerProvider(logger),
                new EnumSerializer(),
                new ArraySerializer(),
                new ListSerializer(),
                new ClassSerializationProvider(),
                new StructSerializationProvider(),
            };

            return providers;
        }
    }
}