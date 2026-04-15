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

        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output)
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
                    _serializer.Serialize(ref copy, ref output);
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
                            _serializer.Deserialize(ref copy, ref input);
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