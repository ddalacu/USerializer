using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using USerialization;

namespace USerializerTests
{
    public class TypeFieldsIteratorTests
    {
        public class A
        {
            public int AF;
            private int _AF;
        }

        public class B : A
        {
            public int BF;
            private int _BF;
        }

        public class C : B
        {
            public int CF;
            private int _CF;
        }

        [Test]
        public void ReturnAllFields()
        {
            using var iterator = new TypeFieldsIterator(typeof(C),
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var items = new List<FieldInfo>();

            while (iterator.MoveNext(out var current))
            {
                items.Add(current);
            }

            Assert.AreEqual(6, items.Count);

            var names = new HashSet<string>()
            {
                "CF",
                "_CF",
                "BF",
                "_BF",
                "AF",
                "_AF"
            };

            foreach (var item in items)
            {
                if (names.Remove(item.Name) == false)
                    Assert.Fail();
            }
        }

        [Serializable]
        public class MyClass : ISerializationCallbacks
        {
            public bool Before = false;
            public bool After = false;

            public void OnBeforeSerialize(object context)
            {
                if (context == "OnBeforeSerialize")
                    Before = true;
            }

            public void OnAfterSerialize(object context)
            {
                if (context == "OnAfterSerialize")
                    After = true;
            }
        }

        [Test]
        public void TestContextBeingPassed()
        {
            var memStream = new MemoryStream();
            var myClass = new MyClass();

            BinaryUtility.Serialize(myClass, memStream, "OnBeforeSerialize");

            memStream.Position = 0;
            var deser = BinaryUtility.TryDeserialize(memStream, ref myClass, "OnAfterSerialize");
            
            Assert.IsTrue(myClass.Before);
            Assert.IsTrue(myClass.After);
        }
    }
}