using System;
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

        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out BooleanDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
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

    public sealed class BooleanDataTypeLogic : UnmanagedDataTypeLogic<bool>
    {
    }
}