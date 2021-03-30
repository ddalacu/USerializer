using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(SByteSerializer))]

namespace USerialization
{

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class SByteSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(sbyte);

        public static unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(byte*)(fieldAddress);
            output.WriteByte(value);
        }

        public static unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            var value = (byte*)(fieldAddress);
            *value = input.ReadByte();
        }

        public void Initialize(USerializer serializer)
        {
            
        }

        public unsafe SerializationMethods GetMethods()
        {
            return new SerializationMethods(Write, Read, DataType.SByte);
        }
    }
}