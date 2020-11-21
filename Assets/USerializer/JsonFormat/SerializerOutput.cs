using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace USerialization
{

    public readonly struct ValueArrayBlock : IDisposable
    {
        private readonly SerializerOutput _output;

        public ValueArrayBlock(SerializerOutput output, int count, int elementSize)
        {
            _output = output;

            _output.Write('[');
        }

        public void Dispose()
        {
            _output.Write(']');
        }

        public void WriteSeparator()
        {
            _output.Write(',');
        }
    }

    public readonly struct ReferenceArrayBlock : IDisposable
    {
        private readonly SerializerOutput _output;

        public ReferenceArrayBlock(SerializerOutput output, int count)
        {
            _output = output;

            _output.Write('[');
        }

        public void Dispose()
        {
            _output.Write(']');
        }

        public void WriteSeparator()
        {
            _output.Write(',');
        }
    }

    public static class Shared
    {
        public static unsafe void WriteObject(SerializerOutput output, FieldData[] fields, byte* address)
        {
            var length = fields.Length;

            output.Write('{');

            for (var index = 0; index < length; index++)
            {
                var fieldData = fields[index];

                output.Write('\"');
                output.Write(fieldData.FieldInfo.Name);
                output.Write('\"');
                output.Write(':');

                fieldData.SerializationMethods.Serialize(address + fieldData.Offset, output);

                if (index < length - 1)
                    output.Write(',');
            }

            output.Write('}');
        }

    }

    public class SerializerOutput
    {
        private StringBuilder _stringWriter;

        public SerializerOutput(int capacity)
        {
            _stringWriter = new StringBuilder(capacity);
        }

        public void Null()
        {
            _stringWriter.Append("null");
        }

        public string GetData()
        {
            return _stringWriter.ToString();
        }

        public void WriteString(string value)
        {
            _stringWriter.Append('\"');
            _stringWriter.Append(value);
            _stringWriter.Append('\"');
        }

        public void Write(string value)
        {
            _stringWriter.Append(value);
        }

        public void Write(char value)
        {
            _stringWriter.Append(value);
        }

        public void Clear()
        {
            _stringWriter.Length = 0;
        }
    }

}