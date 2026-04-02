using System;
using NUnit.Framework;
using USerialization;

namespace USerializerTests
{
    [TestFixture]
    public class ObjectActivatorTests
    {
        public class PublicCtor
        {
            public bool Called;
            public PublicCtor() => Called = true;
        }

        public class PrivateCtor
        {
            public bool Called;
            private PrivateCtor() => Called = true;
        }

        public class NoDefaultCtor
        {
            public int Value;
            public NoDefaultCtor(int value) => Value = value;
        }

        [Test]
        public void CreateInstance_PublicConstructor_Works()
        {
            var instance = (PublicCtor)ObjectActivator.CreateInstance(typeof(PublicCtor));
            Assert.IsNotNull(instance);
            Assert.IsTrue(instance.Called);
        }

        [Test]
        public void CreateInstance_PrivateConstructor_Works()
        {
            var instance = (PrivateCtor)ObjectActivator.CreateInstance(typeof(PrivateCtor));
            Assert.IsNotNull(instance);
            Assert.IsTrue(instance.Called);
        }

        [Test]
        public void CreateInstance_NoDefaultConstructor_FallsBackToActivator()
        {
            // Activator.CreateInstance(typeof(NoDefaultCtor)) will throw MissingMethodException
            // but ObjectActivator should attempt it and throw the same exception or similar
            Assert.Throws<MissingMethodException>(() => ObjectActivator.CreateInstance(typeof(NoDefaultCtor)));
        }

        [Test]
        public void CreateInstance_Struct_Works()
        {
            var instance = (int)ObjectActivator.CreateInstance(typeof(int));
            Assert.AreEqual(0, instance);
        }
    }
}
