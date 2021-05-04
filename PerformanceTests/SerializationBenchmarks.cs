using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using MessagePack;

namespace PerformanceTests
{

    [ShortRunJob]
    public class SerializationBenchmarks
    {
        private USerializerBenchmark<BookShelf> _uSerializer;

        private MessagePackBenchmark<BookShelf> _messagePackBenchmark;

        private BookShelf _toSerialize;

        [GlobalSetup]
        public void Setup()
        {
            _uSerializer = new USerializerBenchmark<BookShelf>();
            _messagePackBenchmark = new MessagePackBenchmark<BookShelf>();
            _toSerialize = Data(10000);

            _uSerializer.Init(_toSerialize);
            _messagePackBenchmark.Init(_toSerialize);
        }

        public int BookDataSize = 64;

        private byte[] CreateAndFillByteBuffer()
        {
            byte[] optionalPayload = new byte[BookDataSize];

            for (int j = 0; j < optionalPayload.Length; j++)
            {
                optionalPayload[j] = (byte)(j % 26 + 'a');
            }

            return optionalPayload;
        }

        private BookShelf Data(int nToCreate)
        {
            var lret = new BookShelf("private member value")
            {
                Books = Enumerable.Range(1, nToCreate).Select(i => new Book
                {
                    Id = i,
                    Title = $"Book {i}",
                    BookData = CreateAndFillByteBuffer(),
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
        public void MessagePackSerialize()
        {
            _messagePackBenchmark.TestSerialize(_toSerialize);
        }

        [Benchmark()]
        public void MessagePackDeserialize()
        {
            _messagePackBenchmark.TestDeserialize(_toSerialize);
        }
    }

    [Serializable]
    [MessagePackObject]
    public class BookShelf
    {
        [Key(0)]
        [field: SerializeField]
        public List<Book> Books
        {
            get;
            set;
        }

        [Key(1)]
        [SerializeField]
        private string Secret;

        //[IgnoreMember]
        //public string GetSecret => Secret;

        public BookShelf(string secret)
        {
            Secret = secret;
        }

        public BookShelf()
        {
        }
    }

    [Serializable]
    [MessagePackObject]
    public class Book
    {
        [Key(0)]
        public string Title;

        [Key(1)]
        public int Id;

        [Key(2)]
        public byte[] BookData;
    }
}
