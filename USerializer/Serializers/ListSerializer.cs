using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace USerialization
{
    public class ListSerializer : ISerializationProvider
    {
        public bool TryGet(USerializer serializer, Type type, out DataSerializer serializationMethods)
        {
            serializationMethods = default;

            if (type.IsConstructedGenericType == false)
                return false;

            if (type.GetGenericTypeDefinition() != typeof(List<>))
                return false;

            var elementType = type.GetGenericArguments()[0];

            if (serializer.TryGetDataSerializer(elementType, out var elementDataSerializer, false) == false)
            {
                serializationMethods = default;
                return false;
            }

            serializationMethods =
                new ListDataSerializer(type, elementType, elementDataSerializer);
            return true;
        }
    }

    public sealed unsafe class ListDataSerializer : DataSerializer
    {
        private readonly Type _elementType;

        private readonly DataSerializer _elementSerializer;

        private int _size;

        private int _itemsOffset;
        private int _sizeOffset;

        private FieldAccessHelper<IList, Array> _itemsField;
        private FieldAccessHelper<IList, int> _sizeField;


        private readonly Type _fieldType;

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

            var itemsMember = _fieldType.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
            var sizeMember = _fieldType.GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic);

            if (itemsMember == null || sizeMember == null)
                throw new InvalidOperationException("Could not find List internal fields.");

            _itemsField = new FieldAccessHelper<IList, Array>(itemsMember, serializer.RuntimeUtils);
            _sizeField = new FieldAccessHelper<IList, int>(sizeMember, serializer.RuntimeUtils);
        }

        public ListDataSerializer(Type fieldType, Type elementType, DataSerializer elementSerializer)
        {
            _fieldType = fieldType;
            _elementType = elementType;
            _elementSerializer = elementSerializer;
        }

        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output)
        {
            Debug.Assert(span.Length == IntPtr.Size);

            ref var list = ref Unsafe.As<byte, IList>(ref MemoryMarshal.GetReference(span));

            if (list == null)
            {
                output.WriteNull();
                return;
            }
            
            ref var count = ref _sizeField.GetFieldRef(ref list);

            if (count > 0)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    output.EnsureNext(6);
                    output.Write7BitEncodedIntUnchecked(count);
                    output.WriteByteUnchecked((byte)_elementDataType);
                    
                    ref var array = ref _itemsField.GetFieldRef(ref list);
                    var pinnable = Unsafe.As<Array, byte[]>(ref array);

                    fixed (byte* address = pinnable)
                    {
                        var tempAddress = address;
                        var serializer = _elementSerializer;
                        for (var index = 0; index < count; index++)
                        {
                            serializer.Write(new ReadOnlySpan<byte>(tempAddress, _size), ref output);
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

        public override void Read(Span<byte> span, ref SerializerInput input)
        {
            Debug.Assert(span.Length == IntPtr.Size);

            ref var list = ref Unsafe.As<byte, IList>(ref MemoryMarshal.GetReference(span));

            if (input.BeginReadSize(out var end) == false)
            {
                list = null;
                return;
            }

            var count = input.Read7BitEncodedInt();

            Array array;

            if (list == null)
            {
                list = (IList)RuntimeHelpers.GetUninitializedObject(_fieldType);
                array = Array.CreateInstance(_elementType, count);
                _itemsField.GetFieldRef(ref list) = array;
            }
            else
            {
                array = _itemsField.GetFieldRef(ref list);

                if (array.Length < count) //if we need more elements in the array then we allocate a array
                {
                    array = Array.CreateInstance(_elementType, count);
                    _itemsField.GetFieldRef(ref list) = array;
                }
                else
                {
                    var diff = count - array.Length;
                    if (diff > 0)
                        ArrayHelpers.Clear(array, count, diff, _size);
                }
            }

            _sizeField.GetFieldRef(ref list) = count;

            if (count == 0)
                return;

            var type = (DataType)input.ReadByte();
            if (type == _elementDataType)
            {
                var pinnable = Unsafe.As<Array, byte[]>(ref array);
                fixed (byte* address = pinnable)
                {
                    var tempAddress = address;
                    var serializer = _elementSerializer;
                    for (var i = 0; i < count; i++)
                    {
                        serializer.Read(new Span<byte>(tempAddress, _size), ref input);
                        tempAddress += _size;
                    }
                }
            }
            else
            {
                ArrayHelpers.Clear(array, 0, count, _size);
                input.EndObject(end);
            }
        }
    }
}