using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(char), typeof(CharSerializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class CharSerializer : CustomDataSerializer
    {
        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out CharDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void Write(void* fieldAddress, SerializerOutput output, object context)
        {
            var value = *(short*)(fieldAddress);
            output.WriteInt16(value);
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input, object context)
        {
            var value = (short*)(fieldAddress);
            *value = input.ReadInt16();
        }
    }

    public sealed class CharDataTypeLogic : UnmanagedDataTypeLogic<char>
    {

    }

}