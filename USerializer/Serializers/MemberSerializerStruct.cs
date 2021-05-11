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

    public readonly unsafe struct MemberSerializer
    {
        public readonly MemberSerializerStruct[] Members;

        private readonly DataTypesDatabase _dataTypesDatabase;

        private readonly byte[] _headerData;

        public MemberSerializer(MemberSerializerStruct[] members, DataTypesDatabase dataTypesDatabase)
        {
            Members = members;
            _dataTypesDatabase = dataTypesDatabase;
            _headerData = CreateHeaderData(Members);
        }

        private static byte[] CreateHeaderData(MemberSerializerStruct[] datas)
        {
            var fieldsLength = datas.Length;

            var size = sizeof(byte) + (fieldsLength * 5);
            var headerData = new byte[size];

            int position = 0;
            headerData[position++] = (byte)fieldsLength;

            for (var index = 0; index < fieldsLength; index++)
            {
                var fieldData = datas[index];
                var hash = fieldData.Hash;
                headerData[position++] = (byte)hash;
                headerData[position++] = (byte)(hash >> 8);
                headerData[position++] = (byte)(hash >> 16);
                headerData[position++] = (byte)(hash >> 24);
                var dataSerializer = fieldData.DataSerializer;
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
            var typeDataFields = Members;

            var fieldsLength = typeDataFields.Length;

            output.WriteBytes(_headerData, _headerData.Length);

            for (var index = 0; index < fieldsLength; index++)
            {
                var fieldData = typeDataFields[index];
                var dataSerializer = fieldData.DataSerializer;
                dataSerializer.Write(objectAddress, output);
            }
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read(byte* objectAddress, SerializerInput input)
        {
            var fieldDatas = Members;

            var fieldCount = input.ReadByte();
            var size = fieldCount * 5;

            input.Skip(size);
            //we just skipped the required data so we have it in the buffer
            var buffer = input.Buffer;
            var offset = input.PositionInBuffer - size;

            if (SameHeader(buffer, offset, size))
            {
                for (int i = 0; i < fieldCount; i++)
                {
                    var fieldData = fieldDatas[i];
                    var dataSerializer = fieldData.DataSerializer;
                    dataSerializer.Read(objectAddress, input);
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
                    var field = buffer[position++] | buffer[position++] << 8 | buffer[position++] << 16 |
                                buffer[position++] << 24;
                    var type = (DataType)buffer[position++];

                    var deserialized = false;

                    for (var searchIndex = searchStart; searchIndex < fieldsLength; searchIndex++)
                    {
                        var fieldData = fieldDatas[searchIndex];

                        if (field == fieldData.Hash)
                        {
                            var dataSerializer = fieldData.DataSerializer;

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
                    var dataSerializer = fieldData.DataSerializer;
                    dataSerializer.Read(objectAddress, input);
                }
            }
        }
    }

}