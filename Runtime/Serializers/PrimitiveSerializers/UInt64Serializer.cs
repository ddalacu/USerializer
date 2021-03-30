using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(UInt64Serializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class UInt64Serializer : ICustomSerializer
    {
        public Type SerializedType => typeof(ulong);

        public static unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(ulong*)(fieldAddress);
            output.WriteUInt64(value);
        }

        public static unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            var value = (ulong*)(fieldAddress);
            *value = input.ReadUInt64();
        }

        public void Initialize(USerializer serializer)
        {
            
        }

        public unsafe SerializationMethods GetMethods()
        {
            return new SerializationMethods(Write, Read, DataType.UInt64);
        }
    }
}