using System.IO;

namespace PerformanceTests
{
    public abstract class SerializerBenchmark<T> where T : class
    {
        private MemoryStream _memoryStream;

        private MemoryStream _deserializeStream;

        public MemoryStream DeserializeStream => _deserializeStream;
        
        protected SerializerBenchmark()
        {
            _memoryStream = new MemoryStream(1024 * 4);
        }

        public void Init(T obj)
        {
            _deserializeStream = new MemoryStream();
            Serialize(obj, _deserializeStream);
        }

        public void TestSerialize(T obj)
        {
            Serialize(obj, _memoryStream);
            _memoryStream.SetLength(0);
            _memoryStream.Position = 0;
        }

        public void TestDeserialize(T obj)
        {
            _deserializeStream.Position = 0;
            Deserialize(_deserializeStream);
        }

        protected abstract void Serialize(T obj, Stream stream);

        protected abstract T Deserialize(Stream stream);
    }
}