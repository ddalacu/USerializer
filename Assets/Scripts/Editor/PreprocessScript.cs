using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Content;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class TestScript : IPreprocessBuildWithReport, UnityEditor.Build.IPostprocessBuildWithReport
{
    public int callbackOrder { get; } = -1;
    public void OnPostprocessBuild(BuildReport report)
    {

        foreach (var reportPackedAsset in report.packedAssets)
        {
            Debug.Log(reportPackedAsset.file);
            Debug.Log(reportPackedAsset.shortPath);

            foreach (var packedAssetInfo in reportPackedAsset.contents)
            {
                if (packedAssetInfo.type == typeof(MonoScript))
                    continue;
              
                Debug.Log(packedAssetInfo.offset);

                Debug.Log(packedAssetInfo.id);
                Debug.Log(packedAssetInfo.sourceAssetGUID);
                Debug.Log(packedAssetInfo.sourceAssetPath);
                Debug.Log(packedAssetInfo.type);
            }
        }
    }

    public void OnPreprocessBuild(BuildReport report)
    {

    }
}
