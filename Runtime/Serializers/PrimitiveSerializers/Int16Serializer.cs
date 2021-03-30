using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(Int16Serializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class Int16Serializer : ICustomSerializer
    {
        public Type SerializedType => typeof(short);

        public static unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(short*)(fieldAddress);
            output.WriteInt16(value);
        }

        public static unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            var value = (short*)(fieldAddress);
            *value = input.ReadInt16();
        }

        public void Initialize(USerializer serializer)
        {

        }

        public unsafe SerializationMethods GetMethods()
        {
            return new SerializationMethods(Write, Read, DataType.Int16);
        }
    }
}