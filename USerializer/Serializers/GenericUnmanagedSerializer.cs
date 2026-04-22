using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace USerialization
{
    public class GenericUnmanagedSerializer<T> : CustomDataSerializer
        where T : unmanaged
    {
        private DataType _dataType;

        public override DataType DataType => _dataType;

        public GenericUnmanagedSerializer(DataType dataType)
        {
            _dataType = dataType;
        }

        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output)
        {
            Debug.Assert(span.Length == Unsafe.SizeOf<T>());

            ref var item = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span));
            output.Write(item);
            // ref var reference = ref MemoryMarshal.GetReference(fieldAddress);
            // T value= Unsafe.As<byte, T>(ref Unsafe.AsRef( reference));
            // Console.WriteLine(fieldAddress.Length==Unsafe.SizeOf<T>());
            // output.Write(value);
        }

        public override void Read(Span<byte> span, ref SerializerInput input)
        {
            Debug.Assert(span.Length == Unsafe.SizeOf<T>());
            ref var item = ref Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(span));
            item = input.Read<T>();
        }
    }

    public class GenericUnmanagedArraySerializer<T> : CustomDataSerializer
        where T : unmanaged
    {
        private DataType _elementDataType;

        public override DataType DataType => DataType.Array;

        public override unsafe void Write(ReadOnlySpan<byte> span, ref SerializerOutput output)
        {
            Debug.Assert(span.Length == IntPtr.Size);

            var array = Unsafe.As<byte, T[]>(ref MemoryMarshal.GetReference(span));
            if (array == null)
            {
                output.WriteNull();
                return;
            }

            var count = array.Length;

            if (count > 0)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    var byteLength = count * sizeof(T);

                    output.EnsureNext(6 + byteLength);
                    output.Write7BitEncodedIntUnchecked(count);
                    output.WriteByteUnchecked((byte)_elementDataType);
                    output.WriteSpan<T>(array.AsSpan());
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

            ref var array = ref Unsafe.As<byte, T[]>(ref MemoryMarshal.GetReference(span));

            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();
                array = new T[count];

                if (count > 0)
                {
                    var type = (DataType)input.ReadByte();
                    if (type == _elementDataType)
                    {
                        input.FillSpan(array.AsSpan());
                    }
                    else
                    {
                        Array.Clear(array, 0, count);
                        input.EndObject(end);
                    }
                }
            }
            else
            {
                array = null;
            }
        }
    }

    public class GenericUnmanagedListSerializer<T> : CustomDataSerializer
        where T : unmanaged
    {
        private DataType _elementDataType;

        private FieldAccessHelper<List<T>, T[]> _itemsField;

        private FieldAccessHelper<List<T>, int> _sizeField;

        public override bool TryInitialize(USerializer serializer)
        {
            var listType = typeof(List<T>);
            var itemsMember = listType.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
            var sizeMember = listType.GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic);

            if (itemsMember == null || sizeMember == null)
                throw new InvalidOperationException("Could not find List internal fields.");

            _itemsField = new FieldAccessHelper<List<T>, T[]>(itemsMember, serializer.RuntimeUtils);
            _sizeField = new FieldAccessHelper<List<T>, int>(sizeMember, serializer.RuntimeUtils);
            return true;
        }

        public override DataType DataType => DataType.Array;

        public override unsafe void Write(ReadOnlySpan<byte> span, ref SerializerOutput output)
        {
            ref var list = ref Unsafe.As<byte, List<T>>(ref MemoryMarshal.GetReference(span));

            if (list == null)
            {
                output.WriteNull();
                return;
            }

            var count = list.Count;

            if (count > 0)
            {
                var sizeTracker = output.BeginSizeTrack();
                {
                    var byteLength = count * sizeof(T);

                    output.EnsureNext(6 + byteLength);
                    output.Write7BitEncodedIntUnchecked(count);
                    output.WriteByteUnchecked((byte)_elementDataType);

                    ref var array = ref _itemsField.GetFieldRef(ref list);
                    var slice = array.AsSpan().Slice(0, count);
                    output.WriteSpan<T>(slice);
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
            ref var list = ref Unsafe.As<byte, List<T>>(ref MemoryMarshal.GetReference(span));

            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();

                if (count > 0)
                {
                    if (list == null)
                    {
                        list = new List<T>(count);
                    }
                    else
                    {
                        ref var existingArray = ref _itemsField.GetFieldRef(ref list);
                        if (list.Capacity < count)
                        {
                            existingArray = new T[count];
                        }
                        else
                        {
                            var remaining = list.Count - count;
                            if (remaining > 0)
                            {
                                Array.Clear(existingArray, count, remaining);
                            }
                        }
                    }

                    var type = (DataType)input.ReadByte();
                    ref var array = ref _itemsField.GetFieldRef(ref list);
                    ref var size = ref _sizeField.GetFieldRef(ref list);
                    size = count;

                    if (type == _elementDataType)
                    {
                        input.FillSpan(array.AsSpan().Slice(0, (int)count));
                    }
                    else
                    {
                        Array.Clear(array, 0, count);
                        input.EndObject(end);
                    }
                }
                else
                {
                    if (list == null)
                        list = new List<T>();
                    else
                    {
                        ref var array = ref _itemsField.GetFieldRef(ref list);
                        Array.Clear(array, 0, array.Length);
                        ref var size = ref _sizeField.GetFieldRef(ref list);
                        size = 0;
                    }
                }
            }
            else
            {
                list = null;
            }
        }
    }
}