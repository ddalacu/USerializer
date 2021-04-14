using System;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(SByteSerializer))]

namespace USerialization
{

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class SByteSerializer : DataSerializer, ICustomSerializer
    {
        public Type SerializedType => typeof(sbyte);

        public void Initialize(USerializer serializer)
        {

        }

        public DataSerializer GetMethods()
        {
            return this;
        }

        public SByteSerializer() : base(DataType.SByte)
        {
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var value = *(byte*)(fieldAddress);
            output.WriteByte(value);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            var value = (byte*)(fieldAddress);
            *value = input.ReadByte();
        }
    }
}