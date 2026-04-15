using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using USerialization;

namespace USerializerTests
{
    [TestFixture]
    public class HashSetSerializationTests
    {
        [Test]
        public void SerializeIntHashSetSuccess()
        {
            var hashSet = new HashSet<int> { 1, 2, 3, 4, 5 };
            var memStream = new MemoryStream();

            BinaryUtility.Serialize(hashSet, memStream);

            memStream.Position = 0;
            HashSet<int> deserialized = null;
            BinaryUtility.TryDeserialize(memStream, ref deserialized);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(hashSet.Count, deserialized.Count);
            foreach (var item in hashSet)
            {
                Assert.IsTrue(deserialized.Contains(item));
            }
        }

        [Test]
        public void SerializeStringHashSetSuccess()
        {
            var hashSet = new HashSet<string> { "one", "two", "three" };
            var memStream = new MemoryStream();

            BinaryUtility.Serialize(hashSet, memStream);

            memStream.Position = 0;
            HashSet<string> deserialized = null;
            BinaryUtility.TryDeserialize(memStream, ref deserialized);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(hashSet.Count, deserialized.Count);
            foreach (var item in hashSet)
            {
                Assert.IsTrue(deserialized.Contains(item));
            }
        }

        [Test]
        public void SerializeEmptyHashSetSuccess()
        {
            var hashSet = new HashSet<int>();
            var memStream = new MemoryStream();

            BinaryUtility.Serialize(hashSet, memStream);

            memStream.Position = 0;
            HashSet<int> deserialized = null;
            BinaryUtility.TryDeserialize(memStream, ref deserialized);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(0, deserialized.Count);
        }

        [Test]
        public void SerializeNullHashSetSuccess()
        {
            HashSet<int> hashSet = null;
            var memStream = new MemoryStream();

            // BinaryUtility.Serialize throws ArgumentNullException if obj is null
            Assert.Throws<ArgumentNullException>(() => BinaryUtility.Serialize(hashSet, memStream));
        }
        
        [Test]
        public void ReuseHashSetBufferSuccess()
        {
            var hashSet = new HashSet<int> { 1, 2, 3 };
            var memStream = new MemoryStream();

            BinaryUtility.Serialize(hashSet, memStream);

            memStream.Position = 0;
            var originalReference = hashSet;
            BinaryUtility.TryDeserialize(memStream, ref hashSet);

            Assert.AreSame(originalReference, hashSet);
            Assert.AreEqual(3, hashSet.Count);
            Assert.IsTrue(hashSet.Contains(1));
            Assert.IsTrue(hashSet.Contains(2));
            Assert.IsTrue(hashSet.Contains(3));
        }
    }
}
