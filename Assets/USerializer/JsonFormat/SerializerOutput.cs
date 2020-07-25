using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace USerialization
{
    //public unsafe class SerializerOutput
    //{
    //    private StringBuilder _stringWriter;

    //    public enum Token
    //    {
    //        Object,
    //        Array,
    //        Field
    //    }

    //    private Stack<Token> _tokens = new Stack<Token>(12);

    //    public SerializerOutput(int capacity)
    //    {
    //        _stringWriter = new StringBuilder(capacity);
    //    }

    //    public void WriteInt(int value)
    //    {
    //        _stringWriter.Append(value);
    //        CheckArraySeparator();
    //    }

    //    public void WriteIntArray(int[] value, int length)
    //    {
    //        OpenArray();

    //        for (int i = 0; i < length; i++)
    //        {
    //            WriteInt(value[i]);
    //        }

    //        CloseArray();
    //    }

    //    public void WriteFloat(float value)
    //    {
    //        _stringWriter.Append(value);
    //        CheckArraySeparator();
    //    }

    //    public void WriteFloatArray(float[] value, int length)
    //    {
    //        OpenArray();
    //        for (int i = 0; i < length; i++)
    //            WriteFloat(value[i]);
    //        CloseArray();
    //    }

    //    public void WriteBool(bool value)
    //    {
    //        if (value)
    //            _stringWriter.Append("true");
    //        else
    //            _stringWriter.Append("false");
    //        CheckArraySeparator();
    //    }

    //    public void BeginReadField(string name)
    //    {
    //        //#if UNITY_EDITOR
    //        Debug.Assert(_tokens.Peek() == Token.Object);
    //        _tokens.Push(Token.Field);
    //        //#endif

    //        _stringWriter.Append('\"');
    //        _stringWriter.Append(name);
    //        _stringWriter.Append('\"');
    //        _stringWriter.Append(':');
    //    }

    //    public void CloseField()
    //    {
    //        //#if UNITY_EDITOR
    //        var pop = _tokens.Pop();
    //        Debug.Assert(pop == Token.Field);
    //        //#endif

    //        _stringWriter.Append(',');
    //    }

    //    public void Null()
    //    {
    //        _stringWriter.Append("null");

    //        CheckArraySeparator();
    //    }

    //    public void BeginReadObject()
    //    {
    //        //#if UNITY_EDITOR
    //        _tokens.Push(Token.Object);
    //        //#endif
    //        _stringWriter.Append('{');
    //    }

    //    public void CloseObject()
    //    {
    //        //#if UNITY_EDITOR
    //        var pop = _tokens.Pop();
    //        Debug.Assert(pop == Token.Object);
    //        //#endif

    //        CheckTrailingSeparator();

    //        _stringWriter.Append('}');

    //        CheckArraySeparator();
    //    }

    //    private void CheckTrailingSeparator()
    //    {
    //        if (_stringWriter[_stringWriter.Length - 1] == ',')
    //            _stringWriter.Length--;
    //    }

    //    private void CheckArraySeparator()
    //    {
    //        if (_tokens.Count > 0 && _tokens.Peek() == Token.Array)
    //        {
    //            _stringWriter.Append(',');
    //        }
    //    }

    //    public void OpenArray()
    //    {
    //        _tokens.Push(Token.Array);
    //        _stringWriter.Append('[');
    //    }

    //    public void CloseArray()
    //    {
    //        CheckTrailingSeparator();
    //        _stringWriter.Append(']');

    //        //#if UNITY_EDITOR
    //        var pop = _tokens.Pop();
    //        Debug.Assert(pop == Token.Array);
    //        //#endif

    //        CheckArraySeparator();
    //    }

    //    public string GetData()
    //    {
    //        return _stringWriter.ToString();
    //    }

    //    public void WriteString(string value)
    //    {
    //        if (value != null)
    //        {
    //            _stringWriter.Append('\"');
    //            _stringWriter.Append(value);
    //            _stringWriter.Append('\"');
    //        }
    //        else
    //        {
    //            Null();
    //        }

    //        CheckArraySeparator();
    //    }

    //    public void WriteBoolArray(bool[] value, int length)
    //    {
    //        OpenArray();
    //        for (int i = 0; i < length; i++)
    //            WriteBool(value[i]);
    //        CloseArray();
    //    }

    //    public void Clear()
    //    {
    //        _stringWriter.Length = 0;
    //    }

    //    public void WriteStringArray(string[] array, int arrayLength)
    //    {
    //        OpenArray();
    //        for (int i = 0; i < arrayLength; i++)
    //            WriteString(array[i]);
    //        CloseArray();
    //    }

    //    public void WriteNull()
    //    {
    //        Null();
    //    }
    //}

    public unsafe class SerializerOutput
    {
        private StringBuilder _stringWriter;

        public enum Token
        {
            Object,
            Array,
            Field
        }

        private Stack<Token> _tokens = new Stack<Token>(12);

        public SerializerOutput(int capacity)
        {
            _stringWriter = new StringBuilder(capacity);
        }

        public void WriteInt(int value)
        {
            _stringWriter.Append(value);
            CheckArraySeparator();
        }

        public void WriteIntArray(int[] value, int length)
        {
            OpenArray();

            for (int i = 0; i < length; i++)
            {
                WriteInt(value[i]);
            }

            CloseArray();
        }

        public void WriteFloat(float value)
        {
            _stringWriter.Append(value);
            CheckArraySeparator();
        }

        public void WriteFloatArray(float[] value, int length)
        {
            OpenArray();
            for (int i = 0; i < length; i++)
                WriteFloat(value[i]);
            CloseArray();
        }

        public void WriteBool(bool value)
        {
            if (value)
                _stringWriter.Append("true");
            else
                _stringWriter.Append("false");
            CheckArraySeparator();
        }

        public void OpenField(string name)
        {
            //#if UNITY_EDITOR
            Debug.Assert(_tokens.Peek() == Token.Object);
            _tokens.Push(Token.Field);
            //#endif

            _stringWriter.Append('\"');
            _stringWriter.Append(name);
            _stringWriter.Append('\"');
            _stringWriter.Append(':');
        }

        public void CloseField()
        {
            //#if UNITY_EDITOR
            var pop = _tokens.Pop();
            Debug.Assert(pop == Token.Field);
            //#endif

            _stringWriter.Append(',');
        }

        public void Null()
        {
            _stringWriter.Append("null");

            CheckArraySeparator();
        }

        public void OpenObject()
        {
            //#if UNITY_EDITOR
            _tokens.Push(Token.Object);
            //#endif
            _stringWriter.Append('{');
        }

        public void CloseObject()
        {
            //#if UNITY_EDITOR
            var pop = _tokens.Pop();
            Debug.Assert(pop == Token.Object);
            //#endif

            CheckTrailingSeparator();

            _stringWriter.Append('}');

            CheckArraySeparator();
        }

        private void CheckTrailingSeparator()
        {
            if (_stringWriter[_stringWriter.Length - 1] == ',')
                _stringWriter.Length--;
        }

        private void CheckArraySeparator()
        {
            if (_tokens.Count > 0 && _tokens.Peek() == Token.Array)
            {
                _stringWriter.Append(',');
            }
        }

        public void OpenArray()
        {
            _tokens.Push(Token.Array);
            _stringWriter.Append('[');
        }

        public void CloseArray()
        {
            CheckTrailingSeparator();
            _stringWriter.Append(']');

            //#if UNITY_EDITOR
            var pop = _tokens.Pop();
            Debug.Assert(pop == Token.Array);
            //#endif

            CheckArraySeparator();
        }

        public string GetData()
        {
            return _stringWriter.ToString();
        }

        public void WriteString(string value)
        {
            if (value != null)
            {
                _stringWriter.Append('\"');
                _stringWriter.Append(value);
                _stringWriter.Append('\"');
            }
            else
            {
                Null();
            }

            CheckArraySeparator();
        }

        public void WriteBoolArray(bool[] value, int length)
        {
            OpenArray();
            for (int i = 0; i < length; i++)
                WriteBool(value[i]);
            CloseArray();
        }

        public void Clear()
        {
            _stringWriter.Length = 0;
        }

        public void WriteStringArray(string[] array, int arrayLength)
        {
            OpenArray();
            for (int i = 0; i < arrayLength; i++)
                WriteString(array[i]);
            CloseArray();
        }

        public void WriteNull()
        {
            Null();
        }
    }

}