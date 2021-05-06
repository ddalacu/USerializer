using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{

    [StructLayout(LayoutKind.Auto)]
    public readonly struct FieldData
    {
        public readonly int FieldNameHash;
        public readonly ushort Offset;
        public readonly DataSerializer SerializationMethods;

        public readonly int[] AlternateHashes;

        public FieldData(string fieldName, string[] alternateFieldNames, DataSerializer serializationMethods, ushort offset)
        {
            SerializationMethods = serializationMethods;
            Offset = offset;
            FieldNameHash = fieldName.GetInt32Hash();

            if (alternateFieldNames != null &&
                alternateFieldNames.Length != 0)
            {
                var length = alternateFieldNames.Length;

                AlternateHashes = new int[length];

                for (var index = 0; index < length; index++)
                    AlternateHashes[index] = alternateFieldNames[index].GetInt32Hash();
            }
            else
            {
                AlternateHashes = null;
            }
        }
    }

    public static class FieldDataExtensions
    {
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void WriteFields(this FieldData[] fieldDatas, byte* objectAddress, SerializerOutput output)
        {
            var fieldsCount = fieldDatas.Length;

            output.WriteByte((byte)fieldsCount);

            for (var index = 0; index < fieldsCount; index++)
            {
                var fieldData = fieldDatas[index];

                output.EnsureNext(5);
                output.WriteIntUnchecked(fieldData.FieldNameHash);
                var dataSerializer = fieldData.SerializationMethods;
                output.WriteByteUnchecked((byte)dataSerializer.GetDataType());
                dataSerializer.WriteDelegate(objectAddress + fieldData.Offset, output);
            }
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe void ReadFields(this FieldData[] fieldDatas, byte* objectAddress, SerializerInput input, DataTypesDatabase dataTypesDatabase)
        {
            var fieldCount = input.ReadByte();
            var fieldsLength = fieldDatas.Length;

            int searchStart = 0;
            for (var i = 0; i < fieldCount; i++)
            {
                var field = input.ReadInt();

                var type = (DataType)input.ReadByte();

                var deserialized = false;

                for (var searchIndex = searchStart; searchIndex < fieldsLength; searchIndex++)
                {
                    var fieldData = fieldDatas[searchIndex];

                    if (field == fieldData.FieldNameHash)
                    {
                        var dataSerializer = fieldData.SerializationMethods;

                        if (type == dataSerializer.GetDataType())
                        {
                            var fieldDataOffset = objectAddress + fieldData.Offset;
                            dataSerializer.ReadDelegate(fieldDataOffset, input);
                            deserialized = true;
                        }

                        searchStart = searchIndex + 1;
                        break;
                    }
                }

                if (deserialized == false)
                {
                    if (FieldsData.GetAlternate(fieldDatas, type, field, out var alternate))
                    {
                        alternate.SerializationMethods.ReadDelegate(objectAddress + alternate.Offset, input);
                    }
                    else
                    {
                        //Debug.Log($"Skipping field of type {type}");
                        dataTypesDatabase.SkipData(type, input);
                    }
                }
            }
        }

    }

}