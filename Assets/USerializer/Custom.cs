using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using USerialization;


public abstract class ClassWriterBase<T> : ICustomSerializer where T : class
{
    public Type SerializedType => typeof(T);

    public DataType DataType => DataType.Object;

    public delegate void WriteDelegate(T obj, SerializerOutput output);

    public delegate void ReadDelegate(T obj, SerializerInput output);

    public readonly struct FieldSerializerStruct
    {
        public readonly int Hash;
        public readonly DataType DataType;
        public readonly WriteDelegate WriteDelegate;
        public readonly ReadDelegate ReadDelegate;


        public FieldSerializerStruct(int hash, DataType dataType, WriteDelegate writeDelegate, ReadDelegate readDelegate)
        {
            Hash = hash;
            DataType = dataType;
            WriteDelegate = writeDelegate;
            ReadDelegate = readDelegate;
        }
    }

    private List<FieldSerializerStruct> _fields = new List<FieldSerializerStruct>();

    public void AddField<TMember>(int hash, Action<T, TMember> set, Func<T, TMember> get) where TMember : class
    {
        _serializer.TryGetSerializationMethods(typeof(TMember), out var methods);

        unsafe void Write(T obj, SerializerOutput output)
        {
            var value = get(obj);
            methods.Serialize(Unsafe.AsPointer(ref value), output);
        }

        unsafe void Read(T obj, SerializerInput input)
        {
            var ptr = new IntPtr();
            var address = UnsafeUtility.AddressOf(ref ptr);
            methods.Deserialize(address, input);

            ref var instance = ref Unsafe.AsRef<TMember>(address);
            set(obj, instance);
        }

        _fields.Add(new FieldSerializerStruct(hash, methods.DataType, Write, Read));
    }

    public unsafe void Write(void* fieldAddress, SerializerOutput output)
    {
        var obj = Unsafe.Read<T>(fieldAddress);
        if (obj == null)
        {
            output.WriteNull();
            return;
        }

        var track = output.BeginSizeTrack();
        {
            output.WriteByte((byte)_fields.Count);

            for (var i = 0; i < _fields.Count; i++)
            {
                output.WriteInt(_fields[i].Hash);
                output.WriteByteUnchecked((byte)_fields[i].DataType);
                _fields[i].WriteDelegate(obj, output);
            }
        }
        output.WriteSizeTrack(track);
    }

    public unsafe void Read(void* fieldAddress, SerializerInput input)
    {
        ref var instance = ref Unsafe.AsRef<T>(fieldAddress);

        if (input.BeginReadSize(out var end))
        {
            if (instance == null)
                instance = (T)Activator.CreateInstance(typeof(T));

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
                            fieldData.ReadDelegate(instance, input);
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
            instance = null;
        }
    }

    private USerializer _serializer;

    public void Initialize(USerializer serializer)
    {
        _serializer = serializer;
        LocalInit();
    }

    public abstract void LocalInit();
}

