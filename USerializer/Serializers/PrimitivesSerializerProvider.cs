using System;
using System.Collections.Generic;

namespace USerialization
{
    public class PrimitivesSerializerProvider : ISerializationProvider
    {
        private readonly Dictionary<Type, CustomDataSerializer> _map;
        
        public PrimitivesSerializerProvider()
        {
            _map = new Dictionary<Type, CustomDataSerializer>(64);
            
            _map.Add(typeof(double), new GenericUnmanagedSerializer<double, UnmanagedDataSkipper<double>>(DataType.Double));
            _map.Add(typeof(double[]), new GenericUnmanagedArraySerializer<double, UnmanagedDataSkipper<double>>());
            _map.Add(typeof(List<double>), new GenericUnmanagedListSerializer<double, UnmanagedDataSkipper<double>>());
            
            _map.Add(typeof(float), new GenericUnmanagedSerializer<float, UnmanagedDataSkipper<float>>(DataType.Float));
            _map.Add(typeof(float[]), new GenericUnmanagedArraySerializer<float, UnmanagedDataSkipper<float>>());
            _map.Add(typeof(List<float>), new GenericUnmanagedListSerializer<float, UnmanagedDataSkipper<float>>());

            _map.Add(typeof(int), new GenericUnmanagedSerializer<int, UnmanagedDataSkipper<int>>(DataType.Int32));
            _map.Add(typeof(int[]), new GenericUnmanagedArraySerializer<int, UnmanagedDataSkipper<int>>());
            _map.Add(typeof(List<int>), new GenericUnmanagedListSerializer<int, UnmanagedDataSkipper<int>>());

            _map.Add(typeof(uint), new GenericUnmanagedSerializer<uint, UnmanagedDataSkipper<uint>>(DataType.UInt32));
            _map.Add(typeof(uint[]), new GenericUnmanagedArraySerializer<uint, UnmanagedDataSkipper<uint>>());
            _map.Add(typeof(List<uint>), new GenericUnmanagedListSerializer<uint, UnmanagedDataSkipper<uint>>());

            _map.Add(typeof(long), new GenericUnmanagedSerializer<long, UnmanagedDataSkipper<long>>(DataType.Int64));
            _map.Add(typeof(long[]), new GenericUnmanagedArraySerializer<long, UnmanagedDataSkipper<long>>());
            _map.Add(typeof(List<long>), new GenericUnmanagedListSerializer<long, UnmanagedDataSkipper<long>>());

            _map.Add(typeof(ulong), new GenericUnmanagedSerializer<ulong, UnmanagedDataSkipper<ulong>>(DataType.UInt64));
            _map.Add(typeof(ulong[]), new GenericUnmanagedArraySerializer<ulong, UnmanagedDataSkipper<ulong>>());
            _map.Add(typeof(List<ulong>), new GenericUnmanagedListSerializer<ulong, UnmanagedDataSkipper<ulong>>());

            _map.Add(typeof(short), new GenericUnmanagedSerializer<short, UnmanagedDataSkipper<short>>(DataType.Int16));
            _map.Add(typeof(short[]), new GenericUnmanagedArraySerializer<short, UnmanagedDataSkipper<short>>());
            _map.Add(typeof(List<short>), new GenericUnmanagedListSerializer<short, UnmanagedDataSkipper<short>>());

            _map.Add(typeof(ushort), new GenericUnmanagedSerializer<ushort, UnmanagedDataSkipper<ushort>>(DataType.UInt16));
            _map.Add(typeof(ushort[]), new GenericUnmanagedArraySerializer<ushort, UnmanagedDataSkipper<ushort>>());
            _map.Add(typeof(List<ushort>), new GenericUnmanagedListSerializer<ushort, UnmanagedDataSkipper<ushort>>());

            _map.Add(typeof(byte), new GenericUnmanagedSerializer<byte, UnmanagedDataSkipper<byte>>(DataType.Byte));
            _map.Add(typeof(byte[]), new GenericUnmanagedArraySerializer<byte, UnmanagedDataSkipper<byte>>());
            _map.Add(typeof(List<byte>), new GenericUnmanagedListSerializer<byte, UnmanagedDataSkipper<byte>>());

            _map.Add(typeof(sbyte), new GenericUnmanagedSerializer<sbyte, UnmanagedDataSkipper<sbyte>>(DataType.SByte));
            _map.Add(typeof(sbyte[]), new GenericUnmanagedArraySerializer<sbyte, UnmanagedDataSkipper<sbyte>>());
            _map.Add(typeof(List<sbyte>), new GenericUnmanagedListSerializer<sbyte, UnmanagedDataSkipper<sbyte>>());

            _map.Add(typeof(bool), new GenericUnmanagedSerializer<bool, UnmanagedDataSkipper<bool>>(DataType.Bool));
            _map.Add(typeof(bool[]), new GenericUnmanagedArraySerializer<bool, UnmanagedDataSkipper<bool>>());
            _map.Add(typeof(List<bool>), new GenericUnmanagedListSerializer<bool, UnmanagedDataSkipper<bool>>());

            _map.Add(typeof(char), new GenericUnmanagedSerializer<char, UnmanagedDataSkipper<char>>(DataType.Char));
            _map.Add(typeof(char[]), new GenericUnmanagedArraySerializer<char, UnmanagedDataSkipper<char>>());
            _map.Add(typeof(List<char>), new GenericUnmanagedListSerializer<char, UnmanagedDataSkipper<char>>());
            
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