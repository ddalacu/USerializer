using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(UInt64Serializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class UInt64Serializer : DataSerializer, ICustomSerializer
    {
        public Type SerializedType => typeof(ulong);

        public void Initialize(USerializer serializer)
        {

        }

        public DataSerializer GetMethods()
        {
            return this;
        }

        public UInt64Serializer() : base(DataType.UInt64)
        {
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var value = *(ulong*)(fieldAddress);
            output.WriteUInt64(value);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            var value = (ulong*)(fieldAddress);
            *value = input.ReadUInt64();
        }
    }
}