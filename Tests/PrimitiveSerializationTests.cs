using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Unity.Collections.LowLevel.Unsafe;
using USerialization;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace Tests
{
    public class PrimitiveSerializationTests
    {
        public static unsafe T BuildData<T>() where T : unmanaged
        {
            var size = sizeof(T);

            var item = new T();

            var addr = (byte*)UnsafeUtility.AddressOf(ref item);

            for (int i = 0; i < size; i++)
                addr[i] = (byte)Random.Range(0, 255);

            return item;
        }

        public static List<T> BuildDataList<T>(int count = 100) where T : unmanaged
        {
            var list = new List<T>(count);
            for (int i = 0; i < count; i++)
                list.Add(BuildData<T>());

            return list;
        }

        public static T[] BuildDataArray<T>(int count = 100) where T : unmanaged
        {
            var array = new T[count];
            for (int i = 0; i < count; i++)
                array[i] = BuildData<T>();

            return array;
        }

        [Test]
        public void IntSerialization()
        {
            var input = BuildData<int>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(input == result);
        }

        [Test]
        public void IntArraySerialization()
        {
            var input = BuildDataArray<int>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void IntListSerialization()
        {
            var input = BuildDataList<int>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void UIntSerialization()
        {
            var input = BuildData<uint>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(input == result);
        }

        [Test]
        public void UIntArraySerialization()
        {
            var input = BuildDataArray<uint>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void UIntListSerialization()
        {
            var input = BuildDataList<uint>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }
        [Test]
        public void ShortSerialization()
        {
            var input = BuildData<short>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(input == result);
        }

        [Test]
        public void ShortArraySerialization()
        {
            var input = BuildDataArray<short>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void ShortListSerialization()
        {
            var input = BuildDataList<short>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void ByteSerialization()
        {
            var input = BuildData<byte>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(input == result);
        }

        [Test]
        public void ByteArraySerialization()
        {
            var input = BuildDataArray<byte>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void ByteListSerialization()
        {
            var input = BuildDataList<byte>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void UShortSerialization()
        {
            var input = BuildData<ushort>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(input == result);
        }

        [Test]
        public void UShortArraySerialization()
        {
            var input = BuildDataArray<ushort>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void UShortListSerialization()
        {
            var input = BuildDataList<ushort>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void LongSerialization()
        {
            var input = BuildData<long>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(input == result);
        }

        [Test]
        public void LongArraySerialization()
        {
            var input = BuildDataArray<long>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void LongListSerialization()
        {
            var input = BuildDataList<long>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void ULongSerialization()
        {
            var input = BuildData<ulong>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(input == result);
        }

        [Test]
        public void ULongArraySerialization()
        {
            var input = BuildDataArray<ulong>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void ULongListSerialization()
        {
            var input = BuildDataList<ulong>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void FloatSerialization()
        {
            var input = BuildData<float>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(input == result);
        }

        [Test]
        public void FloatArraySerialization()
        {
            var input = BuildDataArray<float>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void FloatListSerialization()
        {
            var input = BuildDataList<float>();
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }


        [Test]
        public void BoolSerialization()
        {
            var result = TestUtils.SerializeDeserializeTest(true);
            Debug.Assert(true == result);
        }

        [Test]
        public void BoolArraySerialization()
        {
            var input = new bool[] { false, true, false, false };
            var result = TestUtils.SerializeDeserializeTest(input);

            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void BoolListSerialization()
        {
            var input = new List<bool>() { false, true, false, false };
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void CharSerialization()
        {
            var result = TestUtils.SerializeDeserializeTest('a');
            Debug.Assert('a' == result);
        }

        [Test]
        public void CharArraySerialization()
        {
            var input = new[] { 'a', 'b', 'c', 'd' };
            var result = TestUtils.SerializeDeserializeTest(input);

            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void CharListSerialization()
        {
            var input = new List<char>() { 'a', 'b', 'c', 'd' };
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void StringSerialization()
        {
            var input = "haolo";
            var result = TestUtils.SerializeDeserializeTest<string>(input);
            Debug.Assert(input == result);
        }

        [Test]
        public void StringArraySerialization()
        {
            var input = new string[]
            {
                "haolo","haolo1","haolo2"
            };
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

        [Test]
        public void StringListSerialization()
        {
            var input = new List<string>
            {
                "haolo0","haolo00","haolo000"
            };
            var result = TestUtils.SerializeDeserializeTest(input);
            Debug.Assert(TestUtils.EqualArrays(input, result));
        }

    }
}
