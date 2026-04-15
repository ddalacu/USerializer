using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace USerialization
{
    public class DictionarySerializer<TKey, TValue> : CustomDataSerializer
    {
        private DataSerializer _serializer;

        public override DataType DataType => DataType.Array;

        protected override void Initialize(USerializer serializer)
        {
            base.Initialize(serializer);
            if (serializer.TryGetDataSerializer(typeof(KeyValuePair<TKey, TValue>), out _serializer) == false)
                throw new Exception("Could not find serializer for KeyValuePair<TKey,TValue>");
        }

        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output, object context)
        {
            ref var instance = ref Unsafe.As<byte, Dictionary<TKey, TValue>>(ref MemoryMarshal.GetReference(span));
            if (instance == null)
            {
                output.WriteNull();
                return;
            }

            var count = instance.Count;
            if (count > 0)
            {
                var sizeTracker = output.BeginSizeTrack();
                output.EnsureNext(6);
                output.Write7BitEncodedIntUnchecked(count);
                output.WriteByteUnchecked((byte)_serializer.DataType);

                foreach (var kvPair in instance)
                {
                    var copy = kvPair;
                    ref var data = ref Unsafe.As<KeyValuePair<TKey, TValue>, byte>(ref copy);
                    _serializer.Write(MemoryMarshal.CreateSpan(ref data, Unsafe.SizeOf<KeyValuePair<TKey, TValue>>()), ref output, context);
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

        public override void Read(Span<byte> span, ref SerializerInput input, object context)
        {
            ref var instance = ref Unsafe.As<byte, Dictionary<TKey, TValue>>(ref MemoryMarshal.GetReference(span));
            if (input.BeginReadSize(out var end))
            {
                var count = input.Read7BitEncodedInt();
                if (instance == null)
                {
                    instance = new Dictionary<TKey, TValue>(count);
                }
                else
                {
                    instance.Clear();
                }

                if (count > 0)
                {
                    var type = (DataType)input.ReadByte();
                    if (type == _serializer.DataType)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            var copy = new KeyValuePair<TKey, TValue>();
                            ref var data = ref Unsafe.As<KeyValuePair<TKey, TValue>, byte>(ref copy);
                            _serializer.Read(MemoryMarshal.CreateSpan(ref data, Unsafe.SizeOf<KeyValuePair<TKey, TValue>>()), ref input, context);
                            instance.Add(copy.Key, copy.Value);
                        }
                    }
                    else
                    {
                        input.EndObject(end);
                    }
                }
            }
            else
            {
                instance = null;
            }
        }
    }
}