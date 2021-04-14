using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(FloatSerializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class FloatSerializer : DataSerializer, ICustomSerializer
    {
        public Type SerializedType => typeof(float);

        public void Initialize(USerializer serializer)
        {

        }

        public DataSerializer GetMethods()
        {
            return this;
        }

        public FloatSerializer() : base(DataType.Single)
        {
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var value = *(float*)(fieldAddress);
            output.WriteFloat(value);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            var value = (float*)(fieldAddress);
            *value = input.ReadFloat();
        }
    }

}