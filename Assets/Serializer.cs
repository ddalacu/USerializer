using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using USerialization;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;


public class Serializer : MonoBehaviour
{
    private USerializer _uSerializer;


    public GameObject ToSerialize;

    private void Awake()
    {
        _uSerializer = new USerializer(new UnitySerializationPolicy());

        var watch = Stopwatch.StartNew();
        _uSerializer.PreCacheLayout(typeof(TestClass));
        watch.Stop();
        Debug.Log($"Cache {watch.Elapsed.TotalMilliseconds}");

        _internals = ObjectInternals.Create();
    }

    private ObjectInternals _internals;


    [Serializable]
    public class FieldAccessTest
    {
        public int Value;
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    private unsafe void DirectSet(FieldAccessTest instance, int iterations)
    {
        var watch = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            instance.Value = i;
        }

        Debug.Log($"Direct elapsed ms {watch.Elapsed.TotalMilliseconds}");
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    private unsafe void UnsafeSet(FieldAccessTest instance, int iterations)
    {
        var type = typeof(FieldAccessTest);
        _uSerializer.GetTypeData(type, out var data);

        var watch = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            var address = (byte*)UnsafeUtility.PinGCObjectAndGetAddress(instance, out var handle);

            var fieldData = data.Fields[0];
            var fieldAddress = address + fieldData.Offset;
            *(int*)fieldAddress = i;

            UnsafeUtility.ReleaseGCObject(handle);
        }

        Debug.Log($"Unsafe elapsed ms {watch.Elapsed.TotalMilliseconds}");
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    private unsafe void UnsafeSetNoPin(FieldAccessTest instance, int iterations)
    {
        var type = typeof(FieldAccessTest);
        _uSerializer.GetTypeData(type, out var data);
        var fieldData = data.Fields[0];

        var watch = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            var address = (byte*)(*(void**)Unsafe.AsPointer(ref instance));

            var fieldAddress = address + fieldData.Offset;
            *(int*)fieldAddress = i;
        }

        Debug.Log($"UnsafeSetNoPin elapsed ms {watch.Elapsed.TotalMilliseconds}");
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    private unsafe void UnsafeSetNoPin2(FieldAccessTest instance, int iterations)
    {
        var type = typeof(FieldAccessTest);
        _uSerializer.GetTypeData(type, out var data);
        var fieldData = data.Fields[0];

        var watch = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            //var address = (*(void**)Unsafe.AsPointer(ref instance));
            fieldData.AsRef<int>(instance) = i;
        }

        Debug.Log($"UnsafeSetNoPin2 elapsed ms {watch.Elapsed.TotalMilliseconds}");
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    private unsafe void UnsafeSetNoPin3(FieldAccessTest instance, int iterations)
    {
        var type = typeof(FieldAccessTest);
        _uSerializer.GetTypeData(type, out var data);
        var fieldData = data.Fields[0];

        var watch = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            var address = (*(void**)Unsafe.AsPointer(ref instance));
            fieldData.AsRef<int>(address) = i;

            fixed (byte* objAddress = &GetPinnableReference(instance))
            {
                var fieldAddress = objAddress + fieldData.Offset;
                *(int*)fieldAddress = i;
            }
        }

        Debug.Log($"UnsafeSetNoPin3 elapsed ms {watch.Elapsed.TotalMilliseconds}");
    }


    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    private unsafe void ReflectionSet(FieldAccessTest instance, int iterations)
    {
        var field = typeof(FieldAccessTest).GetField("Value");

        var watch = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            field.SetValue(instance, i);
        }

