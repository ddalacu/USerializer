using System;
using USerialization;

[assembly: CustomSerializer(typeof(UInt16), typeof(UInt16Serializer))]

namespace USerialization
{
    public sealed class UInt16Serializer : GenericUnmanagedSerializer<UInt16, UInt16DataTypeLogic>
    {
    }

    public sealed class UInt16DataTypeLogic : UnmanagedDataTypeLogic<UInt16>
    {
    }
}