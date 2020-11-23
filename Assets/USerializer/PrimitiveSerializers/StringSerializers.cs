using System;
using System.Runtime.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(StringSerializer))]

namespace USerialization
{
    public sealed class StringSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(string);
        public USerializer Serializer { get; set; }

        public DataType DataType => DataType.String;

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var obj = Unsafe.Read<string>(fieldAddress);
            output.WriteString(obj);
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<string>(fieldAddress);
            value = input.ReadString();
        }
    }
}