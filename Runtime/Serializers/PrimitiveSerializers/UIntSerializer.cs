using System;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(UIntSerializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class UIntSerializer : CustomDataSerializer
    {
        public override Type SerializedType => typeof(uint);

        public UIntSerializer() : base(DataType.UInt32)
        {
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var value = *(uint*)(fieldAddress);
            output.WriteUInt(value);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            var value = (uint*)(fieldAddress);
            *value = input.ReadUInt();
        }
    }
}