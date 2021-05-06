using System.IO;
using log4net.Config;
using UnityEngine;

public static class LoggingConfiguration
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ConfigureLogging()
    {
        string fileName = "log4net.config";
        var file = $"{Application.streamingAssetsPath}/{fileName}";

        if (File.Exists(file) == false)
        {
            Debug.LogError($"There is no {fileName} in streaming assets");
            return;
        }

        using (var stream = File.OpenRead(file))
        {
            XmlConfigurator.Configure(stream);
        }
    }
}