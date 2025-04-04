using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
public class GameMenu : MenuBase
{
    [Title("Game Menu"), Space]
    [SerializeField] private Button fingerSpellAZButton;
    [SerializeField] private Button detectFingerButton;

    protected override void OnRegisterEvents()
    {
        fingerSpellAZButton.onClick.AddListener(OnFingerSpellAZButtonClicked);
        detectFingerButton.onClick.AddListener(OnDetectFingerButtonClicked);
    }

    protected override void OnUnRegisterEvents()
    {
        fingerSpellAZButton.onClick.RemoveListener(OnFingerSpellAZButtonClicked);
        detectFingerButton.onClick.RemoveListener(OnDetectFingerButtonClicked);
    }

    private void OnFingerSpellAZButtonClicked()
    {
        Debug.Log("OnFingerSpellAZButtonClicked");
        UIManager.Instance.OpenMenu(MenuType.FingerSpellAZ, null);
    }

    private void OnDetectFingerButtonClicked()
    {
        Debug.Log("OnDetectFingerButtonClicked");
        UIManager.Instance.OpenMenu(MenuType.DetectFinger, null);
    }
}