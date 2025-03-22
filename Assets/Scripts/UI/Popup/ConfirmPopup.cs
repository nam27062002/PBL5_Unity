using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPopup : PopupBase
{
    [Title("Confirm Popup"), Space]
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private TextMeshProUGUI confirmText;
    [SerializeField] private TextMeshProUGUI cancelText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    
    protected override void OnRegisterEvents()
    {
        base.OnRegisterEvents();
        confirmButton.onClick.AddListener(OnConfirmButtonClick);
        cancelButton.onClick.AddListener(OnCancelButtonClick);
    }

    protected override void OnUnRegisterEvents()
    {
        base.OnUnRegisterEvents();
        confirmButton.onClick.RemoveListener(OnConfirmButtonClick);
        cancelButton.onClick.RemoveListener(OnCancelButtonClick);
    }

    private void OnConfirmButtonClick()
    {
        
    }

    private void OnCancelButtonClick()
    {
        
    }
}