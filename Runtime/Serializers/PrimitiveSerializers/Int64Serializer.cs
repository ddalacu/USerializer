using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(Int64Serializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class Int64Serializer : DataSerializer, ICustomSerializer
    {
        public Type SerializedType => typeof(long);

        public void Initialize(USerializer serializer)
        {

        }

        public DataSerializer GetMethods()
        {
            return this;
        }

        public Int64Serializer() : base(DataType.Int64)
        {
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var value = *(long*)(fieldAddress);
            output.WriteInt64(value);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            var value = (long*)(fieldAddress);
            *value = input.ReadInt64();
        }
    }
}