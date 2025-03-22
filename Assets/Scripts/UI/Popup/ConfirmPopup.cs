using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

    private UnityEvent _onConfirm;
    private UnityEvent _onCancel;
    
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
    
    public override void Open(IBaseEventParamsUI baseEventParamsUI)
    {
        base.Open(baseEventParamsUI);
        if (baseEventParamsUI is ConfirmPopupEventParams confirmPopupEventParams)
        {
            title.SetText(confirmPopupEventParams.title);
            description.SetText(confirmPopupEventParams.description);
            confirmText.SetText(confirmPopupEventParams.confirmText);
            cancelText.SetText(confirmPopupEventParams.cancelText);
            _onConfirm = confirmPopupEventParams.onConfirm;
            _onCancel = confirmPopupEventParams.onCancel;
        }
    }

    private void OnConfirmButtonClick()
    {
        _onConfirm?.Invoke();
        ClosePopup();
    }

    private void OnCancelButtonClick()
    {
        _onCancel?.Invoke();
        ClosePopup();
    }

    protected override void ClosePopup()
    {
        base.ClosePopup();
        _onConfirm = null;
        _onCancel = null;
    }
}