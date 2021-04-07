using System;
using USerialization;

[assembly: CustomSerializer(typeof(DateTimeSerializer))]

namespace USerialization
{
    public sealed class DateTimeSerializer : SurrogateSerializerBase<DateTime, long>
    {
        public override void CopyToSurrogate(ref DateTime @from, ref long to)
        {
            to = from.ToBinary();
        }

        public override void CopyFromSurrogate(ref long @from, ref DateTime to)
        {
            to = DateTime.FromBinary(from);
        }
    }

}