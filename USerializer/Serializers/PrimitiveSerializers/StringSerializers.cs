using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(string), typeof(StringSerializer))]

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class StringSerializer : CustomDataSerializer
    {
        private DataType _dataType;

        public override DataType GetDataType() => _dataType;

        public override bool TryInitialize(USerializer serializer)
        {
            var typeLogic = serializer.DataTypesDatabase;

            if (typeLogic.TryGet(out StringDataTypeLogic arrayDataTypeLogic) == false)
                return false;

            _dataType = arrayDataTypeLogic.Value;
            return true;
        }

        public override unsafe void Write(void* fieldAddress, SerializerOutput output, object context)
        {
            var value = Unsafe.Read<string>(fieldAddress);

            if (value == null)
            {
                //Write7BitEncodedInt(0);
                output.WriteByte(0);
                return;
            }

            var valueLength = value.Length;
            var byteLength = valueLength * sizeof(char);

            output.EnsureNext(byteLength + 5); //5 if from the max size of Write7BitEncodedIntUnchecked
            output.Write7BitEncodedIntUnchecked(valueLength + 1);

            fixed (void* textPtr = value)
            {
                output.WriteBytesUnchecked(textPtr, byteLength);
            }
        }

        public override unsafe void Read(void* fieldAddress, SerializerInput input, object context)
        {
            ref var value = ref Unsafe.AsRef<string>(fieldAddress);

            var length = input.Read7BitEncodedInt();

            length -= 1;

            if (length == -1)
            {
                value = null;
                return;
            }

#if DEBUG
            if (length < 0)
                throw new Exception("byteLength is negative!");
#endif
            
            if (length == 0)
            {
                value = string.Empty;
                return;
            }
            
            var chars = input.GetNext<char>(length);
            value = new string(chars);
        }
    }

    public sealed class StringDataTypeLogic : IDataTypeLogic
    {
        public DataType Value { get; set; }

        public void Skip(SerializerInput input)
        {
            var chars = input.Read7BitEncodedInt();

            chars -= 1;

            if (chars == -1) //null
                return;

            input.Skip(chars * sizeof(char));
        }
    }
}