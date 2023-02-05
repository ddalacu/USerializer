using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(UInt64), typeof(UInt64Serializer))]

namespace USerialization
{
    public sealed class UInt64Serializer : GenericUnmanagedSerializer<UInt64, UInt64DataTypeLogic>
    {
        
    }

    public sealed class UInt64DataTypeLogic : UnmanagedDataTypeLogic<UInt64>
    {
    }
}