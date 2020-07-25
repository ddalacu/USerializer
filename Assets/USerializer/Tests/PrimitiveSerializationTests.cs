using System.Collections.Generic;
using NUnit.Framework;

namespace Tests
{
    public class PrimitiveSerializationTests
    {

        [Test]
        public void IntSerialization()
        {
            TestUtils.SerializeDeserializeTest<int>(123);
        }

        [Test]
        public void IntArraySerialization()
        {
            TestUtils.SerializeDeserializeTest(new int[]
            {
                1,2,3
            });
        }

        [Test]
        public void IntListSerialization()
        {
            TestUtils.SerializeDeserializeTest(new List<int>
            {
                1,2,3
            });
        }

        [Test]
        public void FloatSerialization()
        {
            TestUtils.SerializeDeserializeTest<float>(123.133f);
        }

        [Test]
        public void FloatArraySerialization()
        {
            TestUtils.SerializeDeserializeTest(new float[]
            {
                1.1f,2.2f,3.3f
            });
        }

        [Test]
        public void FloatListSerialization()
        {
            TestUtils.SerializeDeserializeTest(new List<float>
            {
                1.1f,2.2f,3.3f
            });
        }


        [Test]
        public void BoolSerialization()
        {
            TestUtils.SerializeDeserializeTest<bool>(true);
        }

        [Test]
        public void BoolArraySerialization()
        {
            TestUtils.SerializeDeserializeTest(new bool[]
            {
               true,false,true
            });
        }

        [Test]
        public void BoolListSerialization()
        {
            TestUtils.SerializeDeserializeTest(new List<bool>
            {
                false,true,false
            });
        }


        [Test]
        public void StringSerialization()
        {
            TestUtils.SerializeDeserializeTest<string>("haolo");
        }

        [Test]
        public void StringArraySerialization()
        {
            TestUtils.SerializeDeserializeTest(new string[]
            {
                "haolo","haolo1","haolo2"
            });
        }

        [Test]
        public void StringListSerialization()
        {
            TestUtils.SerializeDeserializeTest(new List<string>
            {
                "haolo0","haolo00","haolo000"
            });
        }
    }
}
