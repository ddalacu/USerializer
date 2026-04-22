using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace USerialization
{
    public sealed class StringSerializer : CustomDataSerializer
    {
        public override DataType DataType => DataType.String;

        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        public override void Write(ReadOnlySpan<byte> span, ref SerializerOutput output)
        {
            Debug.Assert(span.Length == IntPtr.Size);
            ref var value = ref Unsafe.As<byte, string>(ref MemoryMarshal.GetReference(span));

            if (value == null)
            {
                output.WriteNull();
                return;
            }

            var length = value.Length;

            if (length == 0)
            {
                output.Write<int>(0);
                return;
            }

            var maxPossible = Utf8.GetMaxByteCount(length);

            var track = output.BeginSizeTrack();
            {
                var outputSpan = output.GetWriteableSpan(maxPossible);
                var written = Utf8.GetBytes(value.AsSpan(), outputSpan);
                output.AdvancePosition(written);
            }

            output.WriteSizeTrack(track);
        }

        public override void Read(Span<byte> span, ref SerializerInput input)
        {
            Debug.Assert(span.Length == IntPtr.Size);
            ref var value = ref Unsafe.As<byte, string>(ref MemoryMarshal.GetReference(span));

            var length = input.Read<int>();
            if (length >= 0)
            {
                if (length == 0)
                {
                    value = string.Empty;
                }
                else
                {
                    var byteSpan = input.GetSpan(length);
                    value = Utf8.GetString(byteSpan);
                }
            }
            else
            {
                value = null;
            }
        }
    }

    public sealed class StringDataSkipper : IDataSkipper
    {
        public void Skip(ref SerializerInput input)
        {
            var bytes = input.Read<int>();
            input.Skip(bytes);
        }
    }
}