        Debug.Log($"Reflection elapsed ms {watch.Elapsed.TotalMilliseconds}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ref byte GetPinnableReference(object obj)
    {
        return ref *(byte*)*(void**)Unsafe.AsPointer(ref obj);
    }
    public sealed class PinnableObject
    {
        public byte Pinnable;
    }

    public struct Test
    {

    }

    public class EmptyClass
    {
        public class Example
        {
            public int asd;
        }

        public Example[] _field;
    }


    public unsafe void TestPinning()
    {
        var empty = new EmptyClass();

        var pinnableObject = Unsafe.As<PinnableObject>(empty);

        fixed (void* ptr = &pinnableObject.Pinnable)
        {
            var emptyAddress = (byte*)(*(void**)Unsafe.AsPointer(ref empty));

            Debug.Log(new IntPtr(emptyAddress));

            Debug.Log(new IntPtr(ptr));
        }
    }
    public unsafe void TestMethodPin()
    {
        var empty = new EmptyClass();

        var address = (byte*)UnsafeUtility.PinGCObjectAndGetAddress(empty, out var handle);

        void* ptr = null;
        UnsafeUtility.CopyObjectAddressToPtr(empty, &ptr);

        Debug.Log(new IntPtr(ptr));
        Debug.Log(new IntPtr(address));

        UnsafeUtility.ReleaseGCObject(handle);


        var array = new EmptyClass[2];

        address = (byte*)UnsafeUtility.PinGCArrayAndGetDataAddress(array, out handle);

        ptr = null;
        UnsafeUtility.CopyObjectAddressToPtr(array, &ptr);



        Debug.Log(new IntPtr(ptr));
        Debug.Log(new IntPtr(address));

        UnsafeUtility.ReleaseGCObject(handle);

    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    private unsafe void CustomInstanceId(Object ob, int iterations)
    {
        var watch = Stopwatch.StartNew();
        var value = 0;
        for (int i = 0; i < iterations; i++)
        {
            if (UnityHelpers.GetInstanceId(ob) != 0)
            {
                value++;
            }
        }

        Debug.Log($"Custom elapsed ms {watch.Elapsed.TotalMilliseconds}");
    }

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    private unsafe void CustomInstanceIdTwo(Object ob, int iterations)
    {
        var value = 0;
        var watch = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            if (_internals.GetInstanceId(ob) != 0)
            {
                value++;
            }
        }

        Debug.Log($"Custom2 elapsed ms {watch.Elapsed.TotalMilliseconds}");
    }


    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]

    private unsafe void DefaultInstanceId(Object ob, int iterations)
    {
        var watch = Stopwatch.StartNew();
        var value = 0;
        for (int i = 0; i < iterations; i++)
        {
            if (ob.GetInstanceID() != 0)
            {
                value++;
            }
        }

        Debug.Log($"Default elapsed ms {watch.Elapsed.TotalMilliseconds}");
    }


    private unsafe void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            var output = new SerializerOutput(2048);


            //UnitySerialization.Serialize(ToSerialize, output);

            var message = output.GetData();

            Debug.Log(message);

            //UnitySerialization.Deserialize(new SerializerInput(message));

            return;

            var testClass = ToSerialize.GetComponent<TestClass>();
            _uSerializer.Serialize(output, testClass);

            var data = message;
            Debug.Log(data);
            Debug.Log(JsonUtility.ToJson(testClass));

            _uSerializer.DeserializeObject(new SerializerInput(new MemoryStream(data)), testClass);
        }


        return;

        Debug.Log(UnityHelpers.GetInstanceId(this));
        Debug.Log(this.GetInstanceID());

        //var iterations = 1000000;
        //CustomInstanceId(this, iterations);
        //DefaultInstanceId(this, iterations);
        //CustomInstanceIdTwo(this, iterations);

        //return;
        //TestPinning();
        //TestMethodPin();
        //return;


        var instance = new FieldAccessTest();

        instance.Value = 0;
        UnsafeSet(instance, 99999);


        instance.Value = 0;
        ReflectionSet(instance, 99999);

        instance.Value = 0;
        DirectSet(instance, 99999);

        instance.Value = 0;
        UnsafeSetNoPin(instance, 99999);

        instance.Value = 0;
        UnsafeSetNoPin2(instance, 99999);

        instance.Value = 0;
        UnsafeSetNoPin3(instance, 99999);


        return;
        fixed (byte* objAddress = &GetPinnableReference(instance))
        {
            Debug.Log(new IntPtr(objAddress));
        }

        var address = (byte*)(*(void**)Unsafe.AsPointer(ref instance));
        Debug.Log(new IntPtr(address));

        var pinnable = Unsafe.As<PinnableObject>(instance);
        fixed (void* ptr = &pinnable.Pinnable)
        {
            Debug.Log(new IntPtr(ptr));
        }

        //if (Input.GetKeyDown(KeyCode.Space) == false)
        //    return;

        //var ser = new UnityObjectSerializer();

        //var prefabOutput = new SerializerOutput(2048);

        //ser.Serialize(_uSerializer, prefabOutput, ToSerialize);

        //var json = prefabOutput.GetData();

        //Debug.Log(json);

        ////return;

        //var input = new SerializerInput(json);
        //ser.Deserialize(_uSerializer, input);
    }
}
