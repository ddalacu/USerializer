using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;
using USerialization;


/// <summary>
/// Please note that the serializer can serialize/deserialize the list by itself
/// In this example we are showing that we can read/write multiple objects in the same stream
/// </summary>
public class USerializerAdvancedExample : MonoBehaviour
{

    /// <summary>
    /// The serialization system supports everything unity supports (difference being it serializes null objects)
    /// <see cref="ISerializationCallbackReceiver"/>
    /// <see cref="FormerlySerializedAsAttribute"/>
    /// <see cref="NonSerializedAttribute"/>
    /// <see cref="SerializeField"/>
    /// </summary>
    [Serializable]
    public class StateData : ISerializationCallbackReceiver
    {
        public int Coins;
        public int Gold;

        public void OnBeforeSerialize()
        {
            Debug.Log("Object will get serialized!");
        }

        public void OnAfterDeserialize()
        {
            Debug.Log("Object was deserialized!");
        }
    }

    private List<StateData> _states = new List<StateData>();

    [SerializeField]
    private string _fileName = "AdvancedStateData.bin";

    [SerializeField]
    private int _count = 3;

    private USerializer _uSerializer;

    private void Awake()
    {
        Debug.Log($"Target file {GetSaveFile()}");

        for (var i = 0; i < _count; i++)
            _states.Add(new StateData());

        var providers = ProvidersUtils.GetDefaultProviders();
        _uSerializer = new USerializer(new UnitySerializationPolicy(), providers);
    }

    private string GetSaveFile()
    {
        return Path.Combine(Application.persistentDataPath, _fileName);
    }

    /// <summary>
    /// The benefit of creating the <see cref="USerializer"/> instance and <see cref="SerializerOutput"/>
    /// we can reuse them for multiple objects, <see cref="BinaryUtility"/> will create a <see cref="SerializerOutput"/>
    /// each time we serialize something
    /// </summary>
    private void Save()
    {
        using (var stream = new FileStream(GetSaveFile(), FileMode.Create
            , FileAccess.ReadWrite, FileShare.None, 16, FileOptions.WriteThrough))//we don't need big buffer size because we already have a internal buffer
        {
            var output = new SerializerOutput(2048, stream);

            output.WriteInt(_states.Count);
            foreach (var stateData in _states)
                _uSerializer.Serialize(output, stateData);

            //BinaryUtility.Serialize(_states, stream);
        }
    }

    /// <summary>
    /// The benefit of creating the <see cref="USerializer"/> instance and <see cref="SerializerInput"/>
    /// we can reuse them for multiple objects, <see cref="BinaryUtility"/> will create a <see cref="SerializerInput"/>
    /// each time we deserialize something
    /// </summary>
    private void Load()
    {
        using (var stream = new FileStream(GetSaveFile(), FileMode.Open
            , FileAccess.ReadWrite, FileShare.None, 16))//we don't need big buffer size because we already have a internal buffer
        {
            var serializerInput = new SerializerInput(2048, stream);

            var count = serializerInput.ReadInt();

            _states = new List<StateData>(count);

            for (int i = 0; i < count; i++)
            {
                _uSerializer.TryDeserialize(serializerInput, out StateData data);
                _states.Add(data);
            }

            //BinaryUtility.TryPopulateObject(stream, ref _states); this would be a simplier way
        }
    }

    private void OnGUI()
    {
        for (var index = 0; index < _states.Count; index++)
        {
            var stateData = _states[index];

            GUILayout.Box($"Index {index}");

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(nameof(StateData.Coins));
                var txt = GUILayout.TextField(stateData.Coins.ToString());

                if (int.TryParse(txt, out var coins))
                {
                    stateData.Coins = coins;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label(nameof(StateData.Gold));
                var txt = GUILayout.TextField(stateData.Gold.ToString());

                if (int.TryParse(txt, out var coins))
                {
                    stateData.Gold = coins;
                }
            }
            GUILayout.EndHorizontal();
        }


        if (GUILayout.Button("Save"))
        {
            Save();
        }

        if (File.Exists(GetSaveFile()))
        {
            if (GUILayout.Button("Populate"))
            {
                Load();
            }

            if (GUILayout.Button("Delete Save"))
            {
                File.Delete(GetSaveFile());
            }
        }
    }
}
