﻿using System;
using System.Reflection;
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


        private static void OrderFields(FieldData[] fields)
        {
            var fieldsLength = fields.Length;
            if (fieldsLength > 255)
                throw new Exception();
            for (var i = 0; i < fieldsLength; i++)
            {
                for (var j = 0; j < fieldsLength; j++)
                {
                    if (i == j)
                        continue;

                    if (fields[i].FieldNameHash == fields[j].FieldNameHash)
                        throw new Exception("Field hash collision!");
                }
            }

            //important
            Array.Sort(fields, (a, b) =>
            {
                if (a.FieldNameHash > b.FieldNameHash)
                    return 1;
                if (a.FieldNameHash < b.FieldNameHash)
                    return -1;
                return 0;
            });
        }

        public static bool GetAlternate(FieldData[] fields, DataType type, int field, out int compatibleIndex)
        {
            int fieldsLength = fields.Length;

            for (var index = 0; index < fieldsLength; index++)
            {
                var fieldData = fields[index];

                if (type != fieldData.SerializationMethods.GetDataType())
                    continue;

                var alternateHashes = fieldData.AlternateHashes;
                if (fieldData.AlternateHashes == null)
                    continue;

                var alternateHashesLength = alternateHashes.Length;
                for (var j = 0; j < alternateHashesLength; j++)
                {
                    if (field == alternateHashes[j])
                    {
                        compatibleIndex = index;
                        return true;
                    }
                }
            }

            compatibleIndex = default;
            return false;
        }

        public static FieldData[] GetFields(Type type, USerializer uSerializer)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            using (var fieldsIterator = new TypeFieldsIterator(8))
            {
                var size = fieldsIterator.Fill(type, bindingFlags);
                var fields = new FieldData[size];
                var index = 0;

                for (var i = 0; i < size; i++)
                {
                    var fieldInfo = fieldsIterator[i];

                    if (uSerializer.SerializationPolicy.ShouldSerialize(fieldInfo) == false)
                        continue;

                    if (uSerializer.TryGetDataSerializer(fieldInfo.FieldType, out var serializationMethods) == false)
                        continue;

                    if (serializationMethods == null)
                        throw new Exception($"Returned null serializer for {fieldInfo.FieldType}");

                    var fieldOffset = UnsafeUtils.GetFieldOffset(fieldInfo);
                    if (fieldOffset > short.MaxValue)
                        throw new Exception("Field offset way to big!");

                    var alternateNames = uSerializer.SerializationPolicy.GetAlternateNames(fieldInfo);

                    fields[index] = new FieldData(fieldInfo.Name, alternateNames, serializationMethods, (ushort)fieldOffset);
                    index++;
                }

                if (index != fields.Length)
                    Array.Resize(ref fields, index);

                OrderFields(fields);

                return fields;
            }
        }

    }

    public readonly unsafe struct FieldsSerializer
    {
        private readonly DataTypesDatabase _dataTypesDatabase;
        private readonly byte[] _headerData;
        private readonly FieldData[] _fields;

        public FieldsSerializer(FieldData[] fields, DataTypesDatabase dataTypesDatabase)
        {
            _fields = fields;
            _dataTypesDatabase = dataTypesDatabase;
            _headerData = CreateHeaderData(fields);
        }

        private static byte[] CreateHeaderData(FieldData[] datas)
        {
            var fieldsLength = datas.Length;

            var size = sizeof(byte) + (fieldsLength * 5);
            var headerData = new byte[size];

            int position = 0;
            headerData[position++] = (byte)fieldsLength;

            for (var index = 0; index < fieldsLength; index++)
            {
                var fieldData = datas[index];
                var hash = fieldData.FieldNameHash;
                headerData[position++] = (byte)hash;
                headerData[position++] = (byte)(hash >> 8);
                headerData[position++] = (byte)(hash >> 16);
                headerData[position++] = (byte)(hash >> 24);
                var dataSerializer = fieldData.SerializationMethods;
                headerData[position++] = (byte)dataSerializer.GetDataType();
            }

            return headerData;
        }
        private static bool UnsafeCompare(byte* p1, byte* p2, int l)
        {
            byte* x1 = p1, x2 = p2;
            for (int i = 0; i < l / 8; i++, x1 += 8, x2 += 8)
                if (*((long*)x1) != *((long*)x2)) return false;
            if ((l & 4) != 0) { if (*((int*)x1) != *((int*)x2)) return false; x1 += 4; x2 += 4; }
            if ((l & 2) != 0) { if (*((short*)x1) != *((short*)x2)) return false; x1 += 2; x2 += 2; }
            if ((l & 1) != 0) if (*((byte*)x1) != *((byte*)x2)) return false;
            return true;
        }

        private bool SameHeader(byte[] arr, int offset, int len)
        {
            if (len == _headerData.Length - 1)
            {
                fixed (byte* arrPtr = arr)
                {
                    fixed (byte* pinned = _headerData)
                    {
                        return UnsafeCompare(pinned + 1, arrPtr + offset, len);
                    }
                }
            }

            return false;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte* objectAddress, SerializerOutput output)
        {
            output.WriteBytes(_headerData, _headerData.Length);

            var typeDataFields = _fields;
            var fieldsLength = typeDataFields.Length;

            for (var index = 0; index < fieldsLength; index++)
            {
                var fieldData = typeDataFields[index];
                var dataSerializer = fieldData.SerializationMethods;
                dataSerializer.Write(objectAddress + fieldData.Offset, output);
            }
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(byte* objectAddress, SerializerInput input)
        {

            var fieldCount = input.ReadByte();
            var size = fieldCount * 5;

            input.Skip(size);
            //we just skipped the required data so we have it in the buffer
            var buffer = input.Buffer;
            var offset = input.PositionInBuffer - size;

            var fieldDatas = _fields;

            if (SameHeader(buffer, offset, size))
            {
                for (int i = 0; i < fieldCount; i++)
                {
                    var fieldData = fieldDatas[i];
                    var fieldDataOffset = objectAddress + fieldData.Offset;
                    var dataSerializer = fieldData.SerializationMethods;
                    dataSerializer.Read(fieldDataOffset, input);
                }
            }
            else
            {
                int position = offset;

                var indexes = stackalloc byte[fieldCount];
                var dataTypes = stackalloc DataType[fieldCount];

                var fieldsLength = fieldDatas.Length;

                int searchStart = 0;
                for (var i = 0; i < fieldCount; i++)
                {
                    dataTypes[i] = 0;

                    var field = buffer[position++] | buffer[position++] << 8 | buffer[position++] << 16 |
                                buffer[position++] << 24;

                    var type = (DataType)buffer[position++];

                    var deserialized = false;

                    for (var searchIndex = searchStart; searchIndex < fieldsLength; searchIndex++)
                    {
                        var fieldData = fieldDatas[searchIndex];

                        if (field == fieldData.FieldNameHash)
                        {
                            var dataSerializer = fieldData.SerializationMethods;

                            if (type == dataSerializer.GetDataType())
                            {
                                indexes[i] = (byte)searchIndex;
                                deserialized = true;
                            }

                            searchStart = searchIndex + 1;
                            break;
                        }
                    }

                    if (deserialized == false)
                    {
                        if (FieldData.GetAlternate(fieldDatas, type, field, out var alternateIndex))
                        {
                            indexes[i] = (byte)alternateIndex;
                        }
                        else
                        {
                            dataTypes[i] = type;
                        }
                    }
                }

                for (var i = 0; i < fieldCount; i++)
                {
                    var index = indexes[i];

                    if ((byte)dataTypes[i] != 0)
                    {
                        _dataTypesDatabase.SkipData(dataTypes[i], input);
                        continue;
                    }

                    var fieldData = fieldDatas[index];
                    var fieldDataOffset = objectAddress + fieldData.Offset;
                    var dataSerializer = fieldData.SerializationMethods;
                    dataSerializer.Read(fieldDataOffset, input);
                }
            }
        }
    }


}