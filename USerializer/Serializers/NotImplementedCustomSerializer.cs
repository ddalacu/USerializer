using System;

namespace USerialization
{
    /// <summary>
    /// Simply throws a exception when trying to write or read
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public unsafe class NotImplementedCustomSerializer<T> : CustomDataSerializer
    {
        public override DataType GetDataType() => DataType.None;

        public override void Write(ReadOnlySpan<byte> span, SerializerOutput output, object context)
        {
            throw new NotImplementedException();
        }

        public override void Read(Span<byte> span, SerializerInput input, object context)
        {
            throw new NotImplementedException();
        }
    }
}