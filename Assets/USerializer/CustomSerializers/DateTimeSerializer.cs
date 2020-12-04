using System;
using USerialization;

[assembly: CustomSerializer(typeof(DateTimeSerializer))]

namespace USerialization
{
    public class DateTimeSerializer : CustomSerializerBase<DateTime>
    {
        public override void LocalInit()
        {
            AddField(1, (ref DateTime dateTime, long var) =>
            {
                dateTime = DateTime.FromBinary(var);
            }, (ref DateTime dateTime) => dateTime.ToBinary());
        }
    }

}