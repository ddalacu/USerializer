namespace USerialization
{
    /// <summary>
    /// Wrote on the stream after the field hash and before the value check <see cref="ClassSerializationProvider"/> <see cref="StructSerializationProvider"/>
    /// </summary>
    public enum DataType : byte
    {
        None = 0,
        Object = 1,
        Array = 2,
        String = 3,
        Byte = 4,
        SByte = 5,
        Char = 6,
        Bool = 7,
        Int16 = 8,
        Int32 = 9,
        Int64 = 10,
        UInt16 = 11,
        UInt32 = 12,
        UInt64 = 13,
        Float = 14,
        Double = 15
    }

    public unsafe class UnmanagedDataSkipper<T> : IDataSkipper where T : unmanaged
    {
        public void Skip(SerializerInput input)
        {
            input.Skip(sizeof(T));
        }
    }

    public sealed class ObjectDataSkipper : IDataSkipper
    {
        public void Skip(SerializerInput input)
        {
            var toSkip = input.Read<int>();
            if (toSkip == -1)//null
                return;

            input.Skip(toSkip);
        }
    }

    public sealed class ArrayDataSkipper : IDataSkipper
    {
        public void Skip(SerializerInput input)
        {
            var toSkip = input.Read<int>();
            if (toSkip == -1)//null
                return;

            input.Skip(toSkip);
        }
    }


}