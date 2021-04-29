﻿using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(Int16Serializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class Int16Serializer : CustomDataSerializer
    {
        public override Type SerializedType => typeof(short);

        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out Int16DataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var value = *(short*)(fieldAddress);
            output.WriteInt16(value);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            var value = (short*)(fieldAddress);
            *value = input.ReadInt16();
        }
    }

    public sealed class Int16DataTypeLogic : UnmanagedDataTypeLogic<Int16>
    {
    }

}