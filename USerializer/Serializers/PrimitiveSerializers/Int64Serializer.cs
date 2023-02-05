using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(Int64), typeof(Int64Serializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class Int64Serializer : GenericUnmanagedSerializer<Int64, Int64DataTypeLogic>
    {
        
    }

    public sealed class Int64DataTypeLogic : UnmanagedDataTypeLogic<Int64>
    {

    }

}