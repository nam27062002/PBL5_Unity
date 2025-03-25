using Sirenix.OdinInspector;

public class ScriptableObjectManager : SingletonMonoBehavior<ScriptableObjectManager>
{
    [TabGroup("UI", "ConfirmPopup")] public ConfirmPopupEventParams allowUseCameraPopup;
    [TabGroup("UI", "ConfirmPopup")] public ConfirmPopupEventParams cameraNotDetectedPopup;       
    
    [TabGroup("Game Config", "Letter")] public LettersConfig lettersConfig;
}