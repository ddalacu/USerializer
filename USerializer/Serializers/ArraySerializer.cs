using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
namespace USerialization
{
    public class ArraySerializer : ISerializationProvider
    {
        public bool TryGet(USerializer serializer, Type type, out DataSerializer serializationMethods)
        {
            serializationMethods = default;

            if (type.IsArray == false)
            {
                serializationMethods = default;
                return false;
            }

            if (type.GetArrayRank() > 1)
            {
                serializationMethods = default;
                return false;
            }
            
            var elementType = type.GetElementType();

            if (serializer.TryGetDataSerializer(elementType, out var elementSerializer, false) == false)
            {
                serializationMethods = default;
                return false;
            }

            serializationMethods = new ArrayDataSerializer(elementType, elementSerializer);
            return true;
        }
    }
    
    public sealed unsafe class ArrayDataSerializer : DataSerializer
    {
        private readonly Type _elementType;

        private readonly DataSerializer _elementSerializer;

        private int _size;
        
        public override DataType DataType => DataType.Array;

        private DataType _elementDataType;

        protected override void Initialize(USerializer serializer)
        {
            _elementSerializer.RootInitialize(serializer);

            _elementDataType = _elementSerializer.DataType;

            if (_elementDataType == DataType.None)
            {
                serializer.Logger.Error("Element data type is none, something went wrong!");
            }

            _size = serializer.RuntimeUtils.GetStackSize(_elementType);
        }

        public ArrayDataSerializer(Type elementType, DataSerializer elementSerializer)
        {
            _elementType = elementType;
            _elementSerializer = elementSerializer;
        }

        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output, object context)
        {
            Debug.Assert(span.Length == IntPtr.Size);
            
            ref var array = ref Unsafe.As<byte, Array>(ref MemoryMarshal.GetReference(span));

            if (array == null)
            {
                output.WriteNull();
                return;
            }

            var count = array.Length;

            if (count > 0)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    output.EnsureNext(6);
                    output.Write7BitEncodedIntUnchecked(count);
                    output.WriteByteUnchecked((byte)_elementDataType);

                    var pinnable = Unsafe.As<Array, byte[]>(ref array);
                    var serializer = _elementSerializer;
                    fixed (byte* address = pinnable)
                    {
                        var tempAddress = address;

                        for (var index = 0; index < count; index++)
                        {
                            serializer.Write(new Span<byte>(tempAddress, _size), ref output, context);
                            tempAddress += _size;
                        }
                    }
                }
                output.WriteSizeTrack(sizeTracker);
            }
            else
            {
                output.EnsureNext(5);
                output.WriteUnchecked<int>(1); //size tracker
                output.WriteByteUnchecked(0);
            }
        }


        public override void Read(Span<byte> span, ref SerializerInput input, object context)
        {
            Debug.Assert(span.Length == IntPtr.Size);
            
            ref var array = ref Unsafe.As<byte, Array>(ref MemoryMarshal.GetReference(span));

            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();

                if (array == null || array.Length != count)
                {
                    array = Array.CreateInstance(_elementType, count);
                }

                if (count > 0)
                {
                    var type = (DataType)input.ReadByte();

                    if (type == _elementDataType)
                    {
                        var pinnable = Unsafe.As<Array, byte[]>(ref array);
                        var serializer = _elementSerializer;
                        fixed (byte* address = pinnable)
                        {
                            var tempAddress = address;

                            for (var i = 0; i < count; i++)
                            {
                                serializer.Read(new Span<byte>(tempAddress, _size), ref input, context);
                                tempAddress += _size;
                            }
                        }
                    }
                    else
                    {
                        ArrayHelpers.CleanArray(array, 0, (uint)count, (uint)_size);
                        input.EndObject(end);
                    }
                }
            }
            else
            {
                array = null;
            }
        }
    }
}