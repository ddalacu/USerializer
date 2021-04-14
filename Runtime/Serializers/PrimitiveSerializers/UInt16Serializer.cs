using System;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(UInt16Serializer))]
namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class UInt16Serializer : DataSerializer, ICustomSerializer
    {
        public Type SerializedType => typeof(ushort);

        public void Initialize(USerializer serializer)
        {

        }

        public DataSerializer GetMethods()
        {
            return this;
        }

        public UInt16Serializer() : base(DataType.UInt16)
        {

        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var value = *(ushort*)(fieldAddress);
            output.WriteUInt16(value);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            var value = (ushort*)(fieldAddress);
            *value = input.ReadUInt16();
        }
    }
}