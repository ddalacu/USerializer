using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(UInt64), typeof(UInt64Serializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class UInt64Serializer : CustomDataSerializer
    {
        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out UInt64DataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void Write(void* fieldAddress, SerializerOutput output, object context)
        {
            var value = *(ulong*)(fieldAddress);
            output.WriteUInt64(value);
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input, object context)
        {
            var value = (ulong*)(fieldAddress);
            *value = input.ReadUInt64();
        }
    }

    public sealed class UInt64DataTypeLogic : UnmanagedDataTypeLogic<UInt64>
    {
    }
}