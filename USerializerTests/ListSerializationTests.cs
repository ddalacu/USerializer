using System;
using System.Collections;
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
            var itemsField = new FieldAccessHelper<List<T>, T[]>(itemsMember, runtimeUtils);
            var sizeField = new FieldAccessHelper<List<T>, int>(sizeMember, runtimeUtils);
            var array = itemsField.GetFieldRef(ref list);
            var count = sizeField.GetFieldRef(ref list);
            
            BinaryUtility.Serialize(list, memStream);

            for (var i = 0; i < elements.Length; i++)//fill some of the remaining space with non default values so we can check they are cleared
                list.Add(elements[i]);

            memStream.Position = 0;
            BinaryUtility.TryDeserialize(memStream, ref list);

            var deserializeArray = itemsField.GetFieldRef(ref list);
            var deserializeCount = sizeField.GetFieldRef(ref list);

            Assert.AreEqual(count, deserializeCount);
            Assert.AreSame(array, deserializeArray);
            
            for (var i = count; i < deserializeArray.Length; i++)
            {
                Console.WriteLine(deserializeArray[i]);
                Assert.AreEqual(default(T), deserializeArray[i]);
            }

            Assert.AreEqual(list.Capacity, elements.Length * 4);
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