using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using USerialization;

namespace Tests
{
    public class TestUtils
    {
        private static bool EqualArrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }

        public static T SerializeDeserializeTest<T>(T value)
        {
            var output = new SerializerOutput(2048);
            var uSerializer = new USerializer(new UnitySerializationPolicy());

            uSerializer.Serialize(output, value);

            var result = output.GetData();
            var memStream = new MemoryStream(result, 0, (int)output.Length);

            uSerializer.TryDeserialize<T>(new SerializerInput(memStream), out var deserializeResult);

            output.Clear();
            uSerializer.Serialize(output, deserializeResult);
            var json2 = output.GetData();


            var ob = default(T);
            memStream.Position = 0;
            uSerializer.TryPopulateObject(new SerializerInput(memStream), ref ob);

            output.Clear();
            uSerializer.Serialize(output, deserializeResult);
            var json3 = output.GetData();

            //Debug.Log(result);
            //Debug.Log(json2);
            //Debug.Log(json3);


            if (EqualArrays(result, json2))
            {

            }
            else
            {
                Debug.Log(result);
                Debug.Log(json2);

                Debug.Log(JsonUtility.ToJson(value));
                Debug.Log(JsonUtility.ToJson(deserializeResult));

                Assert.Fail();
            }

            return deserializeResult;
        }

        public static string UnitySerializeArray<T>(IList<T> list)
        {
            var builder = new StringBuilder();

            builder.Append('[');

            for (int i = 0; i < list.Count; i++)
            {
                builder.Append(JsonUtility.ToJson(list[i]));

                if (i < list.Count - 1)
                    builder.Append(',');
            }

            builder.Append(']');

            return builder.ToString();
        }


    }
}