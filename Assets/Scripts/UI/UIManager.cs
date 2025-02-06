using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonMonoBehavior<UIManager>
{
    [SerializeField] private SerializableDictionary<PopupType, UIBase> allPopups = new();
    [SerializeField] private SerializableDictionary<MenuType, UIBase> allMenus = new();

    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField] private Image greyBackground;

    private UIBase _currentPopup;
    private UIBase _currentMenu;

    private void Start()
    {
        HideGreyBackground();
        HideAllMenus();
        HideAllPopups();
    }

    public void OpenPopup(PopupType popupType)
    {
        ClosePopup();
        _currentPopup = allPopups[popupType];
        _currentPopup.Open();
        menuCanvasGroup.interactable = false;
    }

    public void ClosePopup()
    {
        _currentPopup.Close();
        menuCanvasGroup.interactable = true;
        HideGreyBackground();
    }

    public void OpenMenu(MenuType menuType)
    {
        _currentMenu?.Close();
        _currentMenu = allMenus[menuType];
        _currentMenu.Open();
    }

    #region Sub

    private void HideAllPopups()
    {
        foreach (var popup in allPopups)
        {
            popup.Value.gameObject.SetActiveIfNeeded(false);
        }
    }

    private void HideAllMenus()
    {
        foreach (var menu in allMenus)
        {
            menu.Value.gameObject.SetActiveIfNeeded(false);
        }
    }

    public void ShowGreyBackground()
    {
        greyBackground.enabled = true;
    }

    private void HideGreyBackground()
    {
        greyBackground.enabled = false;
    }

    #endregion
}