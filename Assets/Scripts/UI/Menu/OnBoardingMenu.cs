using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class OnBoardingMenu : MenuBase
{
    [Title("On Boarding"), Space]
    [SerializeField] private Button startButton;
    
    protected override void OnRegisterEvents()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        base.OnRegisterEvents();
    }

    protected override void OnUnRegisterEvents()
    {
        startButton.onClick.RemoveListener(OnStartButtonClicked);
        base.OnUnRegisterEvents();
    }

    private void OnStartButtonClicked()
    {
        if (LoadSaveManager.Instance.OnBoardingFinished)
        {
            UIManager.OpenMenu(MenuType.Game, null);
        }
        else
        {
            if (!LoadSaveManager.Instance.AllowUseCamera)
            {
                UIManager.OpenPopup(PopupType.Confirm, ScriptableObjectManager.Instance.allowUseCameraPopup);
            }
            else
            {
                if (WebCamManager.HasWebCamDevice)
                {
                    UIManager.OpenPopup(PopupType.OnBoarding, null);
                }
                else
                {
                    UIManager.OpenPopup(PopupType.Confirm, ScriptableObjectManager.Instance.cameraNotDetectedPopup);   
                }
            }
        }
    }
}