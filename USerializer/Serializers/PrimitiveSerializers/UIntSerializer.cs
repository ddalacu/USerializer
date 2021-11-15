using System;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(uint), typeof(UIntSerializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class UIntSerializer : CustomDataSerializer
    {
        private DataType _dataType;
        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out UInt32DataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void Write(void* fieldAddress, SerializerOutput output, object context)
        {
            var value = *(uint*)(fieldAddress);
            output.WriteUInt(value);
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input, object context)
        {
            var value = (uint*)(fieldAddress);
            *value = input.ReadUInt();
        }
    }

    public sealed class UInt32DataTypeLogic : UnmanagedDataTypeLogic<UInt32>
    {

    }

}