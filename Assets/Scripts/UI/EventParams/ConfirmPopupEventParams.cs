using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "EventParamsUIConfirmPopup", menuName = "SO/IBaseEventParamsUI/EventParamsUIConfirmPopup")]
public class ConfirmPopupEventParams : ScriptableObject, IBaseEventParamsUI
{
    public string title;
    public string description;
    public string confirmText;
    public string cancelText;
    public UnityEvent onConfirm;
    public UnityEvent onCancel;
}