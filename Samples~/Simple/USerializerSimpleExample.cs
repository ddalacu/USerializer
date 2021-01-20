using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using USerialization;

public class USerializerSimpleExample : MonoBehaviour
{
    [Serializable]
    public class StateData
    {
        public int Coins;
        public int Gold;
    }

    private List<StateData> _states = new List<StateData>();

    [SerializeField]
    private string _fileName = "SimpleStateData.bin";

    [SerializeField]
    private int _count = 3;

    private void Awake()
    {
        Debug.Log($"Target file {GetSaveFile()}");

        for (var i = 0; i < _count; i++)
            _states.Add(new StateData());
    }

    private string GetSaveFile()
    {
        return Path.Combine(Application.persistentDataPath, _fileName);
    }

    private void Save()
    {
        using (var stream = new FileStream(GetSaveFile(), FileMode.Create
            , FileAccess.ReadWrite, FileShare.None, 16, FileOptions.WriteThrough))//we don't need big buffer size because we already have a internal buffer
        {
            BinaryUtility.Serialize(_states, stream);
        }
    }

    private void Load()
    {
        using (var stream = new FileStream(GetSaveFile(), FileMode.Open
            , FileAccess.ReadWrite, FileShare.None, 16))//we don't need big buffer size because we already have a internal buffer
        {
            //BinaryUtility.TryDeserialize(stream, out State); you can use this one or populate but populate might allocates less
            BinaryUtility.TryPopulateObject(stream, ref _states);
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
