using Sirenix.OdinInspector;
using UnityEngine;

public abstract class PopupBase : UIBase
{
    [Title("Popup Base"), Space]
    [SerializeField] private PopupType popupType;

    protected override string OnOpenMessage => $"{popupType}: OnOpen";
    protected override string OnCloseMessage => $"{popupType}: OnClose";
}