using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace USerialization
{
    public readonly struct FieldSerializerStruct
    {
        public readonly int Hash;
        public readonly DataType DataType;
        public readonly WriteDelegate FieldWriteDelegate;
        public readonly ReadDelegate FieldReadDelegate;

        public FieldSerializerStruct(int hash, DataType dataType, WriteDelegate fieldWriteDelegate,
            ReadDelegate fieldReadDelegate)
        {
            Hash = hash;
            DataType = dataType;
            FieldWriteDelegate = fieldWriteDelegate;
            FieldReadDelegate = fieldReadDelegate;
        }
    }

    //we use classes and not local functions so we can mark methods with attributes
    public sealed unsafe class ReadFieldMethod<T, TMember>
    {
        private readonly ReadDelegate _readDelegate;
        private readonly SetDelegate<T, TMember> _set;

        public ReadFieldMethod([NotNull] ReadDelegate readDelegate, [NotNull] SetDelegate<T, TMember> set)
        {
            _readDelegate = readDelegate ?? throw new ArgumentNullException(nameof(readDelegate));
            _set = set ?? throw new ArgumentNullException(nameof(set));
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public void ReadField(void* fieldAddress, SerializerInput input)
        {
            ref var instance = ref Unsafe.AsRef<T>(fieldAddress);

            TMember def = default;
            var ptr = Unsafe.AsPointer(ref def);
            _readDelegate(ptr, input);
            _set(ref instance, def);
        }
    }

    //we use classes and not local functions so we can mark methods with attributes
    public sealed unsafe class WriteFieldMethod<T, TMember>
    {
        private readonly WriteDelegate _writeDelegate;
        private readonly GetDelegate<T, TMember> _get;

        public WriteFieldMethod([NotNull] WriteDelegate writeDelegate, [NotNull] GetDelegate<T, TMember> get)
        {
            _writeDelegate = writeDelegate ?? throw new ArgumentNullException(nameof(writeDelegate));
            _get = get ?? throw new ArgumentNullException(nameof(get));
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        public void WriteField(void* fieldAddress, SerializerOutput output)
        {
            ref var instance = ref Unsafe.AsRef<T>(fieldAddress);
            var value = _get(ref instance);
            _writeDelegate(Unsafe.AsPointer(ref value), output);
        }
    }

    public delegate void SetDelegate<T, in TMember>(ref T obj, TMember value);

    public delegate TMember GetDelegate<T, out TMember>(ref T obj);

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public abstract class CustomSerializerBase<T> : DataSerializer, ICustomSerializer
    {
        public Type SerializedType => _type;

        private FieldSerializerStruct[] _fields;

        private int _fieldCount;

        private USerializer _serializer;

        private Type _type;

        protected USerializer Serializer => _serializer;

        protected CustomSerializerBase() : base(DataType.Object)
        {
            _type = typeof(T);
            _fields = new FieldSerializerStruct[6];
            var callSerializationEvents = typeof(ISerializationCallbackReceiver).IsAssignableFrom(_type);
            if (callSerializationEvents)
                Debug.LogError("This is not supported!");
        }

        public void Initialize(USerializer serializer)
        {
            _serializer = serializer;
            LocalInit();
            ArrayUtils.CropToSize(ref _fields, ref _fieldCount);
            if (_fieldCount > 255)
                throw new Exception("To many fields!");
        }

        protected unsafe void AddField<TMember>(int hash, SetDelegate<T, TMember> set, GetDelegate<T, TMember> get)
        {
            var memberType = typeof(TMember);
            if (_serializer.TryGetSerializationMethods(memberType, out var methods) == false)
            {
                Debug.LogError($"{typeof(T)} custom serializer, cannot get serialization methods for field of type {memberType}");
                return;
            }

            var writer = new WriteFieldMethod<T, TMember>(methods.WriteDelegate, get);
            var reader = new ReadFieldMethod<T, TMember>(methods.ReadDelegate, set);
            var toAdd = new FieldSerializerStruct(hash, methods.DataType, writer.WriteField, reader.ReadField);

            for (var i = 0; i < _fieldCount; i++)
            {
                if (hash == _fields[i].Hash)
                    throw new Exception("Hash already added!");

                if (hash < _fields[i].Hash)
                {
                    ArrayUtils.Insert(ref _fields, ref _fieldCount, i, toAdd);
                    return;
                }
            }

            ArrayUtils.Add(ref _fields, ref _fieldCount, toAdd);
        }

        public delegate void CustomWriteDelegate(ref T obj, SerializerOutput output);
        public delegate void CustomReadDelegate(ref T obj, SerializerInput input);


        protected unsafe void AddField(int hash, DataType type, CustomWriteDelegate writeField, CustomReadDelegate readField)
        {
            void Write(void* fieldAddress, SerializerOutput output)
            {
                ref var instance = ref Unsafe.AsRef<T>(fieldAddress);
                writeField(ref instance, output);
            }

            void Read(void* fieldAddress, SerializerInput input)
            {
                ref var instance = ref Unsafe.AsRef<T>(fieldAddress);
                readField(ref instance, input);
            }

            var toAdd = new FieldSerializerStruct(hash, type, Write, Read);

            for (var i = 0; i < _fieldCount; i++)
            {
                if (hash == _fields[i].Hash)
                    throw new Exception("Hash already added!");

                if (hash < _fields[i].Hash)
                {
                    ArrayUtils.Insert(ref _fields, ref _fieldCount, i, toAdd);
                    return;
                }
            }

            ArrayUtils.Add(ref _fields, ref _fieldCount, toAdd);
        }


        public delegate TElement GetElementDelegate<TElement>(ref T obj, int index);
        public delegate void SetElementDelegate<TElement>(ref T obj, int index, ref TElement element);

        public delegate int GetLengthDelegate(ref T obj);
        public delegate void SetLengthDelegate(ref T obj, int length);

        protected unsafe void AddArrayField<TElement>(int hash,
            GetLengthDelegate getLength, GetElementDelegate<TElement> getElement,
            SetLengthDelegate setLength, SetElementDelegate<TElement> setElement)
        {
            var memberType = typeof(TElement);
            if (_serializer.TryGetSerializationMethods(memberType, out var elementSerializer) == false)
            {
                Debug.LogError($"{typeof(T)} custom serializer, cannot get serialization methods for element of type {memberType}");
                return;
            }

            void Write(void* fieldAddress, SerializerOutput output)
            {
                ref var instance = ref Unsafe.AsRef<T>(fieldAddress);

                var count = getLength(ref instance);

                if (count == -1)
                {
                    output.WriteNull();
                    return;
                }

                var sizeTracker = output.BeginSizeTrack();
                {
                    output.EnsureNext(6);
                    output.WriteByteUnchecked((byte)elementSerializer.DataType);
                    output.Write7BitEncodedIntUnchecked(count);

                    for (var index = 0; index < count; index++)
                    {
                        var element = getElement(ref instance, index);
                        var itemAddress = Unsafe.AsPointer(ref element);
                        elementSerializer.WriteDelegate(itemAddress, output);
                    }
                }
                output.WriteSizeTrack(sizeTracker);
            }

            void Read(void* fieldAddress, SerializerInput input)
            {
                ref var instance = ref Unsafe.AsRef<T>(fieldAddress);

                if (input.BeginReadSize(out var end))
                {
                    var type = (DataType)input.ReadByte();

                    var count = input.Read7BitEncodedInt();

                    setLength(ref instance, count);

                    if (type == elementSerializer.DataType)
                    {
                        for (var index = 0; index < count; index++)
                        {
                            TElement def = default;
                            var ptr = Unsafe.AsPointer(ref def);
                            elementSerializer.ReadDelegate(ptr, input);
                            setElement(ref instance, index, ref def);
                        }
                    }

                    input.EndObject(end);
                }
                else
                {
                    setLength(ref instance, -1);
                }
            }

            var toAdd = new FieldSerializerStruct(hash, DataType.Array, Write, Read);

            for (var i = 0; i < _fieldCount; i++)
            {
                if (hash == _fields[i].Hash)
                    throw new Exception("Hash already added!");

                if (hash < _fields[i].Hash)
                {
                    ArrayUtils.Insert(ref _fields, ref _fieldCount, i, toAdd);
                    return;
                }
            }

            ArrayUtils.Add(ref _fields, ref _fieldCount, toAdd);
        }


        public unsafe void WriteReferenceType(void* fieldAddress, SerializerOutput output)
        {
            if (Unsafe.Read<object>(fieldAddress) == null)
            {
                output.WriteNull();
                return;
            }

            var track = output.BeginSizeTrack();
            {
                var size = _fieldCount;
                output.WriteByte((byte)size);

                for (var i = 0; i < size; i++)
                {
                    output.EnsureNext(5);
                    var field = _fields[i];
                    output.WriteIntUnchecked(field.Hash);
                    output.WriteByteUnchecked((byte)field.DataType);
                    field.FieldWriteDelegate(fieldAddress, output);
                }
            }
            output.WriteSizeTrack(track);
        }

        public unsafe void WriteValueType(void* fieldAddress, SerializerOutput output)
        {
            var track = output.BeginSizeTrack();
            {
                var size = _fieldCount;

                output.WriteByte((byte)size);

                for (var i = 0; i < size; i++)
                {
                    output.EnsureNext(5);
                    var field = _fields[i];
                    output.WriteIntUnchecked(field.Hash);
                    output.WriteByteUnchecked((byte)field.DataType);
                    field.FieldWriteDelegate(fieldAddress, output);
                }
            }
            output.WriteSizeTrack(track);
        }

        public unsafe void ReadReferenceType(void* fieldAddress, SerializerInput input)
        {
            if (input.BeginReadSize(out var end))
            {
                ref var objectInstance = ref Unsafe.AsRef<object>(fieldAddress);
                if (objectInstance == null)
                    objectInstance = Activator.CreateInstance(_type);

                var fieldsCount = input.ReadByte();

                var fieldsLength = _fieldCount;

                int searchStart = 0;

                for (var i = 0; i < fieldsCount; i++)
                {
                    var field = input.ReadInt();
                    var type = (DataType)input.ReadByte();

                    var deserialized = false;

                    for (var searchIndex = searchStart; searchIndex < fieldsLength; searchIndex++)
                    {
                        var fieldData = _fields[searchIndex];

                        if (field == fieldData.Hash)
                        {
                            if (type == fieldData.DataType)
                            {
                                fieldData.FieldReadDelegate(fieldAddress, input);
                                deserialized = true;
                            }

                            searchStart = searchIndex + 1;
                            break;
                        }
                    }

                    if (deserialized == false)
                    {
                        input.SkipData(type);
                    }
                }


                input.EndObject(end);
            }
            else
            {
                ref var objectInstance = ref Unsafe.AsRef<object>(fieldAddress);
                objectInstance = null;
            }
        }

        public unsafe void ReadValueType(void* fieldAddress, SerializerInput input)
        {
            if (input.BeginReadSize(out var end))
            {
                var fieldsCount = input.ReadByte();
                var fieldsLength = _fieldCount;

                int searchStart = 0;

                for (var i = 0; i < fieldsCount; i++)
                {
                    var field = input.ReadInt();
                    var type = (DataType)input.ReadByte();

                    var deserialized = false;

                    for (var searchIndex = searchStart; searchIndex < fieldsLength; searchIndex++)
                    {
                        var fieldData = _fields[searchIndex];

                        if (field == fieldData.Hash)
                        {
                            if (type == fieldData.DataType)
                            {
                                fieldData.FieldReadDelegate(fieldAddress, input);
                                deserialized = true;
                            }

                            searchStart = searchIndex + 1;
                            break;
                        }
                    }

                    if (deserialized == false)
                    {
                        input.SkipData(type);
                    }
                }


                input.EndObject(end);
            }
            else
            {
                ref var instance = ref Unsafe.AsRef<T>(fieldAddress);
                instance = default;
            }
        }

        public abstract void LocalInit();

        public DataSerializer GetMethods()
        {
            return this;
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var isReferenceType = _type.IsValueType == false;

            if (isReferenceType)
            {
                WriteReferenceType(fieldAddress, output);
            }
            else
            {
                WriteValueType(fieldAddress, output);
            }
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            var isReferenceType = _type.IsValueType == false;

            if (isReferenceType)
            {
                ReadReferenceType(fieldAddress, input);
            }
            else
            {
                ReadValueType(fieldAddress, input);
            }
        }
    }

}

