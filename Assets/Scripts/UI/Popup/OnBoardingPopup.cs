using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnBoardingPopup : PopupBase
{

    [Serializable]
    public class StepConfig
    {
        public string message;
    }


    [Title("OnBoarding Popup"), Space]
    [SerializeField] private Button startButton;
    [SerializeField] private Button skipButton;

    [Title("Scripts")]
    [SerializeField] private UI_Camera uiCamera;

    [Title("Config")]
    [SerializeField] private TextMeshProUGUI message;
    [SerializeField] private List<StepConfig> stepConfigs;
    private int _stepIndex;

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

    public override void Open(IBaseEventParamsUI baseEventParamsUI)
    {
        base.Open(baseEventParamsUI);
        uiCamera.HideWebCam();
        startButton.gameObject.SetActiveIfNeeded(true);
        _stepIndex = 0;
        SetupUI();
    }

    private void OnStartButtonClicked()
    {
        startButton.gameObject.SetActiveIfNeeded(false);
        uiCamera.ShowWebCam();
        _stepIndex++;
        SetupUI();
    }

    private void SetupUI()
    {
        if (_stepIndex >= stepConfigs.Count) return;
        var stepConfig = stepConfigs[_stepIndex];
        message.SetText(stepConfig.message);
    }
}
