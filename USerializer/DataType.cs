namespace USerialization
{
    /// <summary>
    /// Wrote on the stream after the field hash and before the value check <see cref="ClassSerializationProvider"/> <see cref="StructSerializationProvider"/>
    /// </summary>
    public enum DataType : byte
    {
        None = 0,
    }

    public unsafe class UnmanagedDataTypeLogic<T> : IDataTypeLogic where T : unmanaged
    {
        public DataType Value { get; set; }

        public void Skip(SerializerInput input)
        {
            input.Skip(sizeof(T));
        }
    }

    public sealed class ObjectDataTypeLogic : IDataTypeLogic
    {
        public DataType Value { get; set; }

        public void Skip(SerializerInput input)
        {
            var toSkip = input.Read<int>();
            if (toSkip == -1)//null
                return;

            input.Skip(toSkip);
        }
    }

    public sealed class ArrayDataTypeLogic : IDataTypeLogic
    {
        public DataType Value { get; set; }

        public void Skip(SerializerInput input)
        {
            var toSkip = input.Read<int>();
            if (toSkip == -1)//null
                return;

            input.Skip(toSkip);
        }
    }


}