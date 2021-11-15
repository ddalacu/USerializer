using System;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(UInt16), typeof(UInt16Serializer))]
namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class UInt16Serializer : CustomDataSerializer
    {
        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out UInt16DataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void Write(void* fieldAddress, SerializerOutput output, object context)
        {
            var value = *(ushort*)(fieldAddress);
            output.WriteUInt16(value);
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input, object context)
        {
            var value = (ushort*)(fieldAddress);
            *value = input.ReadUInt16();
        }
    }

    public sealed class UInt16DataTypeLogic : UnmanagedDataTypeLogic<UInt16>
    {
    }
}