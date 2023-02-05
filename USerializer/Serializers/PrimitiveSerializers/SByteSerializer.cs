using USerialization;

[assembly: CustomSerializer(typeof(sbyte), typeof(SByteSerializer))]

namespace USerialization
{
    
    public sealed class SByteSerializer : GenericUnmanagedSerializer<sbyte, SByteDataTypeLogic>
    {
        
    }
    public sealed class SByteDataTypeLogic : UnmanagedDataTypeLogic<sbyte>
    {

    }

}