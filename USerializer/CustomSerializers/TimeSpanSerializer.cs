using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using USerialization;

[assembly: CustomSerializer(typeof(TimeSpan), typeof(TimeSpanSerializer))]

namespace USerialization
{
    public class TimeSpanSerializer : CustomDataSerializer
    {
        public override DataType DataType => DataType.Int64;
        
        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output)
        {
            ref var item = ref Unsafe.As<byte, TimeSpan>(ref MemoryMarshal.GetReference(span));
            output.Write<long>(item.Ticks);
        }

        public override void Read(Span<byte> span, ref SerializerInput input)
        {
            ref var item = ref Unsafe.As<byte, TimeSpan>(ref MemoryMarshal.GetReference(span));
            item = TimeSpan.FromTicks(input.Read<long>());
        }
    }
}