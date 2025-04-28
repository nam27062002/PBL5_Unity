using Sirenix.OdinInspector;
using System.Collections;
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
    [SerializeField] private Button okButton;

    private UnityEvent _onConfirm;
    private UnityEvent _onCancel;
    private UnityEvent _onOk;

    // Thêm các event mới
    private UnityEvent _onConfirmAndClosed;
    private UnityEvent _onCancelAndClosed;
    private UnityEvent _onOkAndClosed;

    // Theo dõi nút nào đã được nhấn
    private ButtonType _clickedButton = ButtonType.None;

    // Độ trễ trước khi gọi event sau đóng popup
    private const float DELAY_AFTER_CLOSE = 1.0f;

    private enum ButtonType
    {
        None,
        Confirm,
        Cancel,
        Ok
    }

    protected override void OnRegisterEvents()
    {
        base.OnRegisterEvents();
        confirmButton.onClick.AddListener(OnConfirmButtonClick);
        cancelButton.onClick.AddListener(OnCancelButtonClick);
        okButton.onClick.AddListener(OnOkButtonClick);
    }

    protected override void OnUnRegisterEvents()
    {
        base.OnUnRegisterEvents();
        confirmButton.onClick.RemoveListener(OnConfirmButtonClick);
        cancelButton.onClick.RemoveListener(OnCancelButtonClick);
        okButton.onClick.RemoveListener(OnOkButtonClick);
    }

    public override void Open(IBaseEventParamsUI baseEventParamsUI)
    {
        base.Open(baseEventParamsUI);
        _clickedButton = ButtonType.None;

        if (baseEventParamsUI is ConfirmPopupEventParams confirmPopupEventParams)
        {
            title.SetText(confirmPopupEventParams.title);
            description.SetText(confirmPopupEventParams.description);

            if (confirmPopupEventParams.confirmPopupType == ConfirmPopupType.YesNo)
            {
                confirmText.SetText(confirmPopupEventParams.confirmText);
                cancelText.SetText(confirmPopupEventParams.cancelText);
                _onConfirm = confirmPopupEventParams.onConfirm;
                _onCancel = confirmPopupEventParams.onCancel;
                _onConfirmAndClosed = confirmPopupEventParams.onConfirmAndClosed;
                _onCancelAndClosed = confirmPopupEventParams.onCancelAndClosed;
            }
            else if (confirmPopupEventParams.confirmPopupType == ConfirmPopupType.Okay)
            {
                okButton.GetComponentInChildren<TextMeshProUGUI>().SetText(confirmPopupEventParams.okText);
                _onOk = confirmPopupEventParams.onOk;
                _onOkAndClosed = confirmPopupEventParams.onOkAndClosed;
            }

            confirmButton.gameObject.SetActiveIfNeeded(confirmPopupEventParams.confirmPopupType == ConfirmPopupType.YesNo);
            cancelButton.gameObject.SetActiveIfNeeded(confirmPopupEventParams.confirmPopupType == ConfirmPopupType.YesNo);
            okButton.gameObject.SetActiveIfNeeded(confirmPopupEventParams.confirmPopupType == ConfirmPopupType.Okay);
        }
    }

    private void OnConfirmButtonClick()
    {
        _clickedButton = ButtonType.Confirm;
        _onConfirm?.Invoke();
        ClosePopup();
    }

    private void OnCancelButtonClick()
    {
        _clickedButton = ButtonType.Cancel;
        _onCancel?.Invoke();
        ClosePopup();
    }

    private void OnOkButtonClick()
    {
        _clickedButton = ButtonType.Ok;
        _onOk?.Invoke();
        ClosePopup();
    }

    protected override void ClosePopup()
    {
        // Lưu lại button đã click và các event trước khi đóng popup
        ButtonType clickedButton = _clickedButton;
        UnityEvent onConfirmAndClosed = _onConfirmAndClosed;
        UnityEvent onCancelAndClosed = _onCancelAndClosed;
        UnityEvent onOkAndClosed = _onOkAndClosed;

        // Đóng popup bằng cách gọi base
        base.ClosePopup();

        // Reset tất cả các event
        _onConfirm = null;
        _onCancel = null;
        _onOk = null;
        _onConfirmAndClosed = null;
        _onCancelAndClosed = null;
        _onOkAndClosed = null;
        _clickedButton = ButtonType.None;

        // Khởi động coroutine để gọi event sau khi popup đã đóng, với độ trễ
        CoroutineDispatcher.RunCoroutine(InvokeClosedEventsAfterDelay(clickedButton, onConfirmAndClosed, onCancelAndClosed, onOkAndClosed));
    }

    private IEnumerator InvokeClosedEventsAfterDelay(ButtonType clickedButton,
        UnityEvent onConfirmAndClosed, UnityEvent onCancelAndClosed, UnityEvent onOkAndClosed)
    {
        // Chờ 1 giây sau khi popup đã đóng
        yield return new WaitForSeconds(DELAY_AFTER_CLOSE);

        // Gọi sự kiện tương ứng với nút đã được nhấn
        switch (clickedButton)
        {
            case ButtonType.Confirm:
                onConfirmAndClosed?.Invoke();
                break;
            case ButtonType.Cancel:
                onCancelAndClosed?.Invoke();
                break;
            case ButtonType.Ok:
                onOkAndClosed?.Invoke();
                break;
        }
    }
}