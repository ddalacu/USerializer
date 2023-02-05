using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(int), typeof(IntSerializer))]
[assembly: CustomSerializer(typeof(int[]), typeof(IntArraySerializer))]
[assembly: CustomSerializer(typeof(List<int>), typeof(IntListSerializer))]


namespace USerialization
{
    public sealed class Int32DataTypeLogic : UnmanagedDataTypeLogic<int>
    {
        
    }

    public sealed class IntSerializer : GenericUnmanagedSerializer<int, Int32DataTypeLogic>
    {
        
    }

    public sealed class IntArraySerializer : GenericUnmanagedArraySerializer<int, Int32DataTypeLogic>
    {
        
    }

    public sealed class IntListSerializer : GenericUnmanagedListSerializer<int, Int32DataTypeLogic>
    {
        
    }
}