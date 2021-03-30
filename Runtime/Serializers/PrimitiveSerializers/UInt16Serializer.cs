using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(UInt16Serializer))]
namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class UInt16Serializer : ICustomSerializer
    {
        public Type SerializedType => typeof(ushort);

        public static unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(ushort*)(fieldAddress);
            output.WriteUInt16(value);
        }

        public static unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            var value = (ushort*)(fieldAddress);
            *value = input.ReadUInt16();
        }

        public void Initialize(USerializer serializer)
        {

        }

        public unsafe SerializationMethods GetMethods()
        {
            return new SerializationMethods(Write, Read, DataType.UInt16);
        }
    }
}