using System.Collections.Generic;
using USerialization;

[assembly: CustomSerializer(typeof(char), typeof(CharSerializer))]
[assembly: CustomSerializer(typeof(char[]), typeof(CharArraySerializer))]
[assembly: CustomSerializer(typeof(List<char>), typeof(CharListSerializer))]

namespace USerialization
{
    public sealed class CharSerializer : GenericUnmanagedSerializer<char, CharDataTypeLogic>
    {
    }

    public sealed class CharArraySerializer : GenericUnmanagedArraySerializer<char, CharDataTypeLogic>
    {
    }

    public sealed class CharListSerializer : GenericUnmanagedListSerializer<char, CharDataTypeLogic>
    {
    }

    public sealed class CharDataTypeLogic : UnmanagedDataTypeLogic<char>
    {
    }
}