using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "EventParamsUIConfirmPopup", menuName = "SO/IBaseEventParamsUI/EventParamsUIConfirmPopup")]
public class ConfirmPopupEventParams : ScriptableObject, IBaseEventParamsUI
{
    public ConfirmPopupType confirmPopupType;
    public string title;
    public string description;
    [ShowIf("@confirmPopupType == ConfirmPopupType.YesNo")] public string confirmText;
    [ShowIf("@confirmPopupType == ConfirmPopupType.YesNo")] public string cancelText;
    [ShowIf("@confirmPopupType == ConfirmPopupType.YesNo")] public UnityEvent onConfirm;
    [ShowIf("@confirmPopupType == ConfirmPopupType.YesNo")] public UnityEvent onCancel;
    [ShowIf("@confirmPopupType == ConfirmPopupType.YesNo")] public UnityEvent onConfirmAndClosed;
    [ShowIf("@confirmPopupType == ConfirmPopupType.YesNo")] public UnityEvent onCancelAndClosed;

    [ShowIf("@confirmPopupType == ConfirmPopupType.Okay")] public string okText;
    [ShowIf("@confirmPopupType == ConfirmPopupType.Okay")] public UnityEvent onOk;
    [ShowIf("@confirmPopupType == ConfirmPopupType.Okay")] public UnityEvent onOkAndClosed;
}

[Serializable]
public enum ConfirmPopupType
{
    None = 0,
    YesNo = 1,
    Okay = 2,
}