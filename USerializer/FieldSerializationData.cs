using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    public readonly struct FieldMetaData
    {
        public readonly DataType DataType;

        public readonly int FieldNameHash;

        public readonly int[] AlternateHashes;

        public readonly FieldInfo Field;

        public FieldMetaData(string fieldName, string[] alternateFieldNames, FieldInfo info,
            DataSerializer serializationMethods)
        {
            DataType = serializationMethods.GetDataType();

            if (DataType == DataType.None)
                throw new Exception();


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

            Field = info;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public readonly struct FieldSerializationData
    {
        public readonly ushort Offset;

        public readonly DataSerializer DataSerializer;

        public FieldSerializationData(DataSerializer dataSerializer, ushort offset)
        {
            Offset = offset;
            DataSerializer = dataSerializer;

            if (dataSerializer.Initialized == false)
                throw new Exception($"{dataSerializer} not initialized!");
        }


        private static void OrderFields(List<(FieldMetaData Meta, FieldSerializationData SerializationData)> fields)
        {
            var fieldsLength = fields.Count;
            if (fieldsLength > 255)
                throw new Exception();

            //important
            fields.Sort((a, b) =>
            {
                if (a.Meta.FieldNameHash > b.Meta.FieldNameHash)
                    return 1;
                if (a.Meta.FieldNameHash < b.Meta.FieldNameHash)
                    return -1;
                return 0;
            });

            for (var i = 0; i < fieldsLength - 1; i++)
            {
                if (fields[i].Meta.FieldNameHash == fields[i + 1].Meta.FieldNameHash)
                {
                    throw new Exception(
                        $"Field hash collision! {fields[i].Meta.Field.DeclaringType}-{fields[i].Meta.Field} and {fields[i + 1].Meta.Field.DeclaringType}- {fields[i + 1].Meta.Field}");
                }
            }
        }

        public static bool GetAlternate(FieldMetaData[] metas, DataType type, int field, out int compatibleIndex)
        {
            int fieldsLength = metas.Length;

            for (var index = 0; index < fieldsLength; index++)
            {
                var metaData = metas[index];

                if (type != metaData.DataType)
                    continue;

                var alternateHashes = metaData.AlternateHashes;
                if (metaData.AlternateHashes == null)
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

        public static (FieldMetaData[] Metas, FieldSerializationData[] SerializationDatas) GetFields(Type type,
            USerializer uSerializer, bool initializeDataSerializer = true)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            using var fieldsIterator = new TypeFieldsIterator(type, bindingFlags);

            var fields = new List<(FieldMetaData, FieldSerializationData)>(32);

            while (fieldsIterator.MoveNext(out var fieldInfo))
            {
                if (uSerializer.SerializationPolicy.ShouldSerialize(fieldInfo) == false)
                    continue;

                if (uSerializer.TryGetDataSerializer(fieldInfo.FieldType,
                        out var serializationMethods,
                        initializeDataSerializer) == false)
                    continue;

                if (serializationMethods == null)
                    throw new Exception($"Returned null serializer for {fieldInfo.FieldType}");

                var fieldOffset = UnsafeUtils.GetFieldOffset(fieldInfo);
                if (fieldOffset > short.MaxValue)
                    throw new Exception("Field offset way to big!");

                var alternateNames = uSerializer.SerializationPolicy.GetAlternateNames(fieldInfo);

                var fieldSerializationData = new FieldSerializationData(serializationMethods, (ushort) fieldOffset);
                var metaData = new FieldMetaData(fieldInfo.Name, alternateNames, fieldInfo, serializationMethods);

                fields.Add((metaData, fieldSerializationData));
            }

            OrderFields(fields);
            var fieldsCount = fields.Count;

            var splitMetas = new FieldMetaData[fieldsCount];
            var splitDatas = new FieldSerializationData[fieldsCount];

            for (var i = 0; i < fieldsCount; i++)
            {
                splitMetas[i] = fields[i].Item1;
                splitDatas[i] = fields[i].Item2;
            }

            return (splitMetas, splitDatas);
        }
    }

    public readonly unsafe struct FieldsSerializer
    {
        //used more frequently
        private readonly byte[] _headerData;
        private readonly FieldSerializationData[] _fields;


        //used less often
        private readonly DataTypesDatabase _dataTypesDatabase;
        private readonly FieldMetaData[] _fieldsMetas;

        public FieldsSerializer(FieldMetaData[] metas, FieldSerializationData[] fields,
            DataTypesDatabase dataTypesDatabase)
        {
            _fields = fields;
            _fieldsMetas = metas;
            _dataTypesDatabase = dataTypesDatabase;
            _headerData = CreateHeaderData(metas);
        }

        private static byte[] CreateHeaderData(FieldMetaData[] metas)
        {
            var fieldsLength = metas.Length;

            var size = sizeof(byte) + (fieldsLength * 5);
            var headerData = new byte[size];

            int position = 0;
            headerData[position++] = (byte) fieldsLength;

            for (var index = 0; index < fieldsLength; index++)
            {
                var fieldMeta = metas[index];

                var hash = fieldMeta.FieldNameHash;
                headerData[position++] = (byte) hash;
                headerData[position++] = (byte) (hash >> 8);
                headerData[position++] = (byte) (hash >> 16);
                headerData[position++] = (byte) (hash >> 24);
                //var dataSerializer = fieldData.SerializationMethods;
                var dataType = fieldMeta.DataType;

                headerData[position++] = (byte) dataType;

                if (dataType == DataType.None)
                    throw new Exception("Data type is none!");
            }

            return headerData;
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte* objectAddress, SerializerOutput output, object context)
        {
            output.WriteBytes(_headerData, _headerData.Length);

            var typeDataFields = _fields;
            var fieldsLength = typeDataFields.Length;

            for (var index = 0; index < fieldsLength; index++)
            {
                var fieldData = typeDataFields[index];
                //var dataSerializer = fieldData.SerializationMethods;

                fieldData.DataSerializer.Write(objectAddress + fieldData.Offset, output, context);
            }
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(byte* objectAddress, SerializerInput input, object context)
        {
            var fieldCount = input.ReadByte();
            //we just skipped the required data so we have it in the buffer
            var fieldDatas = _fields;

            var streamData = input.GetNext(fieldCount * 5);
            var localData = new ReadOnlySpan<byte>(_headerData, 1, _headerData.Length - 1);
            
            if (streamData.SequenceEqual(localData))
            {
                for (var i = 0; i < fieldCount; i++)
                {
                    var fieldData = fieldDatas[i];
                    var fieldDataOffset = objectAddress + fieldData.Offset;
                    fieldData.DataSerializer.Read(fieldDataOffset, input, context);
                }
            }
            else
            {
                var indexes = stackalloc byte[fieldCount];
                var dataTypes = stackalloc DataType[fieldCount];

                var fieldsLength = fieldDatas.Length;

                var position = 0;
                
                int searchStart = 0;
                for (var i = 0; i < fieldCount; i++)
                {
                    dataTypes[i] = 0;

                    var field = streamData[position++] | streamData[position++] << 8 | streamData[position++] << 16 | streamData[position++] << 24;

                    var type = (DataType) streamData[position++];

                    var deserialized = false;

                    for (var searchIndex = searchStart; searchIndex < fieldsLength; searchIndex++)
                    {
                        //var fieldData = fieldDatas[searchIndex];
                        var meta = _fieldsMetas[searchIndex];

                        if (field == meta.FieldNameHash)
                        {
                            //var dataSerializer = fieldData.SerializationMethods;

                            if (type == meta.DataType)
                            {
                                indexes[i] = (byte) searchIndex;
                                deserialized = true;
                            }

                            searchStart = searchIndex + 1;
                            break;
                        }
                    }

                    if (deserialized == false)
                    {
                        if (FieldSerializationData.GetAlternate(_fieldsMetas, type, field, out var alternateIndex))
                        {
                            indexes[i] = (byte) alternateIndex;
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

                    if ((byte) dataTypes[i] != 0)
                    {
                        _dataTypesDatabase.SkipData(dataTypes[i], input);
                        continue;
                    }

                    var fieldData = fieldDatas[index];
                    var fieldDataOffset = objectAddress + fieldData.Offset;
                    //var dataSerializer = fieldData.SerializationMethods;
                    //dataSerializer.Read(fieldDataOffset, input);

                    fieldData.DataSerializer.Read(fieldDataOffset, input, context);
                }
            }
        }
    }
}