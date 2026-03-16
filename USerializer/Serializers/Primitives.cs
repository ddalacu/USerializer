using System;
using System.Collections.Generic;
using USerialization;

[assembly: CustomSerializer(typeof(double), typeof(GenericUnmanagedSerializer<double, UnmanagedDataTypeLogic<double>>))]
[assembly: CustomSerializer(typeof(double[]), typeof(GenericUnmanagedArraySerializer<double, UnmanagedDataTypeLogic<double>>))]
[assembly: CustomSerializer(typeof(List<double>), typeof(GenericUnmanagedListSerializer<double, UnmanagedDataTypeLogic<double>>))]

[assembly: CustomSerializer(typeof(Int64), typeof(GenericUnmanagedSerializer<Int64, UnmanagedDataTypeLogic<Int64>>))]


[assembly: CustomSerializer(typeof(int), typeof(GenericUnmanagedSerializer<int, UnmanagedDataTypeLogic<int>>))]
[assembly: CustomSerializer(typeof(int[]), typeof(GenericUnmanagedArraySerializer<int, UnmanagedDataTypeLogic<int>>))]
[assembly: CustomSerializer(typeof(List<int>), typeof(GenericUnmanagedListSerializer<int, UnmanagedDataTypeLogic<int>>))]

[assembly: CustomSerializer(typeof(char), typeof(GenericUnmanagedSerializer<char, UnmanagedDataTypeLogic<char>>))]
[assembly: CustomSerializer(typeof(char[]), typeof(GenericUnmanagedArraySerializer<char, UnmanagedDataTypeLogic<char>>))]
[assembly: CustomSerializer(typeof(List<char>), typeof(GenericUnmanagedListSerializer<char, UnmanagedDataTypeLogic<char>>))]

[assembly: CustomSerializer(typeof(byte), typeof(GenericUnmanagedSerializer<byte, UnmanagedDataTypeLogic<byte>>))]
[assembly: CustomSerializer(typeof(byte[]), typeof(GenericUnmanagedArraySerializer<byte, UnmanagedDataTypeLogic<byte>>))]
[assembly: CustomSerializer(typeof(List<byte>), typeof(GenericUnmanagedListSerializer<byte, UnmanagedDataTypeLogic<byte>>))]

[assembly: CustomSerializer(typeof(bool), typeof(GenericUnmanagedSerializer<bool, UnmanagedDataTypeLogic<bool>>))]
[assembly: CustomSerializer(typeof(bool[]), typeof(GenericUnmanagedArraySerializer<bool, UnmanagedDataTypeLogic<bool>>))]
[assembly: CustomSerializer(typeof(List<bool>), typeof(GenericUnmanagedListSerializer<bool, UnmanagedDataTypeLogic<bool>>))]

[assembly: CustomSerializer(typeof(Int16), typeof(GenericUnmanagedSerializer<Int16, UnmanagedDataTypeLogic<Int16>>))]
[assembly: CustomSerializer(typeof(float), typeof(GenericUnmanagedSerializer<float, UnmanagedDataTypeLogic<float>>))]

[assembly: CustomSerializer(typeof(UInt16), typeof(GenericUnmanagedSerializer<UInt16, UnmanagedDataTypeLogic<UInt16>>))]

[assembly: CustomSerializer(typeof(sbyte), typeof(GenericUnmanagedSerializer<sbyte, UnmanagedDataTypeLogic<sbyte>>))]

[assembly: CustomSerializer(typeof(UInt64), typeof(GenericUnmanagedSerializer<UInt64, UnmanagedDataTypeLogic<UInt64>>))]

[assembly: CustomSerializer(typeof(uint), typeof(GenericUnmanagedSerializer<uint, UnmanagedDataTypeLogic<UInt32>>))]