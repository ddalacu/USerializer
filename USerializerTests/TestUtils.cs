using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;

namespace USerializerTests
{
    public class TestUtils
    {
        public static bool EqualArrays<T>(IList<T> a, IList<T> b)
        {
            var aNull = a == null;
            var bNull = b == null;

            if (aNull != bNull)
                return false;

            if (aNull)
                return true;

            var aLength = a.Count;
            if (aLength != b.Count)
                return false;

            var equalityComparer = EqualityComparer<T>.Default;

            for (var i = 0; i < aLength; i++)
            {
                if (equalityComparer.Equals(a[i], b[i]) == false)
                    return false;
            }

            return true;
        }

        public static T SerializeDeserializeTest<T>(T value)
        {
            var initialSerialize = new MemoryStream();
            var serialized = BinaryUtility.Serialize(value, initialSerialize);

            Assert.True(serialized);

            var initial = initialSerialize.Position;

            initialSerialize.Position = 0;

            T deserialize = default;

            var deserialized = BinaryUtility.TryDeserialize<T>(initialSerialize, ref deserialize);

            Assert.True(deserialized);

            Assert.True(initial == initialSerialize.Position);

            var secondSerialize = new MemoryStream();

            var reserialized = BinaryUtility.Serialize(deserialize, secondSerialize);

            Assert.True(reserialized);

            var ob = default(T);
            secondSerialize.Position = 0;
            var populated = BinaryUtility.TryPopulateObject(secondSerialize, ref ob);

            Assert.True(populated);

            var reserialize = new MemoryStream();
            var rereSeriaLized = BinaryUtility.Serialize(ob, reserialize);

            Assert.True(rereSeriaLized);

            Assert.True(EqualArrays(reserialize.ToArray(), reserialize.ToArray()));

            return ob;
        }
    }
}
