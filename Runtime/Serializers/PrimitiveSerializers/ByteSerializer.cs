using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(ByteSerializer))]
[assembly: CustomSerializer(typeof(ByteArraySerializer))]
[assembly: CustomSerializer(typeof(ByteListSerializer))]


namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class ByteSerializer : DataSerializer, ICustomSerializer
    {
        public Type SerializedType => typeof(byte);

        public void Initialize(USerializer serializer)
        {

        }

        public DataSerializer GetMethods()
        {
            return this;
        }

        public ByteSerializer() : base(DataType.Byte)
        {
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var value = *(byte*)(fieldAddress);
            output.WriteByte(value);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            var value = (byte*)(fieldAddress);
            *value = input.ReadByte();
        }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class ByteArraySerializer : DataSerializer, ICustomSerializer
    {
        public Type SerializedType => typeof(byte[]);

        public void Initialize(USerializer serializer)
        {

        }

        public DataSerializer GetMethods()
        {
            return this;
        }

        public ByteArraySerializer() : base(DataType.Array)
        {
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var array = Unsafe.Read<byte[]>(fieldAddress);
            if (array != null)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    var count = array.Length;

                    output.EnsureNext(6 + (count * sizeof(byte)));

                    output.WriteByteUnchecked((byte)DataType.Byte);
                    output.Write7BitEncodedIntUnchecked(count);

                    output.WriteBytesUnchecked(array, count);
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
            ref var array = ref Unsafe.AsRef<byte[]>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var type = (DataType)input.ReadByte();

                var count = input.Read7BitEncodedInt();

                if (type == DataType.Byte)
                {
                    array = input.ReadBytes(count);
                }
                else
                {
                    array = new byte[count];
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
    public sealed class ByteListSerializer : DataSerializer, ICustomSerializer
    {
        public Type SerializedType => typeof(List<byte>);

        private readonly ListHelper<byte> _listHelper;

        public ByteListSerializer() : base(DataType.Array)
        {
            _listHelper = ListHelper<byte>.Create();
        }

        public void Initialize(USerializer serializer)
        {

        }

        public DataSerializer GetMethods()
        {
            return this;
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var list = Unsafe.Read<List<byte>>(fieldAddress);
            if (list == null)
            {
                output.WriteNull();
                return;
            }

            var sizeTracker = output.BeginSizeTrack();
            {
                var array = _listHelper.GetArray(list, out var count);

                output.EnsureNext(6 + (count * sizeof(byte)));

                output.WriteByteUnchecked((byte)DataType.Byte);
                output.Write7BitEncodedIntUnchecked(count);
                output.WriteBytesUnchecked(array, count);
            }
            output.WriteSizeTrack(sizeTracker);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            ref var list = ref Unsafe.AsRef<List<byte>>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var type = (DataType)input.ReadByte();

                var count = input.Read7BitEncodedInt();
                list = new List<byte>();

                if (type == DataType.Byte)
                {
                    _listHelper.SetArray(list, input.ReadBytes(count));
                }
                else
                {
                    _listHelper.SetArray(list, new byte[count]);
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
