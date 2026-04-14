using System;
using System.Collections.Generic;

namespace USerialization
{
    public interface IDataSkipper
    {
        void Skip(ref SerializerInput input);
    }

    public class DataTypesDatabase
    {
        private IDataSkipper[] _dataEntries;

        public DataTypesDatabase()
        {
            _dataEntries = new IDataSkipper[]
            {
                null, // DataType.None
                new ObjectDataSkipper(), // DataType.Object
                new ArrayDataSkipper(), // DataType.Array
                new StringDataSkipper(), // DataType.String
                new UnmanagedDataSkipper<byte>(), // DataType.Byte
                new UnmanagedDataSkipper<sbyte>(), // DataType.SByte
                new UnmanagedDataSkipper<char>(), // DataType.Char
                new UnmanagedDataSkipper<bool>(), // DataType.Bool
                new UnmanagedDataSkipper<Int16>(), // DataType.Int16
                new UnmanagedDataSkipper<int>(), // DataType.Int32
                new UnmanagedDataSkipper<Int64>(), // DataType.Int64
                new UnmanagedDataSkipper<UInt16>(), // DataType.UInt16
                new UnmanagedDataSkipper<UInt32>(), // DataType.UInt32
                new UnmanagedDataSkipper<UInt64>(), // DataType.UInt64
                new UnmanagedDataSkipper<float>(), // DataType.Float
                new UnmanagedDataSkipper<double>(), // DataType.Double
            };
        }

        public bool TryGet<T>(out T result) where T : IDataSkipper, new()
        {
            foreach (var dataEntry in _dataEntries)
            {
                if (dataEntry is T casted)
                {
                    result = casted;
                    return true;
                }
            }

            result = default;
            return false;
        }

        public bool GetForType(DataType type, out IDataSkipper dataSkipper)
        {
            var index = (int)type;
            if (index >= 0 && index < _dataEntries.Length)
            {
                dataSkipper = _dataEntries[index];
                return dataSkipper != null;
            }

            dataSkipper = null;
            return false;
        }

        public void SkipData(DataType type, ref SerializerInput serializerInput)
        {
            if (GetForType(type, out var dataEntry))
            {
                dataEntry.Skip(ref serializerInput);
            }
            else
            {
                throw new Exception($"Unknown data type {type}");
            }
        }
    }
}