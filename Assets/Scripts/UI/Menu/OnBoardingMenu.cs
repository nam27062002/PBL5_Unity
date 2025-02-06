using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class OnBoardingMenu : MenuBase
{
    [Title("On Boarding"), Space]
    [SerializeField] private Button startButton;

    protected override void OnRegisterEvents()
    {
        base.OnRegisterEvents();
        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    protected override void OnUnRegisterEvents()
    {
        base.OnUnRegisterEvents();
        startButton.onClick.RemoveListener(OnStartButtonClicked);
    }

    private void OnStartButtonClicked()
    {
        AlkawaDebug.Log(ELogCategory.UI, "OnStartButtonClicked");
    }
}