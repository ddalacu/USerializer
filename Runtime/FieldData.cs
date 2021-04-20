using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine.Serialization;

namespace USerialization
{

    [StructLayout(LayoutKind.Auto)]
    public readonly struct FieldData
    {
        //public readonly FieldInfo FieldInfo;
        public readonly int FieldNameHash;
        public readonly ushort Offset;
        public readonly DataSerializer SerializationMethods;

        public readonly int[] AlternateHashes;

        private static Type _formerlySerializedAsType;

        public FieldData(FieldInfo fieldInfo, DataSerializer serializationMethods, ushort offset)
        {
            SerializationMethods = serializationMethods;
            Offset = offset;
            FieldNameHash = fieldInfo.Name.GetInt32Hash();

            if (_formerlySerializedAsType == null)
                _formerlySerializedAsType = typeof(FormerlySerializedAsAttribute);

            var attributes = (FormerlySerializedAsAttribute[])fieldInfo.GetCustomAttributes(_formerlySerializedAsType, false);

            if (attributes.Length != 0)
            {
                AlternateHashes = new int[attributes.Length];
                for (var index = 0; index < attributes.Length; index++)
                {
                    var formerlySerializedAsAttribute = attributes[index];
                    AlternateHashes[index] = formerlySerializedAsAttribute.oldName.GetInt32Hash();
                }
            }
            else
                AlternateHashes = null;
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
                output.WriteByteUnchecked((byte)fieldData.SerializationMethods.GetDataType());

                fieldData.SerializationMethods.WriteDelegate(objectAddress + fieldData.Offset, output);
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
                    if (TypeData.GetAlternate(fieldDatas, type, field, out var alternate))
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