using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.IL2CPP.CompilerServices;
using USerialization;

namespace USerialization
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    public sealed class StringSerializer : CustomDataSerializer
    {
        public override DataType GetDataType() => DataType.String;

        public override void Write(ReadOnlySpan<byte> span, SerializerOutput output, object context)
        {
            Debug.Assert(span.Length == IntPtr.Size);
            ref var value = ref Unsafe.As<byte, string>(ref MemoryMarshal.GetReference(span));

            if (value == null)
            {
                //Write7BitEncodedInt(0);
                output.WriteByte(0);
                return;
            }

            var valueLength = value.Length;
            var byteLength = valueLength * sizeof(char);

            output.EnsureNext(byteLength + 5); //5 if from the max size of Write7BitEncodedIntUnchecked
            output.Write7BitEncodedIntUnchecked(valueLength + 1);
            output.WriteSpan(value.AsSpan());
        }

        public override void Read(Span<byte> span, SerializerInput input, object context)
        {
            Debug.Assert(span.Length == IntPtr.Size);
            ref var value = ref Unsafe.As<byte, string>(ref MemoryMarshal.GetReference(span));

            var length = input.Read7BitEncodedInt();

            length -= 1;

            if (length == -1)
            {
                value = null;
                return;
            }

#if DEBUG
            if (length < 0)
                throw new Exception("byteLength is negative!");
#endif
            
            if (length == 0)
            {
                value = string.Empty;
                return;
            }
            
            var chars = input.GetNext<char>(length);
            value = new string(chars);
        }
    }

    public sealed class StringDataSkipper : IDataSkipper
    {
        public void Skip(SerializerInput input)
        {
            var chars = input.Read7BitEncodedInt();

            chars -= 1;

            if (chars == -1) //null
                return;

            input.Skip(chars * sizeof(char));
        }
    }
}