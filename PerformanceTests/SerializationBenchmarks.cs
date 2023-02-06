using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Ceras;
using MemoryPack;

namespace PerformanceTests
{
    [ShortRunJob]
    public class SerializationBenchmarks
    {
        private USerializerBenchmark<BookShelf> _uSerializer;

        private MemoryPackBenchmark<BookShelf> _memoryPackBenchmark;

        private CerasBenchmark<BookShelf> _cerasBenchmark;

        private BookShelf _toSerialize;

        [GlobalSetup]
        public void Setup()
        {
            _uSerializer = new USerializerBenchmark<BookShelf>();
            _memoryPackBenchmark = new MemoryPackBenchmark<BookShelf>();
            _cerasBenchmark = new CerasBenchmark<BookShelf>();
            _toSerialize = Data(10000);

            _uSerializer.Init(_toSerialize);
            _memoryPackBenchmark.Init(_toSerialize);
            _cerasBenchmark.Init(_toSerialize);
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
            var lret = new BookShelf("private member value")
            {
                Books = Enumerable.Range(1, nToCreate).Select(i => new Book
                    {
                        Id = i,
                        // Title = $"Book {i}",
                        // BookData = CreateAndFillByteBuffer(),
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

        [Benchmark()]
        public void CerasSerialize()
        {
            _cerasBenchmark.TestSerialize(_toSerialize);
        }

        [Benchmark()]
        public void CerasDeserialize()
        {
            _cerasBenchmark.TestDeserialize(_toSerialize);
        }
    }

    [Serializable]
    [MemoryPackable(GenerateType.VersionTolerant)]
    [MemberConfig(TargetMember.All)]
    public partial class BookShelf
    {
        [field: SerializeField]
        [MemoryPackOrder(1)]
        public List<Book> Books { get; set; }

        // [SerializeField]
        // public string Secret;

        //[IgnoreMember]
        //public string GetSecret => Secret;

        public BookShelf(string secret)
        {
            //Secret = secret;
        }

        [MemoryPackConstructor()]
        public BookShelf()
        {
        }
    }

    [Serializable]
    [MemberConfig(TargetMember.All)]
    [MemoryPackable(GenerateType.VersionTolerant)]
    public partial class Book
    {
        //public string Title;
        [MemoryPackOrder(1)]
        public int Id;
        //
        // public byte[] BookData;
    }
}