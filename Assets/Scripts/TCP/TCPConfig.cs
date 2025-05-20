using UnityEngine;
using System.IO;

[System.Serializable]
public class TCPConfiguration
{
    public string serverIP = "127.0.0.1";
    public int serverPort = 5005;
}

public static class TCPConfig
{
    private static string ConfigFileName = "tcpconfig.json";
    private static string ConfigFilePath => Path.Combine(Application.streamingAssetsPath, ConfigFileName);
    
    private static TCPConfiguration _cachedConfig;
    
    public static TCPConfiguration GetConfig()
    {
        if (_cachedConfig != null)
            return _cachedConfig;
        
        if (File.Exists(ConfigFilePath))
        {
            string json = File.ReadAllText(ConfigFilePath);
            _cachedConfig = JsonUtility.FromJson<TCPConfiguration>(json);
            return _cachedConfig;
        }
        
        _cachedConfig = new TCPConfiguration();
        SaveConfig(_cachedConfig);
        return _cachedConfig;
    }
    
    public static void SaveConfig(TCPConfiguration config)
    {
        string json = JsonUtility.ToJson(config, true);
        File.WriteAllText(ConfigFilePath, json);
        _cachedConfig = config;
    }
    
    public static string GetConfigFilePath()
    {
        return ConfigFilePath;
    }
} 