using System;
using System.Runtime.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(BoolSerializer))]

namespace USerialization
{
    public sealed class BoolSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(bool);

        public USerializer Serializer { get; set; }


        public DataType DataType => DataType.Boolean;

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(bool*)(fieldAddress);

            if (value)
                output.WriteByte(1);
            else
                output.WriteByte(0);
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<bool>(fieldAddress);
            value = input.ReadByte() == 1;
        }
    }

}