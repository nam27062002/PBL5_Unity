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

    public void OpenPopup(PopupType popupType, IBaseEventParamsUI baseEventParamsUI)
    {
        ClosePopup();
        _currentPopup = allPopups[popupType];
        _currentPopup.Open(baseEventParamsUI);
        menuCanvasGroup.interactable = false;
    }

    public void ClosePopup()
    {
        _currentPopup?.Close();
        menuCanvasGroup.interactable = true;
        HideGreyBackground();
    }

    public void OpenMenu(MenuType menuType, IBaseEventParamsUI baseEventParamsUI)
    {
        if (_currentMenu != null)
        {
            _currentMenu.Close();

            // Lưu menu cần mở sau khi loading xong
            MenuType targetMenuType = menuType;
            IBaseEventParamsUI targetParams = baseEventParamsUI;

            // Tạo tham số cho loading menu
            var loadingParams = new LoadingMenuEventParams { loadingTime = 1f };

            // Đăng ký callback khi load xong
            GameManager.Instance.OnLoadComplete = () =>
            {
                // Mở menu đích sau khi loading hoàn tất
                _currentMenu = allMenus[targetMenuType];
                _currentMenu.Open(targetParams);
            };

            // Mở loading menu
            _currentMenu = allMenus[MenuType.Loading];
            _currentMenu.Open(loadingParams);
        }
        else
        {
            _currentMenu = allMenus[menuType];
            _currentMenu.Open(baseEventParamsUI);
        }
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