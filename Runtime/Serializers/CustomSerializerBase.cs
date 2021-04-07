using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

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

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public abstract class CustomSerializerBase<T> : ICustomSerializer
    {
        public Type SerializedType => _type;

        private List<FieldSerializerStruct> _fields = new List<FieldSerializerStruct>();

        public delegate void SetDelegate<TMember>(ref T obj, TMember value);

        public delegate TMember GetDelegate<TMember>(ref T obj);

        private bool _isReferenceType;

        private USerializer _serializer;

        private Type _type;

        protected USerializer Serializer => _serializer;

        protected CustomSerializerBase()
        {
            _type = typeof(T);
            _isReferenceType = _type.IsValueType == false;
        }

        public void Initialize(USerializer serializer)
        {
            _serializer = serializer;
            LocalInit();
        }

        protected unsafe void AddField<TMember>(int hash, SetDelegate<TMember> set, GetDelegate<TMember> get)
        {
            _serializer.TryGetSerializationMethods(typeof(TMember), out var methods);

            void Write(void* fieldAddress, SerializerOutput output)
            {
                ref var instance = ref Unsafe.AsRef<T>(fieldAddress);

                var value = get(ref instance);
                methods.Serialize(Unsafe.AsPointer(ref value), output);
            }

            void Read(void* fieldAddress, SerializerInput input)
            {
                ref var instance = ref Unsafe.AsRef<T>(fieldAddress);

                TMember def = default;
                var ptr = Unsafe.AsPointer(ref def);
                methods.Deserialize(ptr, input);
                set(ref instance, def);
            }

            var toAdd = new FieldSerializerStruct(hash, methods.DataType, Write, Read);

            var fieldsCount = _fields.Count;
            for (var i = 0; i < fieldsCount; i++)
            {
                if (hash == _fields[i].Hash)
                    throw new Exception("Hash already added!");

                if (hash < _fields[i].Hash)
                {
                    _fields.Insert(i, toAdd);
                    return;
                }
            }

            _fields.Add(toAdd);

            if (_fields.Count > 255)
                throw new Exception("Too many fields!");
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

            var fieldsCount = _fields.Count;
            for (var i = 0; i < fieldsCount; i++)
            {
                if (hash == _fields[i].Hash)
                    throw new Exception("Hash already added!");

                if (hash < _fields[i].Hash)
                {
                    _fields.Insert(i, toAdd);
                    return;
                }
            }

            _fields.Add(toAdd);

            if (_fields.Count > 255)
                throw new Exception("Too many fields!");
        }


        public delegate TElement GetElementDelegate<TElement>(ref T obj, int index);
        public delegate void SetElementDelegate<TElement>(ref T obj, int index, ref TElement element);

        public delegate int GetLengthDelegate(ref T obj);
        public delegate void SetLengthDelegate(ref T obj, int length);

        protected unsafe void AddArrayField<TElement>(int hash,
            GetLengthDelegate getLength, GetElementDelegate<TElement> getElement,
            SetLengthDelegate setLength, SetElementDelegate<TElement> setElement)
        {
            _serializer.TryGetSerializationMethods(typeof(TElement), out var methods);

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
                    output.WriteByteUnchecked((byte)methods.DataType);
                    output.Write7BitEncodedIntUnchecked(count);

                    for (var index = 0; index < count; index++)
                    {
                        var element = getElement(ref instance, index);
                        var itemAddress = Unsafe.AsPointer(ref element);
                        methods.Serialize(itemAddress, output);
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

                    if (type == methods.DataType)
                    {
                        for (var index = 0; index < count; index++)
                        {
                            TElement def = default;
                            var ptr = Unsafe.AsPointer(ref def);
                            methods.Deserialize(ptr, input);
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

            var fieldsCount = _fields.Count;
            for (var i = 0; i < fieldsCount; i++)
            {
                if (hash == _fields[i].Hash)
                    throw new Exception("Hash already added!");

                if (hash < _fields[i].Hash)
                {
                    _fields.Insert(i, toAdd);
                    return;
                }
            }

            _fields.Add(toAdd);

            if (_fields.Count > 255)
                throw new Exception("Too many fields!");
        }


        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            if (_isReferenceType &&
                Unsafe.Read<object>(fieldAddress) == null)
            {
                output.WriteNull();
                return;
            }

            var track = output.BeginSizeTrack();
            {
                output.WriteByte((byte)_fields.Count);

                for (var i = 0; i < _fields.Count; i++)
                {
                    output.EnsureNext(5);
                    output.WriteIntUnchecked(_fields[i].Hash);
                    output.WriteByteUnchecked((byte)_fields[i].DataType);
                    _fields[i].FieldWriteDelegate(fieldAddress, output);
                }
            }
            output.WriteSizeTrack(track);
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var instance = ref Unsafe.AsRef<T>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                if (_isReferenceType)
                {
                    ref var objectInstance = ref Unsafe.AsRef<object>(fieldAddress);
                    if (objectInstance == null)
                        objectInstance = Activator.CreateInstance(_type);
                }

                var fieldsCount = input.ReadByte();
                var fieldsLength = _fields.Count;

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
                instance = default;
            }
        }

        public abstract void LocalInit();

        public unsafe SerializationMethods GetMethods()
        {
            return new SerializationMethods(Write, Read, DataType.Object);
        }
    }
}

