using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectFinder<T> where T : Object
{
    private static Dictionary<string, T> _map = new Dictionary<string, T>();

    public static T FindObject(string objectName)
    {
        if (_map.TryGetValue(objectName, out var result))
            return result;

        foreach (var findAsset in AssetDatabase.FindAssets(objectName))
        {
            var path = AssetDatabase.GUIDToAssetPath(findAsset);

            if (string.IsNullOrEmpty(path))
                continue;

            var assets = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (var asset in assets)
            {
                if (asset is T casted)
                {
                    _map.Add(objectName, casted);
                    return casted;
                }
            }
        }

        return null;
    }
}
