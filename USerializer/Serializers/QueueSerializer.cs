using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace USerialization
{
    public sealed unsafe class QueueSerializer : CustomDataSerializer
    {
        private readonly Type _fieldType;
        private readonly Type _elementType;
        private readonly DataSerializer _elementSerializer;

        private int _elementSize;

        private FieldAccessHelper<object, Array> _itemsField;
        private FieldAccessHelper<object, int> _headField;
        private FieldAccessHelper<object, int> _tailField;
        private FieldAccessHelper<object, int> _sizeField;

        private DataType _elementDataType;

        public override DataType DataType => DataType.Array;

        public QueueSerializer(Type fieldType, Type elementType, DataSerializer elementSerializer)
        {
            _fieldType = fieldType;
            _elementType = elementType;
            _elementSerializer = elementSerializer;
        }

        protected override void Initialize(USerializer serializer)
        {
            base.Initialize(serializer);
            _elementSerializer.RootInitialize(serializer);
            _elementDataType = _elementSerializer.DataType;

            if (_elementDataType == DataType.None)
            {
                serializer.Logger.Error("Element data type is none, something went wrong!");
            }

            _elementSize = serializer.RuntimeUtils.GetStackSize(_elementType);

            var itemsMember = _fieldType.GetField("_array", BindingFlags.Instance | BindingFlags.NonPublic);
            var headMember = _fieldType.GetField("_head", BindingFlags.Instance | BindingFlags.NonPublic);
            var tailMember = _fieldType.GetField("_tail", BindingFlags.Instance | BindingFlags.NonPublic);
            var sizeMember = _fieldType.GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic);

            if (itemsMember == null || headMember == null || tailMember == null || sizeMember == null)
                throw new InvalidOperationException("Could not find Queue internal fields.");

            _itemsField = new FieldAccessHelper<object, Array>(itemsMember, serializer.RuntimeUtils);
            _headField = new FieldAccessHelper<object, int>(headMember, serializer.RuntimeUtils);
            _tailField = new FieldAccessHelper<object, int>(tailMember, serializer.RuntimeUtils);
            _sizeField = new FieldAccessHelper<object, int>(sizeMember, serializer.RuntimeUtils);
        }

        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output)
        {
            ref var instance = ref Unsafe.As<byte, object>(ref MemoryMarshal.GetReference(span));
            if (instance == null)
            {
                output.WriteNull();
                return;
            }

            ref var array = ref _itemsField.GetFieldRef(ref instance);
            var head = _headField.GetFieldRef(ref instance);
            var size = _sizeField.GetFieldRef(ref instance);

            if (size > 0)
            {
                var sizeTracker = output.BeginSizeTrack();
                output.EnsureNext(6);
                output.Write7BitEncodedIntUnchecked(size);
                output.WriteByteUnchecked((byte)_elementDataType);

                var pinnable = Unsafe.As<Array, byte[]>(ref array);
                fixed (byte* address = pinnable)
                {
                    var serializer = _elementSerializer;
                    var arrayLength = array.Length;
                    for (int i = 0; i < size; i++)
                    {
                        var index = (head + i) % arrayLength;
                        serializer.Write(new ReadOnlySpan<byte>(address + (index * _elementSize), _elementSize),
                            ref output);
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
            ref var instance = ref Unsafe.As<byte, object>(ref MemoryMarshal.GetReference(span));
            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();

                if (instance == null)
                {
                    instance = RuntimeHelpers.GetUninitializedObject(_fieldType);
                    ref var array = ref _itemsField.GetFieldRef(ref instance);
                    array = Array.CreateInstance(_elementType, count);

                    _headField.GetFieldRef(ref instance) = 0;
                    _tailField.GetFieldRef(ref instance) = count;
                    _sizeField.GetFieldRef(ref instance) = count;
                }
                else
                {
                    ref var array = ref _itemsField.GetFieldRef(ref instance);
                    ref var currentHead = ref _headField.GetFieldRef(ref instance);
                    ref var currentSize = ref _sizeField.GetFieldRef(ref instance);

                    if (array.Length < count)
                    {
                        array = Array.CreateInstance(_elementType, count);
                        currentHead = 0;
                        _tailField.GetFieldRef(ref instance) = (count == array.Length) ? 0 : count;
                        currentSize = count;
                    }
                    else
                    {
                        // Clean existing items if any
                        if (currentSize > 0)
                        {
                            var arrayLength = array.Length;
                            for (int i = 0; i < currentSize; i++)
                            {
                                var index = (currentHead + i) % arrayLength;
                                ArrayHelpers.Clear(array, index, 1, _elementSize);
                            }
                        }

                        currentHead = 0;
                        _tailField.GetFieldRef(ref instance) = (count == array.Length) ? 0 : count;
                        currentSize = count;
                    }
                }

                ref var finalArray = ref _itemsField.GetFieldRef(ref instance);
                if (count > 0)
                {
                    var type = (DataType)input.ReadByte();
                    if (type == _elementDataType)
                    {
                        var pinnable = Unsafe.As<Array, byte[]>(ref finalArray);
                        fixed (byte* address = pinnable)
                        {
                            var serializer = _elementSerializer;
                            // We set head=0 during Read
                            for (int i = 0; i < count; i++)
                            {
                                serializer.Read(new Span<byte>(address + (i * _elementSize), _elementSize), ref input);
                            }
                        }
                    }
                    else
                    {
                        ArrayHelpers.Clear(finalArray, 0, count, _elementSize);
                        input.EndObject(end);
                    }
                }
            }
            else
            {
                instance = null;
            }
        }
    }

    public class QueueSerializerProvider : ISerializationProvider
    {
        public bool TryGet(USerializer serializer, Type type, out DataSerializer dataSerializer)
        {
            dataSerializer = default;

            if (!type.IsGenericType)
                return false;

            if (type.GetGenericTypeDefinition() != typeof(Queue<>))
                return false;

            var constructionArguments = type.GetGenericArguments();
            var elementType = constructionArguments[0];

            if (serializer.TryGetDataSerializer(elementType, out var elementDataSerializer, false) == false)
            {
                dataSerializer = default;
                return false;
            }

            dataSerializer = new QueueSerializer(type, elementType, elementDataSerializer);
            return true;
        }
    }
}