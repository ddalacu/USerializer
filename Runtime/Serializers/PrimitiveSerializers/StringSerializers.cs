using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(StringSerializer))]
[assembly: CustomSerializer(typeof(StringArraySerializer))]
[assembly: CustomSerializer(typeof(StringListSerializer))]


namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class StringSerializer : CustomDataSerializer
    {
        public override Type SerializedType => typeof(string);

        public StringSerializer() : base(DataType.String)
        {
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var obj = Unsafe.Read<string>(fieldAddress);
            output.WriteString(obj);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            ref var value = ref Unsafe.AsRef<string>(fieldAddress);
            value = input.ReadString();
        }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class StringArraySerializer : CustomDataSerializer
    {
        public override Type SerializedType => typeof(string[]);

        public StringArraySerializer() : base(DataType.Array)
        {

        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var array = Unsafe.Read<string[]>(fieldAddress);
            if (array != null)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    var count = array.Length;

                    output.EnsureNext(6);
                    output.WriteByteUnchecked((byte)DataType.String);
                    output.Write7BitEncodedIntUnchecked(count);

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

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            ref var array = ref Unsafe.AsRef<string[]>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var type = (DataType)input.ReadByte();

                var count = input.Read7BitEncodedInt();
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
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class StringListSerializer : CustomDataSerializer
    {
        public override Type SerializedType => typeof(List<string>);

        private static readonly ListHelper<string> _listHelper;

        static StringListSerializer()
        {
            _listHelper = ListHelper<string>.Create();
        }

        public StringListSerializer() : base(DataType.Array)
        {
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var list = Unsafe.Read<List<string>>(fieldAddress);
            if (list != null)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    var count = list.Count;

                    output.EnsureNext(6);
                    output.WriteByteUnchecked((byte)DataType.String);
                    output.Write7BitEncodedIntUnchecked(count);

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

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            ref var list = ref Unsafe.AsRef<List<string>>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var type = (DataType)input.ReadByte();

                var count = input.Read7BitEncodedInt();

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
    }

}