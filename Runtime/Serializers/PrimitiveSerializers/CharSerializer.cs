using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(CharSerializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class CharSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(char);
        public DataType DataType => DataType.Char;

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
            return new SerializationMethods(Write, Read, DataType.Char);
        }
    }
}