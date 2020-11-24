using System;
using System.Runtime.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(UInt16Serializer))]
namespace USerialization
{
    public sealed class UInt16Serializer : ICustomSerializer
    {
        public Type SerializedType => typeof(ushort);

        public USerializer Serializer { get; set; }

        public DataType DataType => DataType.UInt16;

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(ushort*)(fieldAddress);
            output.WriteUInt16(value);
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<ushort>(fieldAddress);
            value = input.ReadUInt16();
        }
    }
}