using System;
using USerialization;

[assembly: CustomSerializer(typeof(Int16), typeof(Int16Serializer))]

namespace USerialization
{
    public sealed class Int16Serializer : GenericUnmanagedSerializer<Int16, Int16DataTypeLogic>
    {
        
    }

    public sealed class Int16DataTypeLogic : UnmanagedDataTypeLogic<Int16>
    {
    }

}