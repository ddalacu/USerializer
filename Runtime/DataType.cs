using System;

namespace USerialization
{
    /// <summary>
    /// Wrote on the stream after the field hash and before the value check <see cref="ClassSerializationProvider"/> <see cref="StructSerializationProvider"/>
    /// </summary>
    public enum DataType : byte
    {
        None = 0,
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

    public unsafe class UnmanagedDataTypeLogic<T> : IDataTypeLogic where T : unmanaged
    {
        public DataType Value { get; protected set; }

        public UnmanagedDataTypeLogic(DataType dataType)
        {
            Value = dataType;
        }

        public void Skip(SerializerInput input)
        {
            input.Skip(sizeof(T));
        }
    }

    public sealed class ByteDataTypeLogic : UnmanagedDataTypeLogic<byte>
    {
        public ByteDataTypeLogic() : base(DataType.Byte)
        {

        }
    }

    public sealed class SByteDataTypeLogic : UnmanagedDataTypeLogic<sbyte>
    {
        public SByteDataTypeLogic() : base(DataType.SByte)
        {

        }
    }

    public sealed class CharDataTypeLogic : UnmanagedDataTypeLogic<char>
    {
        public CharDataTypeLogic() : base(DataType.Char)
        {

        }
    }

    public sealed class BooleanDataTypeLogic : UnmanagedDataTypeLogic<bool>
    {
        public BooleanDataTypeLogic() : base(DataType.Boolean)
        {

        }
    }

    public sealed class Int16DataTypeLogic : UnmanagedDataTypeLogic<Int16>
    {
        public Int16DataTypeLogic() : base(DataType.Int16)
        {

        }
    }

    public sealed class Int32DataTypeLogic : UnmanagedDataTypeLogic<Int32>
    {
        public Int32DataTypeLogic() : base(DataType.Int32)
        {

        }
    }

    public sealed class Int64DataTypeLogic : UnmanagedDataTypeLogic<Int64>
    {
        public Int64DataTypeLogic() : base(DataType.Int64)
        {

        }
    }

    public sealed class UInt16DataTypeLogic : UnmanagedDataTypeLogic<UInt16>
    {
        public UInt16DataTypeLogic() : base(DataType.UInt16)
        {

        }
    }

    public sealed class UInt32DataTypeLogic : UnmanagedDataTypeLogic<UInt32>
    {
        public UInt32DataTypeLogic() : base(DataType.UInt32)
        {

        }
    }

    public sealed class UInt64DataTypeLogic : UnmanagedDataTypeLogic<UInt64>
    {
        public UInt64DataTypeLogic() : base(DataType.UInt64)
        {

        }
    }

    public sealed class SingleDataTypeLogic : UnmanagedDataTypeLogic<float>
    {
        public SingleDataTypeLogic() : base(DataType.Single)
        {

        }
    }

    public sealed class DoubleDataTypeLogic : UnmanagedDataTypeLogic<double>
    {
        public DoubleDataTypeLogic() : base(DataType.Double)
        {

        }
    }

    public sealed class StringDataTypeLogic : IDataTypeLogic
    {
        public DataType Value => DataType.String;

        public void Skip(SerializerInput input)
        {
            var chars = input.Read7BitEncodedInt();

            chars -= 1;

            if (chars == -1)//null
                return;

            input.Skip(chars * sizeof(char));
        }
    }

    public sealed class ObjectDataTypeLogic : IDataTypeLogic
    {
        public DataType Value => DataType.Object;

        public void Skip(SerializerInput input)
        {
            var toSkip = input.ReadInt();
            if (toSkip == -1)//null
                return;

            input.Skip(toSkip);
        }
    }

    public sealed class ArrayDataTypeLogic : IDataTypeLogic
    {
        public DataType Value => DataType.Array;

        public void Skip(SerializerInput input)
        {
            var toSkip = input.ReadInt();
            if (toSkip == -1)//null
                return;

            input.Skip(toSkip);
        }
    }


}