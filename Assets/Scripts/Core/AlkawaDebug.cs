#if UNITY_EDITOR
#define USE_DEBUG
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

public enum ELogCategory
{
    NONE = 0,
    UI,
    UDP,
    LOADSAVE,
    ENGINE,
    AUDIO,
    GAMEPLAY,
}

public enum ELogSeverity
{
    INFO,
    WARNING,
    ERROR
}

public class AlkawaDebug
{
    private static readonly Dictionary<ELogCategory, string> CategoryColors = new Dictionary<ELogCategory, string>();
    private static readonly HashSet<ELogCategory> DisabledCategories = new HashSet<ELogCategory>();
    public static bool ShowDebugInfo { get; set; } = false;

    static AlkawaDebug()
    {
        CategoryColors[ELogCategory.UI] = "#2196F3";        
        CategoryColors[ELogCategory.UDP] = "#4CAF50";    
        CategoryColors[ELogCategory.LOADSAVE] = "#FF9800"; 
        CategoryColors[ELogCategory.ENGINE] = "#607D8B";    
        CategoryColors[ELogCategory.AUDIO] = "#E91E63";
        CategoryColors[ELogCategory.GAMEPLAY] = "#FFEB3B";
    }
    
    public static void SetCategoryEnabled(ELogCategory cat, bool enabled)
    {
        if (enabled)
        {
            DisabledCategories.Remove(cat);
        }
        else
        {
            DisabledCategories.Add(cat);
        }
    }

    [Conditional("USE_DEBUG")]
    public static void Log(ELogCategory cat, string msg, Object context = null,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        if (IsCategoryDisabled(cat)) return;

        if (ShowDebugInfo)
        {
            string fileName = System.IO.Path.GetFileName(file);
            msg = $"[{fileName}:{line} {caller}] {msg}";
        }

        InternalLog(cat, ELogSeverity.INFO, msg, context);
    }

    [Conditional("USE_DEBUG")]
    public static void LogWarning(ELogCategory cat, string msg, Object context = null,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        if (IsCategoryDisabled(cat)) return;

        if (ShowDebugInfo)
        {
            string fileName = System.IO.Path.GetFileName(file);
            msg = $"[{fileName}:{line} {caller}] {msg}";
        }

        InternalLog(cat, ELogSeverity.WARNING, msg, context);
    }

    [Conditional("USE_DEBUG")]
    public static void LogError(ELogCategory cat, string msg, Object context = null,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        if (IsCategoryDisabled(cat)) return;

        if (ShowDebugInfo)
        {
            string fileName = System.IO.Path.GetFileName(file);
            msg = $"[{fileName}:{line} {caller}] {msg}";
        }

        InternalLog(cat, ELogSeverity.ERROR, msg, context);
    }
    
    [Conditional("USE_DEBUG")]
    public static void LogException(ELogCategory cat, Exception exception, string msg = "", Object context = null,
        [CallerMemberName] string caller = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        if (IsCategoryDisabled(cat)) return;

        string fileName = System.IO.Path.GetFileName(file);
        string debugInfo = ShowDebugInfo ? $"[{fileName}:{line} {caller}] " : "";
        string fullMsg = $"{debugInfo}{msg}\nException: {exception}";
        InternalLog(cat, ELogSeverity.ERROR, fullMsg, context);
    }
    
    [Conditional("USE_DEBUG")]
    public static void LogFormat(ELogCategory cat, string format, Object context = null, params object[] args)
    {
        if (IsCategoryDisabled(cat)) return;

        string msg = string.Format(format, args);
        InternalLog(cat, ELogSeverity.INFO, msg, context);
    }
    
    [Conditional("USE_DEBUG")]
    private static void InternalLog(ELogCategory cat, ELogSeverity sev, string _msg, Object context)
    {
        string categoryPart = "";
        if (cat != ELogCategory.NONE)
        {
            string colorHex = CategoryColors.GetValueOrDefault(cat, "#FFFFFF");
            categoryPart = $"<color={colorHex}>[{cat}]</color> ";
        }
        // string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        // string msg = $"[{timestamp}] {categoryPart}{_msg}";
        string msg = $"{categoryPart}{_msg}";
        switch (sev)
        {
            case ELogSeverity.INFO:
                Debug.Log(msg, context);
                break;
            case ELogSeverity.WARNING:
                Debug.LogWarning(msg, context);
                break;
            case ELogSeverity.ERROR:
                Debug.LogError(msg, context);
                break;
        }
    }

    private static bool IsCategoryDisabled(ELogCategory cat)
    {
        return DisabledCategories.Contains(cat);
    }
}
