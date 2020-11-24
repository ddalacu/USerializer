using System;
using System.Runtime.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(IntSerializer))]

namespace USerialization
{
    public sealed class IntSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(int);
        public USerializer Serializer { get; set; }

        public DataType DataType => DataType.Int32;

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(int*)(fieldAddress);
            output.WriteInt(value);
        }
        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<int>(fieldAddress);
            value = input.ReadInt();
        }
    }
}