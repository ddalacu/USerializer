using System;
using System.Collections.Generic;

namespace USerialization
{
    public interface IDataTypeLogic
    {
        DataType Value { get; }

        void Skip(SerializerInput input);
    }

    public class DataTypeLogic
    {
        private static Dictionary<DataType, IDataTypeLogic> _dataEntries = new Dictionary<DataType, IDataTypeLogic>();

        static DataTypeLogic()
        {
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

        public static void Register<T>() where T : IDataTypeLogic, new()
        {
            var instance = new T();
            _dataEntries.Add(instance.Value, instance);
        }

        public static bool GetForType(DataType type, out IDataTypeLogic dataTypeLogic)
        {
            return _dataEntries.TryGetValue(type, out dataTypeLogic);
        }

        public static void SkipData(DataType type, SerializerInput serializerInput)
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