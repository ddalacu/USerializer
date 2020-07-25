using NUnit.Framework;
using UnityEngine;
using USerialization;

namespace Tests
{
    public class TestUtils
    {
        public static void SerializeDeserializeTest<T>(T value)
        {
            var output = new SerializerOutput(2048);
            var uSerializer = new USerializer();

            uSerializer.Serialize(output, value);

            var json = output.GetData();

            var input = new SerializerInput(json);

            uSerializer.Deserialize<T>(input, out var result);

            output.Clear();
            uSerializer.Serialize(output, result);
            var json2 = output.GetData();

            if (json == json2)
            {
                Debug.Log(json);
            }
            else
            {
                Debug.Log(json);
                Debug.Log(json2);

                Debug.Log(JsonUtility.ToJson(value));
                Debug.Log(JsonUtility.ToJson(result));

                Assert.Fail();
            }
        }

    }
}