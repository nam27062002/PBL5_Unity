#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class PlayerPrefsClearTool : UnityEditor.Editor
{
    [MenuItem("Internal Tools/Clear All PlayerPrefs")]
    public static void ShowWindow()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("All PlayerPrefs have been cleared.");
    }
}
#endif