using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OnBoardingPopup : PopupBase
{
    [Serializable]
    public class StepConfig
    {
        public string message;
        public UnityEvent onStart;
    }
    
    [Title("OnBoarding Popup"), Space]
    [SerializeField] private Button startButton;
    [SerializeField] private Button skipButton;

    [Title("Scripts"), Space]
    [SerializeField] private UI_Camera uiCamera;

    [Title("Config"), Space]
    [SerializeField] private TextMeshProUGUI message;
    [SerializeField] private List<StepConfig> stepConfigs;

    [Space] 
    [SerializeField] private GameObject loadingObject;
    [SerializeField] private Image loadingImage;
    [SerializeField] private float loadingTime = 3f;
    
    [Title("Optimization Settings")]
    [SerializeField] private float sendInterval = 0.1f;
    
    private float _timer;
    private int _stepIndex;
    private float timeSinceLastSend = 0f;
    private bool _hasHand;
    
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
    
    protected override void OnUpdate()
    {
        base.OnUpdate();
        if (_stepIndex == 1)
        {
            timeSinceLastSend += Time.unscaledDeltaTime;
            if (timeSinceLastSend >= sendInterval)
            {
                timeSinceLastSend = 0f;
                TCPClient.Instance.SendData(KeyData.HandRecognition, WebCamManager.Instance.ProcessingTexture);
            }
            if (_hasHand)
            {
                _timer += Time.unscaledDeltaTime; 
                loadingImage.fillAmount = _timer / loadingTime;
                if (_timer >= loadingTime)
                {
                    Debug.Log("NT - finish");
                }
            }
            else
            {
                _timer = 0;
                loadingImage.fillAmount = 0;
            }
        }
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
        stepConfig.onStart?.Invoke();
    }

    public void HandleHandRecognition()
    {
        TCPClient.Instance.OnStringReceived += OnStringReceived;
    }

    private void OnStringReceived(KeyData _, string hasHandStr)
    {
        _hasHand = Convert.ToBoolean(hasHandStr);
        loadingObject.gameObject.SetActiveIfNeeded(_hasHand);
    }
}
