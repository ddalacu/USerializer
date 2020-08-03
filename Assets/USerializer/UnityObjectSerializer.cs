using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using USerialization;
using Object = UnityEngine.Object;

[assembly: CustomSerializer(typeof(ObjectWrapperSerialization))]

public class UnitySerialization
{
    [Serializable]
    public class GameObjectData
    {
        public string Name;

        public bool Active;

        public int Id;

        public List<ObjectWrapper> Components = new List<ObjectWrapper>();

        public void AddComponentEntry(Component component, int uniqueId)
        {
            Components.Add(new ObjectWrapper()
            {
                Instance = component,
                ID = uniqueId,
                TypeName= component.GetType().AssemblyQualifiedName
            });
        }
    }

    [Serializable]
    public class PrefabData
    {
        public List<GameObjectData> GameObjects = new List<GameObjectData>();
        public List<ObjectWrapper> Objects = new List<ObjectWrapper>();
    }

    private static readonly List<Component> ComponentsBuffer = new List<Component>(64);
    private static readonly Type ComponentType = typeof(Component);

    private static int GetObjectUniqueId(Object o)
    {
        return o.GetInstanceID();
    }

    private static void AddRoots(Transform transform, PrefabData prefabData)
    {
        var gameObject = transform.gameObject;

        if ((gameObject.hideFlags & HideFlags.DontSave) == HideFlags.DontSave)
            return;

        var gameObjectId = GetObjectUniqueId(gameObject);
        //_toBeSerialized.Insert(instanceId);
        //_toSerializeQueue.Enqueue(new ObjectAndSerializer(gameObject, GameObjectSerialization.GetGameObjectSerializeData(), instanceId));

        var gameObjectData = new GameObjectData()
        {
            Name = gameObject.name,
            Active = gameObject.activeSelf,
            Id = gameObjectId
        };


        prefabData.GameObjects.Add(gameObjectData);


        var transformChildCount = transform.childCount;
        for (int i = 0; i < transformChildCount; i++)
            AddRoots(transform.GetChild(i), prefabData);

        var buff = ComponentsBuffer;
        gameObject.GetComponents(ComponentType, buff);

        var bufferCount = buff.Count;
        for (int i = 0; i < bufferCount; i++)
        {
            var comp = buff[i];

            if (comp == null)
                continue;

            if ((comp.hideFlags & HideFlags.DontSave) == HideFlags.DontSave)
                continue;

            gameObjectData.AddComponentEntry(comp, GetObjectUniqueId(comp));
        }
    }

    public static void Serialize(GameObject go, SerializerOutput output)
    {
        var serializer = new USerializer(new UnitySerializationPolicy());

        var prefabData = new PrefabData();

        AddRoots(go.transform, prefabData);

        serializer.Serialize(output, prefabData);

    }

    public static void Deserialize(SerializerInput input)
    {
        var serializer = new USerializer(new UnitySerializationPolicy());

        serializer.TryDeserialize(input, out PrefabData prefabData);

        var set = new HashSet<int>();

        foreach (var prefabDataGameObject in prefabData.GameObjects)
        {
            var goInstance = new GameObject(prefabDataGameObject.Name);

            var componentsCount = prefabDataGameObject.Components.Count;

            for (var index = 0; index < componentsCount; index++)
            {
                var componentWrapper = prefabDataGameObject.Components[index];
                var type = Type.GetType(componentWrapper.TypeName);

                var existing = goInstance.GetComponent(type);

                if (existing != null)
                {
                    var existingInstanceId = existing.GetInstanceID();

                    if (set.Contains(existingInstanceId) == false)
                    {
                        set.Add(existingInstanceId);

                        prefabDataGameObject.Components[index].Instance = existing;

                        continue;
                    }
                }

                var component = goInstance.AddComponent(type);

                set.Add(component.GetInstanceID());

                prefabDataGameObject.Components[index].Instance = component;
            }
        }

        foreach (var prefabDataGameObject in prefabData.GameObjects)
        {
            var componentsCount = prefabDataGameObject.Components.Count;

            for (var index = 0; index < componentsCount; index++)
            {
                prefabDataGameObject.Components[index].Populate(input, serializer);
            }
        }
    }

}



public sealed class ObjectWrapperSerialization : ICustomSerializer
{
    public Type SerializedType => typeof(ObjectWrapper);
    public USerializer Serializer { get; set; }

    public unsafe void Write(void* fieldAddress, SerializerOutput output)
    {
        var wrapper = Unsafe.Read<ObjectWrapper>(fieldAddress);

        if (wrapper == null)
        {
            output.Null();
            return;
        }

        var instance = wrapper.Instance;

        if (Serializer.GetTypeData(instance.GetType(), out var typeData) == false)
            throw new Exception();

        output.OpenObject();
        {
            var serializers = typeData.Fields;
            var fieldsLength = serializers.Length;

            byte* objectAddress;
            UnsafeUtility.CopyObjectAddressToPtr(instance, &objectAddress);

            output.OpenField("$Id");
            output.WriteInt(wrapper.ID);
            output.CloseField();

            output.OpenField("$TypeName");
            output.WriteString(wrapper.TypeName);
            output.CloseField();

            for (var index = 0; index < fieldsLength; index++)
            {
                var serializer = serializers[index];
                output.OpenField(serializer.FieldInfo.Name);
                serializer.SerializationMethods.Serialize(objectAddress + serializer.Offset, output);
                output.CloseField();
            }
        }
        output.CloseObject();
    }

    public unsafe void Read(void* fieldAddress, SerializerInput input)
    {
        ref var value = ref Unsafe.AsRef<ObjectWrapper>(fieldAddress);

        if (value == null)
            value = new ObjectWrapper();

        value.Node = input.CurrentNode;

        if (input.BeginReadObject(out var enumerator))
        {
            var index = 0;
            enumerator.Next(ref index, out var idFieldName);
            value.ID = input.ReadInt();
            enumerator.Next(ref index, out var typeFieldName);
            value.TypeName = input.ReadString();

            //input.CloseObject();
        }

        input.MarkAsRead();
    }
}

[Serializable]
public unsafe class ObjectWrapper
{
    public SerializerInput.Node Node;

    public Object Instance;

    public int ID;

    public string TypeName;
    public void Populate(SerializerInput input, USerializer uSerializer)
    {
        var current = input.CurrentNode;
        input.CurrentNode = Node;

        if (input.BeginReadObject(out var enumerator))
        {
            if (uSerializer.GetTypeData(Instance.GetType(), out var typeData) == false)
            {
                throw new Exception();
            }

            byte* objectAddress;
            UnsafeUtility.CopyObjectAddressToPtr(Instance, &objectAddress);

            var fieldDatas = typeData.Fields;
            var fieldsLength = fieldDatas.Length;

            var readIndex = 2;
            while (enumerator.Next(ref readIndex, out var field))
            {
                for (var index = 0; index < fieldsLength; index++)
                {
                    var fieldData = fieldDatas[index];

                    if (field == fieldData.FieldInfo.Name)
                    {
                        fieldData.SerializationMethods.Deserialize(objectAddress + fieldData.Offset, input);
                        break;
                    }
                }
            }

            input.CloseObject();
        }
        else
        {
            Debug.LogError("Wtf");
        }

        input.CurrentNode = current;
    }
}
