using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using USerialization;

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

        public static T SerializeDeserializeTest<T>(T value) where T : class
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
            var populated = BinaryUtility.TryDeserialize(secondSerialize, ref ob);

            Assert.True(populated);

            var reserialize = new MemoryStream();
            var rereSeriaLized = BinaryUtility.Serialize(ob, reserialize);

            Assert.True(rereSeriaLized);

            Assert.True(EqualArrays(reserialize.ToArray(), reserialize.ToArray()));

            return ob;
        }


        public static T SerializeDeserializeStructTest<T>(T value) where T : struct
        {
            var initialSerialize = new MemoryStream();

            if (BinaryUtility.USerializer.TryGetValueHelper<T>(out var valueSerializer) == false)
                throw new Exception($"Cannot serialize {typeof(T)}");


            var output = new SerializerOutput(2048, initialSerialize);
            valueSerializer.Serialize(ref value, output, null);
            output.Flush();

            var initial = initialSerialize.Position;

            initialSerialize.Position = 0;

            T deserialize = default;

            var serializerInput = new SerializerInput(2048, initialSerialize);
            valueSerializer.Populate(ref deserialize, serializerInput, null);
            serializerInput.FinishRead();

            Assert.True(initial == initialSerialize.Position);

            var secondSerialize = new MemoryStream();

            var output2 = new SerializerOutput(2048, secondSerialize);
            valueSerializer.Serialize(ref deserialize, output2, null);
            output2.Flush();

            var ob = default(T);
            secondSerialize.Position = 0;

            var serializerInput2 = new SerializerInput(2048, secondSerialize);
            valueSerializer.Populate(ref ob, serializerInput2, null);
            serializerInput2.FinishRead();

            var reserialize = new MemoryStream();

            var output3 = new SerializerOutput(2048, reserialize);
            valueSerializer.Serialize(ref ob, output3, null);
            output3.Flush();

            Assert.True(EqualArrays(reserialize.ToArray(), reserialize.ToArray()));

            return ob;
        }
    }
}