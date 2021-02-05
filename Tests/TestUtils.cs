﻿using System.Collections.Generic;
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
            var initialSerialize = new MemoryStream();
            BinaryUtility.Serialize(value, initialSerialize);

            var initial = initialSerialize.Position;

            initialSerialize.Position = 0;
            BinaryUtility.TryDeserialize<T>(initialSerialize, out var deserialize);

            Debug.Assert(initial == initialSerialize.Position);

            var secondSerialize = new MemoryStream();
            BinaryUtility.Serialize(deserialize, secondSerialize);

            var ob = default(T);
            secondSerialize.Position = 0;
            BinaryUtility.TryPopulateObject(secondSerialize, ref ob);

            var reserialize = new MemoryStream();
            BinaryUtility.Serialize(ob, reserialize);

            if (EqualArrays(reserialize.ToArray(), reserialize.ToArray()))
            {

            }
            else
            {
                Debug.Log(JsonUtility.ToJson(value));
                Debug.Log(JsonUtility.ToJson(ob));
                Assert.Fail();
            }

            return ob;
        }

        public static bool CompareSerializedContents<T>(IList<T> a,IList<T> b)
        {
            if (a.Count != b.Count)
                return false;

            for (int i = 0; i < a.Count; i++)
            {
                var elementA = a[i];
                var elementB = b[i];

                if (JsonUtility.ToJson(elementA) != JsonUtility.ToJson(elementB))
                    return false;
            }

            return true;
        }


    }
}