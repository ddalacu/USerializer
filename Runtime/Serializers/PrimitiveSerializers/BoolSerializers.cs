using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(BoolSerializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class BoolSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(bool);

        public static unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(bool*)(fieldAddress);
            output.WriteByte(value ? (byte)1 : (byte)0);
        }

        public static unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            var value = (bool*)(fieldAddress);
            *value = input.ReadByte() == 1;
        }

        public void Initialize(USerializer serializer)
        {
            
        }

        public unsafe SerializationMethods GetMethods()
        {
            return new SerializationMethods(Write, Read, DataType.Boolean);
        }
    }
}