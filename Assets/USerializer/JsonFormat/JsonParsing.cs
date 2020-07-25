using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace USerialization
{
    public partial class SerializerInput
    {
        public enum NodeType : byte
        {
            Object,
            Array,
            Number,
            String,
            Boolean,
            Null
        }

        public class Node
        {
            public readonly NodeType Type;

            public Node(NodeType nodeType)
            {
                Type = nodeType;
            }

            public NumberNode AsNumber
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Unsafe.As<NumberNode>(this);
            }

            public StringNode AsString
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Unsafe.As<StringNode>(this);
            }

            public ObjectNode AsObject
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Unsafe.As<ObjectNode>(this);
            }

            public NullNode AsNull
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Unsafe.As<NullNode>(this);
            }

            public ArrayNode AsArray
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Unsafe.As<ArrayNode>(this);
            }

            public BoolNode AsBool
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Unsafe.As<BoolNode>(this);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsString()
            {
                return Type == NodeType.String;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsObject()
            {
                return Type == NodeType.Object;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsNumber()
            {
                return Type == NodeType.Number;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsArray()
            {
                return Type == NodeType.Array;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsBool()
            {
                return Type == NodeType.Boolean;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsNull()
            {
                return Type == NodeType.Null;
            }
        }

        public sealed class StringNode : Node
        {
            public string Value;

            public StringNode(string value) : base(NodeType.String)
            {
                Value = value;
            }
        }

        public sealed class NumberNode : Node
        {
            public string Value;

            public NumberNode(string value) : base(NodeType.Number)
            {
                Value = value;
            }

            public float Float
            {
                get
                {
                    float.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result);
                    return result;
                }
            }

            public int Int
            {
                get
                {
                    int.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result);
                    return result;
                }
            }
        }

        public sealed class ObjectNode : Node
        {
            public List<KeyValuePair<string, Node>> ObjectFields = new List<KeyValuePair<string, Node>>();

            public void Add(string tokenName, Node parseElement)
            {
                ObjectFields.Add(new KeyValuePair<string, Node>(tokenName, parseElement));
            }

            public ObjectNode() : base(NodeType.Object)
            {
            }
        }

        public sealed class ArrayNode : Node
        {
            public List<Node> ArrayElements = new List<Node>();

            public int Count => ArrayElements.Count;

            public void Add(Node parseElement)
            {
                ArrayElements.Add(parseElement);
            }

            public ArrayNode() : base(NodeType.Array)
            {
            }
        }

        public sealed class BoolNode : Node
        {
            public bool Value;

            public BoolNode(bool value) : base(NodeType.Boolean)
            {
                Value = value;
            }
        }

        public sealed class NullNode : Node
        {
            public NullNode() : base(NodeType.Null)
            {
            }
        }

        public static Node Parse(string json)
        {
            var stack = new Stack<Node>();
            Node ctx = null;
            int i = 0;
            var Token = new StringBuilder();
            string TokenName = null;
            bool QuoteMode = false;
            bool TokenIsQuoted = false;
            while (i < json.Length)
            {
                switch (json[i])
                {
                    case '{':
                        if (QuoteMode)
                        {
                            Token.Append(json[i]);
                            break;
                        }

                        var objectNode = new ObjectNode();

                        stack.Push(objectNode);
                        if (ctx != null)
                        {
                            if (ctx is ArrayNode cArray)
                                cArray.Add(objectNode);
                            else if (ctx is ObjectNode cNode)
                                cNode.Add(TokenName, objectNode);
                        }

                        TokenName = null;
                        Token.Length = 0;
                        ctx = stack.Peek();
                        break;

                    case '[':
                        if (QuoteMode)
                        {
                            Token.Append(json[i]);
                            break;
                        }

                        var arrayNode = new ArrayNode();
                        stack.Push(arrayNode);
                        if (ctx != null)
                        {
                            if (ctx is ArrayNode cArray)
                                cArray.Add(arrayNode);
                            if (ctx is ObjectNode cNode)
                                cNode.Add(TokenName, arrayNode);
                        }

                        TokenName = null;
                        Token.Length = 0;
                        ctx = stack.Peek();
                        break;

                    case '}':
                    case ']':
                        if (QuoteMode)
                        {
                            Token.Append(json[i]);
                            break;
                        }

                        if (stack.Count == 0)
                            throw new Exception("JSON Parse: Too many closing brackets");

                        stack.Pop();
                        if (Token.Length > 0 || TokenIsQuoted)
                        {
                            var parsed = ParseElement(Token.ToString(), TokenIsQuoted);

                            if (ctx is ArrayNode cArray)
                                cArray.Add(parsed);
                            else if (ctx is ObjectNode cNode)
                                cNode.Add(TokenName, parsed);
                        }

                        TokenIsQuoted = false;
                        TokenName = null;
                        Token.Length = 0;
                        if (stack.Count > 0)
                            ctx = stack.Peek();
                        break;

                    case ':':
                        if (QuoteMode)
                        {
                            Token.Append(json[i]);
                            break;
                        }

                        TokenName = Token.ToString();
                        Token.Length = 0;
                        TokenIsQuoted = false;
                        break;

                    case '"':
                        QuoteMode ^= true;
                        TokenIsQuoted |= QuoteMode;
                        break;

                    case ',':
                        if (QuoteMode)
                        {
                            Token.Append(json[i]);
                            break;
                        }

                        if (Token.Length > 0 || TokenIsQuoted)
                        {
                            var parsed = ParseElement(Token.ToString(), TokenIsQuoted);

                            if (ctx is ArrayNode cArray)
                                cArray.Add(parsed);
                            else if (ctx is ObjectNode cNode)
                                cNode.Add(TokenName, parsed);
                        }

                        TokenIsQuoted = false;
                        TokenName = null;
                        Token.Length = 0;
                        TokenIsQuoted = false;
                        break;

                    case '\r':
                    case '\n':
                        break;

                    case ' ':
                    case '\t':
                        if (QuoteMode)
                            Token.Append(json[i]);
                        break;

                    case '\\':
                        ++i;
                        if (QuoteMode)
                        {
                            char C = json[i];
                            switch (C)
                            {
                                case 't':
                                    Token.Append('\t');
                                    break;
                                case 'r':
                                    Token.Append('\r');
                                    break;
                                case 'n':
                                    Token.Append('\n');
                                    break;
                                case 'b':
                                    Token.Append('\b');
                                    break;
                                case 'f':
                                    Token.Append('\f');
                                    break;
                                case 'u':
                                    {
                                        string s = json.Substring(i + 1, 4);
                                        Token.Append((char)int.Parse(
                                            s,
                                            System.Globalization.NumberStyles.AllowHexSpecifier));
                                        i += 4;
                                        break;
                                    }
                                default:
                                    Token.Append(C);
                                    break;
                            }
                        }

                        break;
                    case '/':
                        Token.Append(json[i]);
                        break;
                    case '\uFEFF': // remove / ignore BOM (Byte Order Mark)
                        break;

                    default:
                        Token.Append(json[i]);
                        break;
                }

                ++i;
            }

            if (QuoteMode)
            {
                throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
            }

            if (ctx == null)
            {
                return ParseElement(Token.ToString(), TokenIsQuoted);
            }

            return ctx;
        }

        private static Node ParseElement(string token, bool quoted)
        {
            if (quoted)
                return new StringNode(token);
            string tmp = token.ToLower();
            if (tmp == "false" || tmp == "true")
                return new BoolNode(tmp == "true");
            if (tmp == "null")
                return new NullNode();
            return new NumberNode(token);
        }

    }
}
