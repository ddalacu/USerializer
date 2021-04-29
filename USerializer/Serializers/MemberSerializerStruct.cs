using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    public readonly struct MemberSerializerStruct
    {
        public readonly int Hash;
        public readonly DataSerializer DataSerializer;

        public MemberSerializerStruct(int hash, DataSerializer dataSerializer)
        {
            Hash = hash;
            DataSerializer = dataSerializer;
        }

    }

    public static class MemberSerializerStructExtensions
    {
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteFields(this MemberSerializerStruct[] members, byte* fieldAddress, SerializerOutput output)
        {
            var size = members.Length;
            output.WriteByte((byte)size);

            for (var i = 0; i < size; i++)
            {
                output.EnsureNext(5);
                var field = members[i];
                var fieldDataSerializer = field.DataSerializer;
                output.WriteIntUnchecked(field.Hash);
                output.WriteByteUnchecked((byte)fieldDataSerializer.GetDataType());
                fieldDataSerializer.WriteDelegate(fieldAddress, output);
            }
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReadFields(this MemberSerializerStruct[] members, byte* fieldAddress, SerializerInput input, DataTypesDatabase dataTypesDatabase)
        {
            var fieldsCount = input.ReadByte();
            var size = members.Length;

            int searchStart = 0;

            for (var i = 0; i < fieldsCount; i++)
            {
                var field = input.ReadInt();
                var type = (DataType)input.ReadByte();

                var deserialized = false;

                for (var searchIndex = searchStart; searchIndex < size; searchIndex++)
                {
                    var fieldData = members[searchIndex];
                    var dataSerializer = fieldData.DataSerializer;

                    if (field == fieldData.Hash)
                    {
                        if (type == dataSerializer.GetDataType())
                        {
                            dataSerializer.ReadDelegate(fieldAddress, input);
                            deserialized = true;
                        }

                        searchStart = searchIndex + 1;
                        break;
                    }
                }

                if (deserialized == false)
                {
                    dataTypesDatabase.SkipData(type, input);
                }
            }
        }

    }

}