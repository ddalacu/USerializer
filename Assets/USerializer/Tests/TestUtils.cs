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
        public static bool EqualArrays<T>(IList<T> a, IList<T> b)
        {
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

        public static bool EqualArrays<T>(IList<T> a, IList<T> b, int count)
        {
            var aLength = a.Count;
            if (aLength < count)
                return false;
            if (b.Count < count)
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
            var output = new SerializerOutput(2048);
            var uSerializer = new USerializer(new UnitySerializationPolicy());

            uSerializer.Serialize(output, value);


            uSerializer.TryDeserialize<T>(new SerializerInput(output.GetData()), out var deserializeResult);

            output.Clear();
            uSerializer.Serialize(output, deserializeResult);
            var json2 = output.GetData();


            var ob = default(T);

            uSerializer.TryPopulateObject(new SerializerInput(output.GetData()), ref ob);

            output.Clear();
            uSerializer.Serialize(output, deserializeResult);

            //Debug.Log(result);
            //Debug.Log(json2);
            //Debug.Log(json3);

            //if (EqualArrays(output.GetData(), json2, EqualArrays))
            //{

            //}
            //else
            //{
            //    Debug.Log(JsonUtility.ToJson(value));
            //    Debug.Log(JsonUtility.ToJson(deserializeResult));

            //    Assert.Fail();
            //}

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