using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(sbyte), typeof(SByteSerializer))]

namespace USerialization
{

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class SByteSerializer : CustomDataSerializer
    {
        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out SByteDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var value = *(byte*)(fieldAddress);
            output.WriteByte(value);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            var value = (byte*)(fieldAddress);
            *value = input.ReadByte();
        }
    }

    public sealed class SByteDataTypeLogic : UnmanagedDataTypeLogic<sbyte>
    {

    }

}