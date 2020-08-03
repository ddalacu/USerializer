using System.Collections.Generic;
using UnityEngine;

namespace USerialization
{
    public partial class SerializerInput
    {
        private Node _current;

        public Node CurrentNode
        {
            get { return _current; }
            set { _current = value; }
        }

        public void MarkAsRead()
        {
            _current = null;
        }

        public void SkipNode()
        {
            MarkAsRead();
            Debug.Log($"Skipped value ");
        }

        public SerializerInput(string json)
        {
            _current = Parse(json);
        }

        public float ReadFloat()
        {
            if (_current.IsNumber())
            {
                var ret = _current.AsNumber.Float;
                MarkAsRead();
                return ret;
            }

            SkipNode();
            return 0;
        }

        public float[] ReadFloatArray()
        {
            if (_current.IsNull())
            {
                MarkAsRead();
                return null;
            }

            if (_current.IsArray())
            {
                var jsonNodeAsArray = _current.AsArray;
                var list = new float[jsonNodeAsArray.Count];

                var index = 0;
                foreach (var arrayElement in jsonNodeAsArray.ArrayElements)
                {
                    if (arrayElement.IsNumber())
                        list[index] = arrayElement.AsNumber.Float;
                    index++;
                }

                MarkAsRead();
                return list;
            }

            SkipNode();
            return null;
        }

        public bool ReadBool()
        {
            if (_current.IsBool())
            {
                var ret = _current.AsBool.Value;
                MarkAsRead();
                return ret;
            }
            SkipNode();
            return false;
        }

        public bool[] ReadBoolArray()
        {
            if (_current.IsNull())
            {
                MarkAsRead();
                return null;
            }

            if (_current.IsArray())
            {
                var jsonNodeAsArray = _current.AsArray;
                var list = new bool[jsonNodeAsArray.Count];

                var index = 0;
                foreach (var arrayElement in jsonNodeAsArray.ArrayElements)
                {
                    if (arrayElement.IsBool())
                        list[index] = arrayElement.AsBool.Value;
                    index++;
                }

                MarkAsRead();
                return list;
            }
            SkipNode();
            return null;
        }

        public int ReadInt()
        {
            if (_current.IsNumber())
            {
                var ret = _current.AsNumber.Int;
                MarkAsRead();
                return ret;
            }
            SkipNode();
            return 0;
        }

        public int[] ReadIntArray()
        {
            if (_current.IsNull())
            {
                MarkAsRead();
                return null;
            }

            if (_current.IsArray())
            {
                var jsonNodeAsArray = _current.AsArray;
                var list = new int[jsonNodeAsArray.Count];

                var index = 0;
                foreach (var arrayElement in jsonNodeAsArray.ArrayElements)
                {
                    if (arrayElement.IsNumber())
                        list[index] = arrayElement.AsNumber.Int;
                    index++;
                }
                MarkAsRead();
                return list;
            }

            SkipNode();
            return null;
        }

        public string ReadString()
        {
            if (_current.IsNull())
            {
                MarkAsRead();
                return null;
            }

            if (_current.IsString())
            {
                var ret = _current.AsString.Value;
                MarkAsRead();
                return ret;
            }

            SkipNode();

            return null;
        }

        public string[] ReadStringArray()
        {
            if (_current.IsNull())
            {
                MarkAsRead();
                return null;
            }

            if (_current.IsArray())
            {
                var jsonNodeAsArray = _current.AsArray;
                var list = new string[jsonNodeAsArray.Count];

                var index = 0;
                foreach (var arrayElement in jsonNodeAsArray.ArrayElements)
                {
                    if (arrayElement.IsString())
                        list[index] = arrayElement.AsString.Value;
                    index++;
                }
                MarkAsRead();
                return list;
            }

            SkipNode();
            return null;
        }

        public bool BeginReadArray(out int i, out ArrayElementsEnumerator next)
        {
            if (_current.IsArray())
            {
                var array = _current.AsArray;

                MarkAsRead();

                var elements = array.ArrayElements;
                i = elements.Count;

                next = new ArrayElementsEnumerator(elements, this);

                return true;
            }

            SkipNode();
            i = 0;
            next = default;
            return false;
        }

        public readonly ref struct ArrayElementsEnumerator
        {
            private readonly List<Node> _nodes;
            private readonly SerializerInput _serializerInput;

            public ArrayElementsEnumerator(List<Node> nodes, SerializerInput serializerInput)
            {
                _nodes = nodes;
                _serializerInput = serializerInput;
            }

            public bool Next(ref int index)
            {
                if (index == _nodes.Count)
                    return false;

                _serializerInput._current = _nodes[index];
                index++;
                return true;
            }
        }

        public void EndArray()
        {
            Debug.Assert(_current == null);
            MarkAsRead();
        }

        public bool BeginReadObject(out ObjectFieldsEnumerator fieldsEnumerator)
        {
            if (_current.IsNull())
            {
                MarkAsRead();
                fieldsEnumerator = default;
                return false;
            }

            if (_current.IsObject())
            {
                var objNode = _current.AsObject;

                MarkAsRead();

                fieldsEnumerator = new ObjectFieldsEnumerator(objNode.ObjectFields, this);
                return true;
            }

            SkipNode();

            fieldsEnumerator = default;
            return false;
        }

        public readonly ref struct ObjectFieldsEnumerator
        {
            private readonly List<KeyValuePair<string, Node>> _nodes;
            private readonly SerializerInput _serializerInput;

            public ObjectFieldsEnumerator(List<KeyValuePair<string, Node>> nodes, SerializerInput serializerInput)
            {
                _nodes = nodes;
                _serializerInput = serializerInput;
            }

            public bool Next(ref int index, out string fieldName)
            {
                if (index == _nodes.Count)
                {
                    fieldName = null;
                    return false;
                }

                var arrayNodeObjectField = _nodes[index];

                fieldName = arrayNodeObjectField.Key;
                _serializerInput._current = arrayNodeObjectField.Value;
                index++;
                return true;
            }
        }

        public void CloseObject()
        {
            MarkAsRead();
        }
    }
}