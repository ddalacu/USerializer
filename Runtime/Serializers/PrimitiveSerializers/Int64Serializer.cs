using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(Int64Serializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class Int64Serializer : ICustomSerializer
    {
        public Type SerializedType => typeof(long);

        public static unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(long*)(fieldAddress);
            output.WriteInt64(value);
        }

        public static unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            var value = (long*)(fieldAddress);
            *value = input.ReadInt64();
        }

        public void Initialize(USerializer serializer)
        {
            
        }

        public unsafe SerializationMethods GetMethods()
        {
            return new SerializationMethods(Write, Read, DataType.Int64);
        }
    }
}