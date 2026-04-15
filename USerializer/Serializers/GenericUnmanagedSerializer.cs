using System;
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
                        input.ReadSpan(array.AsSpan());
                    }
                    else
                    {
                        ArrayHelpers.CleanArray(array, 0, (uint)count);
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

        private int _itemsOffset;
        private int _sizeOffset;

        public override bool TryInitialize(USerializer serializer)
        {
            var listType = typeof(List<T>);
            var itemsMember = listType.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
            var sizeMember = listType.GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic);

            if (itemsMember == null || sizeMember == null)
                throw new InvalidOperationException("Could not find List internal fields.");

            _itemsOffset = serializer.RuntimeUtils.GetFieldOffset(itemsMember);
            _sizeOffset = serializer.RuntimeUtils.GetFieldOffset(sizeMember);
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

                    var array = ListHelpers.GetArray(list, _itemsOffset, _sizeOffset, out _);
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
                    var array = ListHelpers.PrepareArray(ref list, count, _itemsOffset, _sizeOffset);

                    var type = (DataType)input.ReadByte();

                    if (type == _elementDataType)
                    {
                        input.ReadSpan(array.AsSpan().Slice(0, (int)count));
                    }
                    else
                    {
                        ArrayHelpers.CleanArray(array, 0, (uint)count);
                        input.EndObject(end);
                    }
                }
                else
                {
                    if (list == null)
                        list = new List<T>();
                    else
                        ListHelpers.SetCount(list, 0, _sizeOffset);
                }
            }
            else
            {
                list = null;
            }
        }
    }
}