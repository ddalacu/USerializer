namespace USerialization
{
    /// <summary>
    /// Wrote on the stream after the field hash and before the value check <see cref="ClassSerializer"/> <see cref="StructSerializer"/>
    /// </summary>
    public enum DataType : byte
    {
        Byte = 1,
        SByte = 2,
        Char = 3,
        Boolean = 4,
        Int16 = 5,
        Int32 = 6,
        Int64 = 7,
        UInt16 = 8,
        UInt32 = 9,
        UInt64 = 10,
        Single = 11,
        Double = 12,
        String = 13,
        Array = 14,
        Object = 15
    }
}