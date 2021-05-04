using System.IO;
using System.Runtime.CompilerServices;
using MessagePack;

namespace PerformanceTests
{

    public class MessagePackBenchmark<T> : SerializerBenchmark<T> where T : class
    {

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected override void Serialize(T obj, Stream stream)
        {
            MessagePackSerializer.Serialize(typeof(T), stream, obj);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected override T Deserialize(Stream stream)
        {
            return MessagePackSerializer.Deserialize<T>(stream);
        }
    }
}
