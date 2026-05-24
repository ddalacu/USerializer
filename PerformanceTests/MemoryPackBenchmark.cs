using System;
using System.IO;
using System.Runtime.CompilerServices;
using MemoryPack;

namespace PerformanceTests
{
    public class MemoryPackBenchmark<T> : SerializerBenchmark<T> where T : class
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        protected override void Serialize(T obj, MemoryStream stream)
        {
            var data = MemoryPackSerializer.Serialize(obj);
            stream.Write(data, 0, data.Length);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected override T Deserialize(MemoryStream stream)
        {
            var len = stream.Length;
            var readOnlySpan = new ReadOnlySpan<byte>(stream.GetBuffer(), 0, (int) len);

            return MemoryPackSerializer.Deserialize<T>(readOnlySpan);
        }
    }
}