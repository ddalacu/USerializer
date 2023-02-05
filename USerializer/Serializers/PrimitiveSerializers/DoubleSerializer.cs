using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(double), typeof(DoubleSerializer))]
[assembly: CustomSerializer(typeof(double[]), typeof(DoubleArraySerializer))]
[assembly: CustomSerializer(typeof(List<double>), typeof(DoubleListSerializer))]

namespace USerialization
{
    public sealed class DoubleSerializer : GenericUnmanagedSerializer<double, DoubleDataTypeLogic>
    {
    }

    public sealed class DoubleArraySerializer : GenericUnmanagedArraySerializer<double, DoubleDataTypeLogic>
    {
    }

    public sealed class DoubleListSerializer : GenericUnmanagedListSerializer<double, DoubleDataTypeLogic>
    {
    }

    public sealed class DoubleDataTypeLogic : UnmanagedDataTypeLogic<double>
    {

    }
}