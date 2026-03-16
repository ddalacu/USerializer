using System;
using System.Collections.Generic;

namespace USerialization
{
    public interface IDataSkipper
    {
        void Skip(SerializerInput input);
    }

    public class DataTypesDatabase
    {
        private Dictionary<DataType, IDataSkipper> _dataEntries;
        
        public DataTypesDatabase()
        {
            _dataEntries = new Dictionary<DataType, IDataSkipper>(32);
            _dataEntries.Add(DataType.Byte, new UnmanagedDataSkipper<byte>());
            _dataEntries.Add(DataType.SByte, new UnmanagedDataSkipper<sbyte>());
            _dataEntries.Add(DataType.Char, new UnmanagedDataSkipper<char>());
            _dataEntries.Add(DataType.Bool, new UnmanagedDataSkipper<bool>());
            _dataEntries.Add(DataType.Int16, new UnmanagedDataSkipper<Int16>());
            _dataEntries.Add(DataType.Int32, new UnmanagedDataSkipper<int>());
            _dataEntries.Add(DataType.Int64, new UnmanagedDataSkipper<Int64>());
            _dataEntries.Add(DataType.UInt16, new UnmanagedDataSkipper<UInt16>());
            _dataEntries.Add(DataType.UInt32, new UnmanagedDataSkipper<UInt32>());
            _dataEntries.Add(DataType.UInt64, new UnmanagedDataSkipper<UInt64>());
            _dataEntries.Add(DataType.Float, new UnmanagedDataSkipper<float>());
            _dataEntries.Add(DataType.Double, new UnmanagedDataSkipper<double>());
            
            _dataEntries.Add(DataType.String, new StringDataSkipper());
            _dataEntries.Add(DataType.Array, new ArrayDataSkipper());
            _dataEntries.Add(DataType.Object, new ObjectDataSkipper());
        }
        
        public bool TryGet<T>(out T result) where T : IDataSkipper, new()
        {
            foreach (var dataEntriesKey in _dataEntries.Values)
            {
                if (dataEntriesKey is T casted)
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
            return _dataEntries.TryGetValue(type, out dataSkipper);
        }

        public void SkipData(DataType type, SerializerInput serializerInput)
        {
            if (GetForType(type, out var dataEntry))
            {
                dataEntry.Skip(serializerInput);
            }
            else
            {
                throw new Exception($"Unknown data type {type}");
            }
        }
    }
}