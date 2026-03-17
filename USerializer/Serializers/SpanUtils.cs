using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace USerialization
{
    public class SpanUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> GetByteSpan<TItem>(ref TItem item)
        {
            ref var data = ref Unsafe.As<TItem, byte>(ref item);
            return MemoryMarshal.CreateSpan(ref data, Unsafe.SizeOf<TItem>());
        }
    }
}