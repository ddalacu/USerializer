using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using USerialization;

public class USerializerTest : MonoBehaviour
{
    [Serializable]
    private class TestClass
    {
        public int IntField;
        public Dictionary<int, float> DictField;
    }


    private void Update()
    {
        var initialSerialize = new MemoryStream();
        BinaryUtility.Serialize(new TestClass()
        {
            IntField = 112,
            DictField = new Dictionary<int, float>()
            {
                {1,2 },
                {2,3}
            }
        }, initialSerialize);

    }
}
