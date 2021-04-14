using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace USerialization
{
    ///// <summary>
    ///// Not working on il2cpp due to the fact it needs il emitting :( ill try finding other ways to do this
    ///// </summary>
    //public class DictionarySerializer : ISerializationProvider
    //{
    //    private USerializer _serializer;

    //    public void Initialize(USerializer serializer)
    //    {
    //        _serializer = serializer;
    //    }

    //    public void Start(USerializer serializer)
    //    {

    //    }

    //    public unsafe bool TryGetSerializationMethods(Type type, out DataSerializer dataSerializer)
    //    {
    //        if (type.IsConstructedGenericType == false)
    //        {
    //            dataSerializer = default;
    //            return false;
    //        }

    //        if (type.GetGenericTypeDefinition() != typeof(Dictionary<,>))
    //        {
    //            dataSerializer = default;
    //            return false;
    //        }

    //        var arguments = type.GetGenericArguments();
    //        var keyType = arguments[0];
    //        var valueType = arguments[1];

    //        if (_serializer.TryGetSerializationMethods(keyType, out var keySerializer) == false)
    //        {
    //            dataSerializer = default;
    //            return false;
    //        }

    //        if (_serializer.TryGetSerializationMethods(valueType, out var valueSerializer) == false)
    //        {
    //            dataSerializer = default;
    //            return false;
    //        }

    //        var cType = typeof(DictSerializer<,>).MakeGenericType(new Type[]
    //        {
    //            keyType,
    //            valueType
    //        });

    //        var instance = (IDictionarySerializer)Activator.CreateInstance(cType);

    //        dataSerializer = new SerializationMethods(instance.Write, instance.Read, DataType.Object);
    //        return true;
    //    }

    //    public unsafe interface IDictionarySerializer
    //    {
    //        void Write(void* fieldAddress, SerializerOutput output);

    //        void Read(void* fieldAddress, SerializerInput input);
    //    }

    //    [Preserve]
    //    public unsafe class DictSerializer<TKey, TValue> : IDictionarySerializer
    //    {
    //        public void Write(void* fieldAddress, SerializerOutput output)
    //        {
    //            ref var dictionary = ref Unsafe.AsRef<Dictionary<TKey, TValue>>(fieldAddress);
    //            if (dictionary != null)
    //            {
    //                Debug.Log(typeof(TKey));
    //                Debug.Log(typeof(TValue));

    //                Debug.Log("Count " + dictionary.Count);
    //            }
    //        }

    //        public void Read(void* fieldAddress, SerializerInput input)
    //        {
    //            Debug.Log("Bruh2");
    //        }
    //    }

    //}
}