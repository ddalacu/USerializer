using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using USerialization;

namespace USerializerTests
{
    public class ListSerializationTests
    {
        public void TestReuseBuffer<T>(params T[] elements)
        {
            var list = new List<T>()
            {
                Capacity = elements.Length * 4
            };

            for (int i = 0; i < elements.Length; i++)
            {
                list.Add(elements[i]);
            }

            var memStream = new MemoryStream();
            
            var listType = typeof(List<T>);
            var itemsMember = listType.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
            var sizeMember = listType.GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic);

            if (itemsMember == null || sizeMember == null)
                throw new InvalidOperationException("Could not find List internal fields.");

            var runtimeUtils = BinaryUtility.USerializer.RuntimeUtils;
            var itemsOffset = runtimeUtils.GetFieldOffset(itemsMember);
            var sizeOffset = runtimeUtils.GetFieldOffset(sizeMember);
            
            var array = ListHelpers.GetArray(list, itemsOffset, sizeOffset, out var count);

            BinaryUtility.Serialize(list, memStream);
            
            for (int i = 0; i < elements.Length; i++)
            {
                list.Add(elements[i]);
            }

            memStream.Position = 0;
            BinaryUtility.TryDeserialize(memStream, ref list);
            
            var deserializeArray = ListHelpers.GetArray(list, itemsOffset, sizeOffset, out var deserializeCount);

            Assert.AreEqual(count, deserializeCount);
            Assert.AreSame(array, deserializeArray);

            Assert.AreEqual(list.Capacity , elements.Length * 4);
        }

        [Test]
        public void ReuseIntBufferSuccess()
        {
            TestReuseBuffer(1, 2, 3, 4);
        }

        [Test]
        public void ReuseByteBufferSuccess()
        {
            TestReuseBuffer<byte>(1, 2, 3, 4);
        }

        [Test]
        public void ReuseBoolBufferSuccess()
        {
            TestReuseBuffer<bool>(true, true, true);
        }

        [Serializable]
        public class Test
        {
            public int Value;
        }

        [Test]
        public void ReuseClassBufferSuccess()
        {
            TestReuseBuffer(
                new Test()
                {
                    Value = 1
                },
                new Test()
                {
                    Value = 2
                });
        }
    }
}