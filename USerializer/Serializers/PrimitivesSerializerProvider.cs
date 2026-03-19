using System;
using System.Collections.Generic;

namespace USerialization
{
    public class PrimitivesSerializerProvider : ISerializationProvider
    {
        private readonly TypeDictionary<CustomDataSerializer> _map;
        
        public PrimitivesSerializerProvider()
        {
            _map = new TypeDictionary<CustomDataSerializer>(64);
            
            _map.Add(typeof(double), new GenericUnmanagedSerializer<double>(DataType.Double));
            _map.Add(typeof(double[]), new GenericUnmanagedArraySerializer<double>());
            _map.Add(typeof(List<double>), new GenericUnmanagedListSerializer<double>());
            
            _map.Add(typeof(float), new GenericUnmanagedSerializer<float>(DataType.Float));
            _map.Add(typeof(float[]), new GenericUnmanagedArraySerializer<float>());
            _map.Add(typeof(List<float>), new GenericUnmanagedListSerializer<float>());

            _map.Add(typeof(int), new GenericUnmanagedSerializer<int>(DataType.Int32));
            _map.Add(typeof(int[]), new GenericUnmanagedArraySerializer<int>());
            _map.Add(typeof(List<int>), new GenericUnmanagedListSerializer<int>());

            _map.Add(typeof(uint), new GenericUnmanagedSerializer<uint>(DataType.UInt32));
            _map.Add(typeof(uint[]), new GenericUnmanagedArraySerializer<uint>());
            _map.Add(typeof(List<uint>), new GenericUnmanagedListSerializer<uint>());

            _map.Add(typeof(long), new GenericUnmanagedSerializer<long>(DataType.Int64));
            _map.Add(typeof(long[]), new GenericUnmanagedArraySerializer<long>());
            _map.Add(typeof(List<long>), new GenericUnmanagedListSerializer<long>());

            _map.Add(typeof(ulong), new GenericUnmanagedSerializer<ulong>(DataType.UInt64));
            _map.Add(typeof(ulong[]), new GenericUnmanagedArraySerializer<ulong>());
            _map.Add(typeof(List<ulong>), new GenericUnmanagedListSerializer<ulong>());

            _map.Add(typeof(short), new GenericUnmanagedSerializer<short>(DataType.Int16));
            _map.Add(typeof(short[]), new GenericUnmanagedArraySerializer<short>());
            _map.Add(typeof(List<short>), new GenericUnmanagedListSerializer<short>());

            _map.Add(typeof(ushort), new GenericUnmanagedSerializer<ushort>(DataType.UInt16));
            _map.Add(typeof(ushort[]), new GenericUnmanagedArraySerializer<ushort>());
            _map.Add(typeof(List<ushort>), new GenericUnmanagedListSerializer<ushort>());

            _map.Add(typeof(byte), new GenericUnmanagedSerializer<byte>(DataType.Byte));
            _map.Add(typeof(byte[]), new GenericUnmanagedArraySerializer<byte>());
            _map.Add(typeof(List<byte>), new GenericUnmanagedListSerializer<byte>());

            _map.Add(typeof(sbyte), new GenericUnmanagedSerializer<sbyte>(DataType.SByte));
            _map.Add(typeof(sbyte[]), new GenericUnmanagedArraySerializer<sbyte>());
            _map.Add(typeof(List<sbyte>), new GenericUnmanagedListSerializer<sbyte>());

            _map.Add(typeof(bool), new GenericUnmanagedSerializer<bool>(DataType.Bool));
            _map.Add(typeof(bool[]), new GenericUnmanagedArraySerializer<bool>());
            _map.Add(typeof(List<bool>), new GenericUnmanagedListSerializer<bool>());

            _map.Add(typeof(char), new GenericUnmanagedSerializer<char>(DataType.Char));
            _map.Add(typeof(char[]), new GenericUnmanagedArraySerializer<char>());
            _map.Add(typeof(List<char>), new GenericUnmanagedListSerializer<char>());
            
            _map.Add(typeof(string), new StringSerializer());
        }
        
        public bool TryGet(USerializer serializer, Type type, out DataSerializer dataSerializer)
        {
            if (!_map.TryGetValue(type, out var ser))
            {
                dataSerializer = default;
                return false;
            }

            if (ser.TryInitialize(serializer))
            {
                dataSerializer = ser;
                return true;
            }

            dataSerializer = default;
            return false;
        }
    }
}