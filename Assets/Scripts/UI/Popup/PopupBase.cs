using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public abstract class PopupBase : UIBase
{
    [Title("Popup Base"), Space]
    [SerializeField] private PopupType popupType;
    [SerializeField] private bool hasClosePopupButton;
    [SerializeField, ShowIf(nameof(hasClosePopupButton))] Button closeButton;

    protected virtual bool HasShowGreyBackground => true;
    protected virtual bool HasPauseGame => true;
    
    protected override string OnOpenMessage => $"{popupType}: OnOpen";
    protected override string OnCloseMessage => $"{popupType}: OnClose";

    protected override void OnRegisterEvents()
    {
        if (hasClosePopupButton) 
            closeButton.onClick.AddListener(ClosePopup);
    }

    protected override void OnUnRegisterEvents()
    {
        if (hasClosePopupButton) 
            closeButton.onClick.RemoveListener(ClosePopup);
    }

    public override void Open()
    {
        base.Open();
        if (HasShowGreyBackground) UIManager.ShowGreyBackground();
        if (HasPauseGame) Time.timeScale = 0;
    }
    
    protected virtual void ClosePopup()
    {
        UIManager.ClosePopup();
    }

    public override void Close()
    {
        base.Close();
        Time.timeScale = 1;
    }
}