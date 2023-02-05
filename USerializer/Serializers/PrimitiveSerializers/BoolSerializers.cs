using System.Collections.Generic;
using USerialization;

[assembly: CustomSerializer(typeof(bool), typeof(BoolSerializer))]
[assembly: CustomSerializer(typeof(bool[]), typeof(BoolArraySerializer))]
[assembly: CustomSerializer(typeof(List<bool>), typeof(BoolListSerializer))]

namespace USerialization
{
    public sealed class BoolSerializer : GenericUnmanagedSerializer<bool, BooleanDataTypeLogic>
    {
    }

    public sealed class BoolArraySerializer : GenericUnmanagedArraySerializer<bool, BooleanDataTypeLogic>
    {
    }

    public sealed class BoolListSerializer : GenericUnmanagedListSerializer<bool, BooleanDataTypeLogic>
    {
    }

    public sealed class BooleanDataTypeLogic : UnmanagedDataTypeLogic<bool>
    {
    }
}