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


            uSerializer.TryDeserialize<T>(new SerializerInput(json), out var deserializeResult);

            output.Clear();
            uSerializer.Serialize(output, deserializeResult);
            var json2 = output.GetData();


            var ob = default(T);
            uSerializer.TryPopulateObject(new SerializerInput(json), ref ob);

            output.Clear();
            uSerializer.Serialize(output, deserializeResult);
            var json3 = output.GetData();

            Debug.Log(json);
            Debug.Log(json2);
            Debug.Log(json3);


            if (json == json2)
            {
                Debug.Log(json);
            }
            else
            {
                Debug.Log(json);
                Debug.Log(json2);

                Debug.Log(JsonUtility.ToJson(value));
                Debug.Log(JsonUtility.ToJson(deserializeResult));

                Assert.Fail();
            }
        }

    }
}