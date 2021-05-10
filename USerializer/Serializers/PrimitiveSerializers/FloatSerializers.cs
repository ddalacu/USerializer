using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(float), typeof(FloatSerializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class FloatSerializer : CustomDataSerializer
    {
        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out SingleDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var value = *(float*)(fieldAddress);
            output.WriteFloat(value);
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            var value = (float*)(fieldAddress);
            *value = input.ReadFloat();
        }
    }

    public sealed class SingleDataTypeLogic : UnmanagedDataTypeLogic<float>
    {

    }
}