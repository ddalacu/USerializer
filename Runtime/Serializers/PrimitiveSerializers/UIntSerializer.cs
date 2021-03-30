using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(UIntSerializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class UIntSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(uint);

        public static unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(uint*)(fieldAddress);
            output.WriteUInt(value);
        }

        public static unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            var value = (uint*)(fieldAddress);
            *value = input.ReadUInt();
        }

        public void Initialize(USerializer serializer)
        {
            
        }

        public unsafe SerializationMethods GetMethods()
        {
            return new SerializationMethods(Write, Read, DataType.UInt32);
        }
    }
}