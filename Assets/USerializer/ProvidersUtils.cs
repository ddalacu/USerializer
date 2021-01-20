namespace USerialization
{
    public static class ProvidersUtils
    {
        public static ISerializationProvider[] GetDefaultProviders(ISerializationProvider[] additionalProviders = null)
        {
            ISerializationProvider[] providers =
            {
                new CustomSerializerProvider(),
                new EnumSerializer(),
                new ClassSerializer(),
                new StructSerializer(),
                new ArraySerializer(),
                new ListSerializer(),
                //new DictionarySerializer()
            };

            if (additionalProviders == null) 
                return providers;

            var providersLength = providers.Length;
            var extended = new ISerializationProvider[providersLength + additionalProviders.Length];

            for (var i = 0; i < providersLength; i++)
                extended[i] = providers[i];

            for (var i = 0; i < additionalProviders.Length; i++)
                extended[i + providers.Length] = additionalProviders[i];

            providers = extended;

            return providers;
        }
    }
}