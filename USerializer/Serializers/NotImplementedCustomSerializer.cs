using System;

namespace USerialization
{
    /// <summary>
    /// Simply throws a exception when trying to write or read
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NotImplementedCustomSerializer<T> : CustomDataSerializer
    {
        public override DataType DataType => DataType.None;

        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output)
        {
            throw new NotImplementedException();
        }

        public override void Read(Span<byte> span, ref SerializerInput input)
        {
            throw new NotImplementedException();
        }
    }
}