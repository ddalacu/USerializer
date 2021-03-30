using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(FloatSerializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class FloatSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(float);

        public static unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(float*)(fieldAddress);
            output.WriteFloat(value);
        }

        public static unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            var value = (float*)(fieldAddress);
            *value = input.ReadFloat();
        }

        public void Initialize(USerializer serializer)
        {
            
        }

        public unsafe SerializationMethods GetMethods()
        {
            return new SerializationMethods(Write, Read, DataType.Single);
        }
    }

}