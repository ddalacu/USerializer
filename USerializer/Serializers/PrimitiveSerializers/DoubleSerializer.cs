using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(double), typeof(DoubleSerializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class DoubleSerializer : CustomDataSerializer
    {
        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out DoubleDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(double*)(fieldAddress);
            output.WriteDouble(value);
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            var value = (double*)(fieldAddress);
            *value = input.ReadDouble();
        }
    }

    public sealed class DoubleDataTypeLogic : UnmanagedDataTypeLogic<double>
    {

    }
}