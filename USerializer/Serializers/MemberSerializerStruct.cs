using System;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace USerialization
{
    public readonly struct FinalMemberSerializerStruct
    {
        public readonly int Hash;
        public readonly DataSerializer Serializer;

        public FinalMemberSerializerStruct(MemberSerializerStruct serializerStruct)
        {
            Hash = serializerStruct.Hash;
            var dataSerializer = serializerStruct.DataSerializer;
            
            if (dataSerializer.Initialized == false)
                throw new Exception($"{dataSerializer} not initialized!");

            Serializer = dataSerializer;
            
            var data = dataSerializer.GetDataType();
            if (data == DataType.None)
                throw new Exception();
        }
    }

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

    public readonly unsafe struct MemberSerializer
    {
        public readonly FinalMemberSerializerStruct[] Members;

        private readonly DataTypesDatabase _dataTypesDatabase;

        private readonly byte[] _headerData;

        public MemberSerializer(MemberSerializerStruct[] members, DataTypesDatabase dataTypesDatabase)
        {
            var final = new FinalMemberSerializerStruct[members.Length];
            for (var i = 0; i < members.Length; i++)
                final[i] = new FinalMemberSerializerStruct(members[i]);

            Members = final;

            _dataTypesDatabase = dataTypesDatabase;
            _headerData = CreateHeaderData(Members);
        }

        private static byte[] CreateHeaderData(FinalMemberSerializerStruct[] datas)
        {
            var fieldsLength = datas.Length;

            var size = sizeof(byte) + (fieldsLength * 5);
            var headerData = new byte[size];

            int position = 0;
            headerData[position++] = (byte) fieldsLength;

            for (var index = 0; index < fieldsLength; index++)
            {
                var fieldData = datas[index];
                var hash = fieldData.Hash;
                headerData[position++] = (byte) hash;
                headerData[position++] = (byte) (hash >> 8);
                headerData[position++] = (byte) (hash >> 16);
                headerData[position++] = (byte) (hash >> 24);

                var dataType = fieldData.Serializer.GetDataType();

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
            var typeDataFields = Members;

            var fieldsLength = typeDataFields.Length;

            output.WriteBytes(_headerData, _headerData.Length);

            for (var index = 0; index < fieldsLength; index++)
            {
                var fieldData = typeDataFields[index];
                fieldData.Serializer.Write(objectAddress, output, context);
            }
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(byte* objectAddress, SerializerInput input, object context)
        {
            var fieldDatas = Members;

            var fieldCount = input.ReadByte();
 
            var streamData = input.GetNext(fieldCount * 5);
            
            var localData = new ReadOnlySpan<byte>(_headerData, 1, _headerData.Length - 1);
            
            if (streamData.SequenceEqual(localData))
            {
                for (var i = 0; i < fieldCount; i++)
                {
                    var fieldData = fieldDatas[i];
                    fieldData.Serializer.Read(objectAddress, input, context);
                }
            }
            else
            {
                int position = 0;

                var indexes = stackalloc byte[fieldCount];
                var dataTypes = stackalloc DataType[fieldCount];

                var fieldsLength = fieldDatas.Length;

                int searchStart = 0;
                for (var i = 0; i < fieldCount; i++)
                {
                    var field = streamData[position++] | streamData[position++] << 8 | streamData[position++] << 16 |
                                streamData[position++] << 24;
                    var type = (DataType) streamData[position++];

                    var deserialized = false;

                    for (var searchIndex = searchStart; searchIndex < fieldsLength; searchIndex++)
                    {
                        var fieldData = fieldDatas[searchIndex];

                        if (field == fieldData.Hash)
                        {
                            if (type == fieldData.Serializer.GetDataType())
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
                        //if (FieldsData.GetAlternate(fieldDatas, type, field, out var alternateIndex))
                        //{
                        //    indexes[i] = (byte)alternateIndex;
                        //}
                        //else
                        {
                            indexes[i] = 255;
                            dataTypes[i] = type;
                        }
                    }
                }

                for (var i = 0; i < fieldCount; i++)
                {
                    var index = indexes[i];

                    if (index == 255)
                    {
                        _dataTypesDatabase.SkipData(dataTypes[i], input);
                        continue;
                    }

                    var fieldData = fieldDatas[index];
                    fieldData.Serializer.Read(objectAddress, input, context);
                }
            }
        }
    }
}