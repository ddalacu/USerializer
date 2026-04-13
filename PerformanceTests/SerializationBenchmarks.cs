using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using MemoryPack;

namespace PerformanceTests
{
    [ShortRunJob]
    public class SerializationBenchmarks
    {
        private USerializerBenchmark<BookShelf> _uSerializer;

        private MemoryPackBenchmark<BookShelf> _memoryPackBenchmark;

        private BookShelf _toSerialize;

        [GlobalSetup]
        public void Setup()
        {
            _uSerializer = new USerializerBenchmark<BookShelf>();
            _memoryPackBenchmark = new MemoryPackBenchmark<BookShelf>();
            _toSerialize = Data(10000);

            _uSerializer.Init(_toSerialize);
            _memoryPackBenchmark.Init(_toSerialize);
        }

        public const int BookDataSize = 64;

        public static byte[] CreateAndFillByteBuffer()
        {
            byte[] optionalPayload = new byte[BookDataSize];

            for (int j = 0; j < optionalPayload.Length; j++)
            {
                optionalPayload[j] = (byte) (j % 26 + 'a');
            }

            return optionalPayload;
        }

        public static BookShelf Data(int nToCreate)
        {
            var lret = new BookShelf()
            {
                Books = Enumerable.Range(1, nToCreate).Select(i => new Book
                    {
                        Id = i,
                        ByteValue = 123,
                        DoubleValue = 123
                    }
                ).ToList()
            };
            return lret;
        }

        [Benchmark()]
        public void USerializerSerialize()
        {
            _uSerializer.TestSerialize(_toSerialize);
        }

        [Benchmark()]
        public void USerializerDeserialize()
        {
            _uSerializer.TestDeserialize(_toSerialize);
        }

        [Benchmark()]
        public void MemoryPackSerialize()
        {
            _memoryPackBenchmark.TestSerialize(_toSerialize);
        }

        [Benchmark()]
        public void MemoryPackDeserialize()
        {
            _memoryPackBenchmark.TestDeserialize(_toSerialize);
        }
    }

    [Serializable]
    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class BookShelf
    {
        [field: SerializeField]
        [MemoryPackOrder(1)]
        public List<Book> Books { get; set; }

        [MemoryPackOrder(2)]
        public int Id;

        [MemoryPackOrder(3)]
        public double DoubleValue;
        
        [MemoryPackOrder(4)]
        public byte ByteValue;
    }

    [Serializable]
    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class Book
    {
        //public string Title;
        [MemoryPackOrder(1)]
        public int Id;

        [MemoryPackOrder(2)]
        public double DoubleValue;
        
        [MemoryPackOrder(2)]
        public byte ByteValue;
        
        //
        // public byte[] BookData;
    }
}