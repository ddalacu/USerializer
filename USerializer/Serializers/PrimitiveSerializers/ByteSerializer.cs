using System.Collections.Generic;
using USerialization;

[assembly: CustomSerializer(typeof(byte), typeof(ByteSerializer))]
[assembly: CustomSerializer(typeof(byte[]), typeof(ByteArraySerializer))]
[assembly: CustomSerializer(typeof(List<byte>), typeof(ByteListSerializer))]

namespace USerialization
{
    public sealed class ByteDataTypeLogic : UnmanagedDataTypeLogic<byte>
    {
        
    }

    public sealed class ByteSerializer : GenericUnmanagedSerializer<byte, ByteDataTypeLogic>
    {
    }

    public sealed class ByteArraySerializer : GenericUnmanagedArraySerializer<byte, ByteDataTypeLogic>
    {
    }

    public sealed class ByteListSerializer : GenericUnmanagedListSerializer<byte, ByteDataTypeLogic>
    {
    }
}