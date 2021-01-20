using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using USerialization;

[assembly: CustomSerializer(typeof(StringSerializer))]
[assembly: CustomSerializer(typeof(StringArraySerializer))]
[assembly: CustomSerializer(typeof(StringListSerializer))]


namespace USerialization
{
    public sealed class StringSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(string);

        public DataType DataType => DataType.String;

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var obj = Unsafe.Read<string>(fieldAddress);
            output.WriteString(obj);
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<string>(fieldAddress);
            value = input.ReadString();
        }

        public void Initialize(USerializer serializer)
        {
            
        }
    }

    public sealed class StringArraySerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(string[]);

        public DataType DataType => DataType.Array;

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var array = Unsafe.Read<string[]>(fieldAddress);
            if (array != null)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    var count = array.Length;

                    output.EnsureNext(5);

                    output.WriteByteUnchecked((byte)DataType.String);
                    output.WriteIntUnchecked(count);

                    for (var i = 0; i < count; i++)
                        output.WriteString(array[i]);
                }
                output.WriteSizeTrack(sizeTracker);
            }
            else
            {
                output.WriteNull();
            }
        }
        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var array = ref Unsafe.AsRef<string[]>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var type = (DataType)input.ReadByte();

                var count = input.ReadInt();
                array = new string[count];

                if (type == DataType.String)
                {
                    for (var i = 0; i < count; i++)
                        array[i] = input.ReadString();
                }

                input.EndObject(end);
            }
            else
            {
                array = null;
            }
        }

        public void Initialize(USerializer serializer)
        {
            
        }
    }

    public sealed class StringListSerializer : ICustomSerializer
    {
        public Type SerializedType => typeof(List<string>);

        public DataType DataType => DataType.Array;

        private readonly ListHelper<string> _listHelper;

        public StringListSerializer()
        {
            _listHelper = ListHelper<string>.Create();
        }

        public unsafe void Write(void* fieldAddress, SerializerOutput output)
        {
            var list = Unsafe.Read<List<string>>(fieldAddress);
            if (list != null)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    var count = list.Count;

                    output.EnsureNext(5);

                    output.WriteByteUnchecked((byte)DataType.String);
                    output.WriteIntUnchecked(count);

                    for (var i = 0; i < count; i++)
                        output.WriteString(list[i]);
                }
                output.WriteSizeTrack(sizeTracker);
            }
            else
            {
                output.WriteNull();
            }
        }

        public unsafe void Read(void* fieldAddress, SerializerInput input)
        {
            ref var list = ref Unsafe.AsRef<List<string>>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var type = (DataType)input.ReadByte();

                var count = input.ReadInt();
                list = new List<string>();
                var array = new string[count];

                _listHelper.SetArray(list, array);

                if (type == DataType.String)
                {
                    for (var i = 0; i < count; i++)
                        array[i] = input.ReadString();
                }

                input.EndObject(end);
            }
            else
            {
                list = null;
            }
        }

        public void Initialize(USerializer serializer)
        {
            
        }
    }

}