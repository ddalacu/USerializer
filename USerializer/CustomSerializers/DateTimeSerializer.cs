using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using USerialization;

[assembly: CustomSerializer(typeof(DateTime), typeof(DateTimeSerializer))]

namespace USerialization
{
    public class DateTimeSerializer : CustomDataSerializer
    {
        public override DataType DataType => DataType.Int64;

        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output)
        {
            ref var item = ref Unsafe.As<byte, DateTime>(ref MemoryMarshal.GetReference(span));
            output.Write<long>(item.ToBinary());
        }

        public override void Read(Span<byte> span, ref SerializerInput input)
        {
            ref var item = ref Unsafe.As<byte, DateTime>(ref MemoryMarshal.GetReference(span));
            item = DateTime.FromBinary(input.Read<long>());
        }
    }
}