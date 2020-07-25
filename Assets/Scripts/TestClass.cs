using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using USerialization;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;


public class TestClass : MonoBehaviour
{
    public int IntegerValue;

    public Vector2 _test;

    [SerializeField]
    private float FloatValue;

    [SerializeField]
    private NestedSerializeMe Nested;

    [SerializeField]
    private NestedStructSerializeMe NestedStruct;

    [SerializeField]
    private NestedSerializeMe[] NestedArray;

    [SerializeField]
    private NestedStructSerializeMe[] NestedStructArray;


    public List<int> IntegerList = new List<int>();

    [SerializeField]
    private List<float> _floatList = new List<float>();

    public List<NestedSerializeMe> NestedList = new List<NestedSerializeMe>();

    public List<NestedStructSerializeMe> NestedStructList = new List<NestedStructSerializeMe>();

    [Serializable]
    public class NestedSerializeMe
    {
        public int d;
        public Object UnityReference;
    }


    [Serializable]
    public struct NestedStructSerializeMe
    {
        public int IntegerValue;

        [SerializeField]
        private float FloatValue;

        public List<int> NestedList;
    }

    public enum TestEnum
    {
        ValueOne = 12,
        ValueTwo = 13
    }

    public TestEnum Enm = TestEnum.ValueTwo;

    public TestEnum[] Enums;

    public List<TestEnum> EnumsList;

    public int asd = 0;

}
