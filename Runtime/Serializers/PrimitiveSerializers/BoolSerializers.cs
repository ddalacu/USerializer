using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(BoolSerializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class BoolSerializer : CustomDataSerializer
    {
        public override Type SerializedType => typeof(bool);

        public BoolSerializer() : base(DataType.Boolean)
        {
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var value = *(bool*)(fieldAddress);
            output.WriteByte(value ? (byte)1 : (byte)0);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            var value = (bool*)(fieldAddress);
            *value = input.ReadByte() == 1;
        }
    }
}