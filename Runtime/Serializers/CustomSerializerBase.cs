using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace USerialization
{

    public unsafe delegate void FieldWriteDelegate(void* fieldAddress, SerializerOutput output);

    public unsafe delegate void FieldReadDelegate(void* fieldAddress, SerializerInput output);

    public readonly struct FieldSerializerStruct
    {
        public readonly int Hash;
        public readonly DataType DataType;
        public readonly FieldWriteDelegate FieldWriteDelegate;
        public readonly FieldReadDelegate FieldReadDelegate;

        public FieldSerializerStruct(int hash, DataType dataType, FieldWriteDelegate fieldWriteDelegate,
            FieldReadDelegate fieldReadDelegate)
        {
            Hash = hash;
            DataType = dataType;
            FieldWriteDelegate = fieldWriteDelegate;
            FieldReadDelegate = fieldReadDelegate;
        }
    }

    public abstract class CustomSerializerBase<T> : ICustomSerializer
    {
        public Type SerializedType => _type;

        public DataType DataType => DataType.Object;

        private List<FieldSerializerStruct> _fields = new List<FieldSerializerStruct>();

        public delegate void SetDelegate<TMember>(ref T obj, TMember value);

        public delegate TMember GetDelegate<TMember>(ref T obj);

        private bool _isReferenceType;

        private USerializer _serializer;

        private Type _type;

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
    }
}

