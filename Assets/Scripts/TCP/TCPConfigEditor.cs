#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class TCPConfigEditor : EditorWindow
{
    private TCPConfiguration config;
    private Vector2 scrollPosition;
    private bool isDirty = false;

    [MenuItem("Tools/TCP Configuration")]
    public static void ShowWindow()
    {
        GetWindow<TCPConfigEditor>("TCP Config");
    }

    private void OnEnable()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {
        config = TCPConfig.GetConfig();
    }

    private void OnGUI()
    {
        if (config == null)
        {
            LoadConfig();
            if (config == null)
            {
                EditorGUILayout.HelpBox("Failed to load configuration.", MessageType.Error);
                return;
            }
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.LabelField("TCP Server Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();

        config.serverIP = EditorGUILayout.TextField("Server IP", config.serverIP);
        config.serverPort = EditorGUILayout.IntField("Server Port", config.serverPort);

        if (EditorGUI.EndChangeCheck())
        {
            isDirty = true;
        }

        EditorGUILayout.Space();

        GUI.enabled = isDirty;
        if (GUILayout.Button("Save Configuration"))
        {
            TCPConfig.SaveConfig(config);
            isDirty = false;
            Debug.Log("TCP Configuration saved to: " + TCPConfig.GetConfigFilePath());
        }
        GUI.enabled = true;

        if (GUILayout.Button("Reset to Default"))
        {
            config = new TCPConfiguration();
            isDirty = true;
        }

        if (GUILayout.Button("Open Config File Location"))
        {
            string configPath = TCPConfig.GetConfigFilePath();
            string directoryPath = Path.GetDirectoryName(configPath);
            EditorUtility.RevealInFinder(directoryPath);
        }

        EditorGUILayout.EndScrollView();
    }
}
#endif 