using System;
using System.Collections.Generic;

namespace USerialization
{
    public interface IDataTypeLogic
    {
        DataType Value { get; set; }
        void Skip(SerializerInput input);
    }

    public class DataTypesDatabase
    {
        private Dictionary<DataType, IDataTypeLogic> _dataEntries;

        private byte _counter;

        public DataTypesDatabase()
        {
            _counter = 0;
            _dataEntries = new Dictionary<DataType, IDataTypeLogic>(32);
            Register<UnmanagedDataTypeLogic<byte>>();
            Register<UnmanagedDataTypeLogic<sbyte>>();
            Register<UnmanagedDataTypeLogic<char>>();
            Register<UnmanagedDataTypeLogic<bool>>();
            Register<UnmanagedDataTypeLogic<Int16>>();
            Register<UnmanagedDataTypeLogic<int>>();
            Register<UnmanagedDataTypeLogic<Int64>>();
            Register<UnmanagedDataTypeLogic<UInt16>>();
            Register<UnmanagedDataTypeLogic<UInt32>>();
            Register<UnmanagedDataTypeLogic<UInt64>>();
            Register<UnmanagedDataTypeLogic<float>>();
            Register<UnmanagedDataTypeLogic<double>>();
            Register<StringDataTypeLogic>();
            Register<ArrayDataTypeLogic>();
            Register<ObjectDataTypeLogic>();
        }

        public void Register<T>() where T : IDataTypeLogic, new()
        {
            var instance = new T();
            if (_counter == 255)
                throw new Exception("To many data types!");

            _counter++;
            var dataType = (DataType)_counter;
            instance.Value = dataType;
            _dataEntries.Add(dataType, instance);
        }

        public bool TryGet<T>(out T result) where T : IDataTypeLogic, new()
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

        public bool GetForType(DataType type, out IDataTypeLogic dataTypeLogic)
        {
            return _dataEntries.TryGetValue(type, out dataTypeLogic);
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