namespace USerialization
{
    /// <summary>
    /// Groups together <see cref="WriteDelegate"/> and <see cref="ReadDelegate"/> together
    /// </summary>
    public readonly struct SerializationMethods
    {
        public readonly WriteDelegate Serialize;

        public readonly ReadDelegate Deserialize;

        public readonly DataType DataType;

        public SerializationMethods(WriteDelegate serialize, ReadDelegate deserialize, DataType dataType)
        {
            Serialize = serialize;
            Deserialize = deserialize;
            DataType = dataType;
        }
    }
}