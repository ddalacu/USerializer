using System.IO;

namespace PerformanceTests
{
    public abstract class SerializerBenchmark<T> where T : class
    {
        private MemoryStream _memoryStream;

        private const int Iterations = 10;

        private MemoryStream _deserializeStream;

        protected SerializerBenchmark()
        {
            _memoryStream = new MemoryStream(1000 * 1000 * 1000);
        }

        public void Init(T obj)
        {
            _deserializeStream = new MemoryStream();
            Serialize(obj, _deserializeStream);
        }

        public void TestSerialize(T obj)
        {
            for (var i = 0; i < Iterations; i++)
            {
                Serialize(obj, _memoryStream);
                _memoryStream.SetLength(0);
                _memoryStream.Position = 0;
            }
        }

        public void TestDeserialize(T obj)
        {
            for (var i = 0; i < Iterations; i++)
            {
                _deserializeStream.Position = 0;
                Deserialize(_deserializeStream);
            }
        }

        protected abstract void Serialize(T obj, Stream stream);

        protected abstract T Deserialize(Stream stream);
    }
}