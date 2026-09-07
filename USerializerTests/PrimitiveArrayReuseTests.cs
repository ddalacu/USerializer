using System;
using System.Buffers;
using NUnit.Framework;
using USerialization;

namespace USerializerTests
{
    public class PrimitiveArrayReuseTests
    {
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        public void ReadReusesOnlyMatchingLength(int destinationLength)
        {
            var source = new[] { 10, 20, 30 };
            var destination = destinationLength < 0 ? null : new int[destinationLength];
            var original = destination;

            ReadInto(source, ref destination);

            CollectionAssert.AreEqual(source, destination);
            if (destinationLength == source.Length)
                Assert.AreSame(original, destination);
            else
                Assert.AreNotSame(original, destination);
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(3)]
        public void ReadEmptyReusesEmptyDestinationOrSharedEmptyArray(int destinationLength)
        {
            var destination = destinationLength < 0 ? null : new int[destinationLength];
            var original = destination;

            ReadInto(Array.Empty<int>(), ref destination);

            Assert.AreSame(destinationLength == 0 ? original : Array.Empty<int>(), destination);
        }

        [Test]
        public void ReadNullClearsDestination()
        {
            var destination = new[] { 10, 20, 30 };

            ReadInto(null, ref destination);

            Assert.IsNull(destination);
        }

        [Test]
        public void ReadMismatchedElementTypeClearsReusedArrayAndSkipsPayload()
        {
            Assert.IsTrue(BinaryUtility.USerializer.TryGetDataSerializer(typeof(int[]), out var serializer));
            var destination = new[] { 10, 20, 30 };
            var original = destination;
            var output = new SerializerOutput(64, ArrayPool<byte>.Shared);
            try
            {
                var tracker = output.BeginSizeTrack();
                output.WriteByte(3);
                output.WriteByte((byte)DataType.String);
                output.WriteByte(42);
                output.WriteSizeTrack(tracker);
                output.WriteByte(99);

                var input = new SerializerInput(output.BufferSpan.ToArray());
                serializer.Deserialize(ref destination, ref input);

                Assert.AreSame(original, destination);
                CollectionAssert.AreEqual(new int[3], destination);
                Assert.AreEqual(99, input.ReadByte());
            }
            finally
            {
                output.Dispose();
            }
        }

        private static void ReadInto(int[] source, ref int[] destination)
        {
            Assert.IsTrue(BinaryUtility.USerializer.TryGetDataSerializer(typeof(int[]), out var serializer));
            var output = new SerializerOutput(64, ArrayPool<byte>.Shared);
            try
            {
                serializer.Serialize(ref source, ref output);
                var input = new SerializerInput(output.BufferSpan.ToArray());
                serializer.Deserialize(ref destination, ref input);
            }
            finally
            {
                output.Dispose();
            }
        }
    }
}
