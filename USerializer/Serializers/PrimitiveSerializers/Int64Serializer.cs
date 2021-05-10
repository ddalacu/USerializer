using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(Int64), typeof(Int64Serializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class Int64Serializer : CustomDataSerializer
    {
        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out Int64DataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(long*)(fieldAddress);
            output.WriteInt64(value);
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            var value = (long*)(fieldAddress);
            *value = input.ReadInt64();
        }
    }

    public sealed class Int64DataTypeLogic : UnmanagedDataTypeLogic<Int64>
    {

    }

}