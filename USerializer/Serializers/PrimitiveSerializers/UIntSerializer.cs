using System;
using USerialization;

[assembly: CustomSerializer(typeof(uint), typeof(UIntSerializer))]

namespace USerialization
{
    public sealed class UIntSerializer : GenericUnmanagedSerializer<uint, UInt32DataTypeLogic>
    {
        
    }

    public sealed class UInt32DataTypeLogic : UnmanagedDataTypeLogic<UInt32>
    {

    }

}