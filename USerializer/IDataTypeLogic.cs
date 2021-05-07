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
            Register<ByteDataTypeLogic>();
            Register<SByteDataTypeLogic>();
            Register<CharDataTypeLogic>();
            Register<BooleanDataTypeLogic>();
            Register<Int16DataTypeLogic>();
            Register<Int32DataTypeLogic>();
            Register<Int64DataTypeLogic>();
            Register<UInt16DataTypeLogic>();
            Register<UInt32DataTypeLogic>();
            Register<UInt64DataTypeLogic>();
            Register<SingleDataTypeLogic>();
            Register<DoubleDataTypeLogic>();
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