using System;
using NUnit.Framework;
using USerialization;

namespace USerializerTests
{
    [TestFixture]
    public class NonZeroIntPtrSetTests
    {
        [Test]
        public void Insert_NewKey_ReturnsTrue()
        {
            var set = new NonZeroIntPtrSet(10);
            var key = new IntPtr(1);
            Assert.IsTrue(set.Insert(key));
            Assert.AreEqual(1, set.Count);
        }

        [Test]
        public void Insert_ExistingKey_ReturnsFalse()
        {
            var set = new NonZeroIntPtrSet(10);
            var key = new IntPtr(1);
            set.Insert(key);
            Assert.IsFalse(set.Insert(key));
            Assert.AreEqual(1, set.Count);
        }

        [Test]
        public void Contains_ExistingKey_ReturnsTrue()
        {
            var set = new NonZeroIntPtrSet(10);
            var key = new IntPtr(1);
            set.Insert(key);
            Assert.IsTrue(set.Contains(key));
        }

        [Test]
        public void Contains_NonExistingKey_ReturnsFalse()
        {
            var set = new NonZeroIntPtrSet(10);
            var key = new IntPtr(1);
            Assert.IsFalse(set.Contains(key));
        }

        [Test]
        public void Insert_MultipleKeys_RehashesCorrectly()
        {
            // Initial capacity 4, rehash at 3 (4 * 0.75)
            var set = new NonZeroIntPtrSet(4);
            Assert.IsTrue(set.Insert(new IntPtr(1)));
            Assert.IsTrue(set.Insert(new IntPtr(2)));
            // This third insert should trigger rehash
            Assert.IsTrue(set.Insert(new IntPtr(3)));
            
            Assert.AreEqual(3, set.Count);
            Assert.IsTrue(set.Contains(new IntPtr(1)));
            Assert.IsTrue(set.Contains(new IntPtr(2)));
            Assert.IsTrue(set.Contains(new IntPtr(3)));
        }

        [Test]
        public void Clear_RemovesAllKeys()
        {
            var set = new NonZeroIntPtrSet(10);
            set.Insert(new IntPtr(1));
            set.Insert(new IntPtr(2));
            set.Clear();
            Assert.AreEqual(0, set.Count);
            Assert.IsFalse(set.Contains(new IntPtr(1)));
            Assert.IsFalse(set.Contains(new IntPtr(2)));
        }

        [Test]
        public void Insert_Zero_ThrowsArgumentException()
        {
            var set = new NonZeroIntPtrSet(10);
            Assert.Throws<ArgumentException>(() => set.Insert(IntPtr.Zero));
        }
    }
}
