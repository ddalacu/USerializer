using USerialization;

[assembly: CustomSerializer(typeof(float), typeof(FloatSerializer))]

namespace USerialization
{
    public sealed class FloatSerializer : GenericUnmanagedSerializer<float, SingleDataTypeLogic>
    {
        
    }
    
    public sealed class SingleDataTypeLogic : UnmanagedDataTypeLogic<float>
    {

    }
}