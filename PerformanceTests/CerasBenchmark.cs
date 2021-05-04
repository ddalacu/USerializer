using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using Ceras;

namespace PerformanceTests
{
    public class CerasBenchmark<T> : SerializerBenchmark<T> where T : class
    {
        private CerasSerializer _cerasSerializer;

        byte[] _buffer = null;

        public CerasBenchmark()
        {
            _cerasSerializer = new CerasSerializer(new SerializerConfig()
            {
                PreserveReferences = false,
                DefaultTargets = TargetMember.All,
                VersionTolerance =
                {
                    Mode = VersionToleranceMode.Standard,
                    VerifySizes = false
                },
            });

            _buffer = new byte[2048 * 100];
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected override void Serialize(T obj, Stream stream)
        {
            int nBytesWritten = _cerasSerializer.Serialize<T>(obj, ref _buffer);
            stream.Write(_buffer, 0, nBytesWritten);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        protected override T Deserialize(Stream stream)
        {
            var pooledArray = ArrayPool<byte>.Shared.Rent((int)stream.Length);
            stream.Read(pooledArray, 0, (int)stream.Length);
            T ret = default;
            int offset = 0;
            _cerasSerializer.Deserialize<T>(ref ret, pooledArray, ref offset, (int)stream.Length);
            ArrayPool<byte>.Shared.Return(pooledArray);
            return ret;
        }
    }
}