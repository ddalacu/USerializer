using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using USerialization;

namespace USerializerTests
{
    public class PrimitiveCollectionTagTests
    {
        [TestCase(typeof(double), DataType.Double)]
        [TestCase(typeof(float), DataType.Float)]
        [TestCase(typeof(int), DataType.Int32)]
        [TestCase(typeof(uint), DataType.UInt32)]
        [TestCase(typeof(long), DataType.Int64)]
        [TestCase(typeof(ulong), DataType.UInt64)]
        [TestCase(typeof(short), DataType.Int16)]
        [TestCase(typeof(ushort), DataType.UInt16)]
        [TestCase(typeof(byte), DataType.Byte)]
        [TestCase(typeof(sbyte), DataType.SByte)]
        [TestCase(typeof(bool), DataType.Bool)]
        [TestCase(typeof(char), DataType.Char)]
        public void WritesElementTagForArraysAndLists(Type elementType, DataType expectedTag)
        {
            var array = Array.CreateInstance(elementType, 1);
            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
            list.Add(array.GetValue(0));

            foreach (object source in new object[] { array, list })
            {
                var bytes = Serialize(source);
                var input = new SerializerInput(bytes);
                Assert.IsTrue(input.BeginReadSize(out var end));
                Assert.AreEqual(1, input.Read7BitEncodedInt());
                Assert.AreEqual((byte)expectedTag, input.ReadByte(), source.GetType().ToString());
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MismatchedPrimitiveTagClearsDestinationAndSkipsPayload(bool list)
        {
            object source = list ? (object)new List<long> { 10, 20, 30 } : new long[] { 10, 20, 30 };
            var bytes = Serialize(source);
            object destination = list ? (object)new List<int> { 1, 2, 3 } : new[] { 1, 2, 3 };
            var original = destination;
            Assert.IsTrue(BinaryUtility.USerializer.TryGetDataSerializer(destination.GetType(), out var serializer));
            var input = new SerializerInput(bytes);

            serializer.Deserialize(ref destination, ref input);

            Assert.AreSame(original, destination);
            CollectionAssert.AreEqual(new int[3], (IEnumerable)destination);
            Assert.AreEqual(99, input.ReadByte());
        }

        private static byte[] Serialize(object source)
        {
            Assert.IsTrue(BinaryUtility.USerializer.TryGetDataSerializer(source.GetType(), out var serializer));
            var output = new SerializerOutput(64, ArrayPool<byte>.Shared);
            try
            {
                serializer.Serialize(ref source, ref output);
                output.WriteByte(99);
                return output.BufferSpan.ToArray();
            }
            finally
            {
                output.Dispose();
            }
        }
    }
}
