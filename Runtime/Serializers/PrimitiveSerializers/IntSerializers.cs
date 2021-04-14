using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

[assembly: CustomSerializer(typeof(IntSerializer))]
[assembly: CustomSerializer(typeof(IntArraySerializer))]
[assembly: CustomSerializer(typeof(IntListSerializer))]


namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class IntSerializer : DataSerializer, ICustomSerializer
    {
        public Type SerializedType => typeof(int);

        public void Initialize(USerializer serializer)
        {

        }

        public DataSerializer GetMethods()
        {
            return this;
        }

        public IntSerializer() : base(DataType.Int32)
        {
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var value = *(int*)(fieldAddress);
            output.WriteInt(value);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            var value = (int*)(fieldAddress);
            *value = input.ReadInt();
        }
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class IntArraySerializer : DataSerializer, ICustomSerializer
    {
        public Type SerializedType => typeof(int[]);

        public void Initialize(USerializer serializer)
        {

        }

        public DataSerializer GetMethods()
        {
            return this;
        }

        public IntArraySerializer() : base(DataType.Array)
        {
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var array = Unsafe.Read<int[]>(fieldAddress);
            if (array != null)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    var count = array.Length;

                    output.EnsureNext(6 + (count * sizeof(int)));
                    output.WriteByteUnchecked((byte)DataType.Int32);
                    output.Write7BitEncodedIntUnchecked(count);

                    for (var i = 0; i < count; i++)
                        output.WriteIntUnchecked(array[i]);
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
            ref var array = ref Unsafe.AsRef<int[]>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var type = (DataType)input.ReadByte();

                var count = input.Read7BitEncodedInt();
                array = new int[count];

                if (type == DataType.Int32)
                {
                    for (var i = 0; i < count; i++)
                        array[i] = input.ReadInt();
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
    public sealed class IntListSerializer : DataSerializer, ICustomSerializer
    {
        public Type SerializedType => typeof(List<int>);

        private static readonly ListHelper<int> _listHelper;

        static IntListSerializer()
        {
            _listHelper = ListHelper<int>.Create();
        }

        public void Initialize(USerializer serializer)
        {

        }

        public DataSerializer GetMethods()
        {
            return this;
        }

        public IntListSerializer() : base(DataType.Array)
        {
        }

        public override unsafe void WriteDelegate(void* fieldAddress, SerializerOutput output)
        {
            var list = Unsafe.Read<List<int>>(fieldAddress);
            if (list == null)
            {
                output.WriteNull();
                return;
            }

            var sizeTracker = output.BeginSizeTrack();
            {
                var count = list.Count;


                output.EnsureNext(6 + (count * sizeof(int)));
                output.WriteByteUnchecked((byte)DataType.Int32);
                output.Write7BitEncodedIntUnchecked(count);

                for (var i = 0; i < count; i++)
                    output.WriteIntUnchecked(list[i]);
            }
            output.WriteSizeTrack(sizeTracker);
        }

        public override unsafe void ReadDelegate(void* fieldAddress, SerializerInput input)
        {
            ref var list = ref Unsafe.AsRef<List<int>>(fieldAddress);

            if (input.BeginReadSize(out var end))
            {
                var type = (DataType)input.ReadByte();

                var count = input.Read7BitEncodedInt();
                list = new List<int>();
                var array = new int[count];

                _listHelper.SetArray(list, array);

                if (type == DataType.Int32)
                {
                    for (var i = 0; i < count; i++)
                        array[i] = input.ReadInt();
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