using System;
using System.Reflection;
using NUnit.Framework;
using USerialization;

namespace USerializerTests
{
    [TestFixture]
    public class FieldAccessHelperTests
    {
        private class TestClass
        {
            public int PublicInt;
            private string _privateString;
            public float PublicFloat;

            public void SetPrivateString(string value) => _privateString = value;
            public string GetPrivateString() => _privateString;
        }

        private IRuntimeUtils _runtimeUtils;

        [SetUp]
        public void SetUp()
        {
            _runtimeUtils = new NETRuntimeUtils();
        }

        [Test]
        public void FieldAccess_PublicInt_Works()
        {
            var fieldInfo = typeof(TestClass).GetField(nameof(TestClass.PublicInt));
            var helper = new FieldAccessHelper<TestClass, int>(fieldInfo, _runtimeUtils);

            var instance = new TestClass { PublicInt = 42 };

            ref int valueRef = ref helper.GetFieldRef(ref instance);

            Assert.That(valueRef, Is.EqualTo(42));

            valueRef = 100;
            Assert.That(((TestClass)instance).PublicInt, Is.EqualTo(100));
        }

        [Test]
        public void FieldAccess_PrivateString_Works()
        {
            var fieldInfo =
                typeof(TestClass).GetField("_privateString", BindingFlags.Instance | BindingFlags.NonPublic);
            var helper = new FieldAccessHelper<TestClass, string>(fieldInfo, _runtimeUtils);

            var instanceObj = new TestClass();
            instanceObj.SetPrivateString("initial");

            ref string valueRef = ref helper.GetFieldRef(ref instanceObj);

            Assert.That(valueRef, Is.EqualTo("initial"));

            valueRef = "modified";
            Assert.That(instanceObj.GetPrivateString(), Is.EqualTo("modified"));
        }

        [Test]
        public void FieldAccess_WrongType_ThrowsArgumentException()
        {
            var fieldInfo = typeof(TestClass).GetField(nameof(TestClass.PublicInt));

            Assert.Throws<ArgumentException>(() =>
            {
                _ = new FieldAccessHelper<TestClass, string>(fieldInfo, _runtimeUtils);
            });
        }

        [Test]
        public void FieldAccess_MultipleFields_Works()
        {
            var intField = typeof(TestClass).GetField(nameof(TestClass.PublicInt));
            var floatField = typeof(TestClass).GetField(nameof(TestClass.PublicFloat));

            var intHelper = new FieldAccessHelper<TestClass, int>(intField, _runtimeUtils);
            var floatHelper = new FieldAccessHelper<TestClass, float>(floatField, _runtimeUtils);

            var instance = new TestClass { PublicInt = 1, PublicFloat = 2.5f };

            ref int intRef = ref intHelper.GetFieldRef(ref instance);
            ref float floatRef = ref floatHelper.GetFieldRef(ref instance);

            Assert.That(intRef, Is.EqualTo(1));
            Assert.That(floatRef, Is.EqualTo(2.5f));

            intRef = 10;
            floatRef = 20.5f;

            Assert.That(((TestClass)instance).PublicInt, Is.EqualTo(10));
            Assert.That(((TestClass)instance).PublicFloat, Is.EqualTo(20.5f));
        }

        private class BaseClass
        {
            protected int ProtectedInt = 5;
        }

        private class DerivedClass : BaseClass
        {
            public int DerivedInt = 10;
        }

        [Test]
        public void FieldAccess_InheritedField_Works()
        {
            var fieldInfo =
                typeof(DerivedClass).GetField("ProtectedInt", BindingFlags.Instance | BindingFlags.NonPublic);
            var helper = new FieldAccessHelper<DerivedClass, int>(fieldInfo, _runtimeUtils);

            var instance = new DerivedClass();

            ref int valueRef = ref helper.GetFieldRef(ref instance);

            Assert.That(valueRef, Is.EqualTo(5));

            valueRef = 55;
            // Need reflection to verify since it's protected
            var val = typeof(BaseClass).GetField("ProtectedInt", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(instance);
            Assert.That(val, Is.EqualTo(55));
        }

        private struct TestStruct
        {
            public int StructInt;
            public string StructString;
        }
    }
}