using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace USerialization
{

    public class SerializerOutput
    {
        private StringBuilder _stringWriter;

        public SerializerOutput(int capacity)
        {
            _stringWriter = new StringBuilder(capacity);
        }

        public void OpenField(string name)
        {
            _stringWriter.Append('\"');
            _stringWriter.Append(name);
            _stringWriter.Append('\"');
            _stringWriter.Append(':');
        }

        public void CloseField()
        {
            _stringWriter.Append(',');
        }

        public void Null()
        {
            _stringWriter.Append("null");
        }

        public void OpenObject()
        {
            _stringWriter.Append('{');
        }

        public void CloseObject()
        {
            CheckTrailingSeparator();

            _stringWriter.Append('}');
        }

        private void CheckTrailingSeparator()
        {
            if (_stringWriter[_stringWriter.Length - 1] == ',')
                _stringWriter.Length--;
        }

        public void OpenArray()
        {
            _stringWriter.Append('[');
        }

        public void CloseArray()
        {
            CheckTrailingSeparator();
            _stringWriter.Append(']');
        }

        public string GetData()
        {
            return _stringWriter.ToString();
        }

        public void WriteArraySeparator()
        {
            _stringWriter.Append(',');
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

        public void WriteNull()
        {
            Null();
        }
    }

}