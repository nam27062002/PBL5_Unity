using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OnBoardingPopup : PopupBase
{
    private const int HAND_RECOGNITION_STEP = 1;
    private const int LETTER_PREDICTION_STEP = 4;
    private const float EDITOR_LOADING_TIME = 0.5f;

    [Serializable]
    public class StepConfig
    {
        public string message;
        public UnityEvent onStart;
    }

    [Title("OnBoarding Popup"), Space]
    [SerializeField] private Button startButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button readyButton;

    [Title("Scripts"), Space]
    [SerializeField] private UI_Camera uiCamera;
    [SerializeField] private UI_Letter uiLetter;

    [Title("Config"), Space]
    [SerializeField] private TextMeshProUGUI message;
    [SerializeField] private List<StepConfig> stepConfigs;

    [Space]
    [SerializeField] private GameObject loadingObject;
    [SerializeField] private Image loadingImage;
    [SerializeField] private float loadingTime = 3f;
    [SerializeField] private LetterType letterTypeTutorial;
    [SerializeField] private TextMeshProUGUI predictedLetter;
    [SerializeField] private TextMeshProUGUI confidenceText;

    [Title("Optimization Settings")]
    [SerializeField] private float sendInterval = 0.1f;

    private float _timer;
    private int _stepIndex;
    private float timeSinceLastSend = 0f;
    private bool _hasHand;
    private int _correctPredictionCount = 0;
    private const int REQUIRED_CORRECT_PREDICTIONS = 5;

    protected override void OnRegisterEvents()
    {
        base.OnRegisterEvents();
        startButton.onClick.AddListener(OnStartButtonClicked);
        readyButton.onClick.AddListener(OnReadyButtonClicked);
    }

    protected override void OnUnRegisterEvents()
    {
        base.OnUnRegisterEvents();
        startButton.onClick.RemoveListener(OnStartButtonClicked);
        readyButton.onClick.RemoveListener(OnReadyButtonClicked);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        HandleStepUpdate();
    }

    private void HandleStepUpdate()
    {
        switch (_stepIndex)
        {
            case HAND_RECOGNITION_STEP:
                HandleHandRecognitionStep();
                break;
            case LETTER_PREDICTION_STEP:
                HandleLetterPredictionStep();
                break;
        }
    }

    private void HandleHandRecognitionStep()
    {
        UpdateSendInterval();
        if (_hasHand)
        {
            UpdateLoadingTimer();
        }
        else
        {
            ResetLoadingTimer();
        }
    }

    private void HandleLetterPredictionStep()
    {
        UpdateSendInterval();
    }

    private void UpdateSendInterval()
    {
        timeSinceLastSend += Time.unscaledDeltaTime;
        if (timeSinceLastSend >= sendInterval)
        {
            timeSinceLastSend = 0f;
            SendData();
        }
    }

    private void SendData()
    {
        if (_stepIndex == HAND_RECOGNITION_STEP)
        {
            TCPClient.Instance.SendData(KeyData.HandRecognition, WebCamManager.Instance.ProcessingTexture);
        }
        else if (_stepIndex == LETTER_PREDICTION_STEP)
        {
            TCPClient.Instance.SendData(KeyData.LetterPrediction, WebCamManager.Instance.ProcessingTexture);
        }
    }

    private void UpdateLoadingTimer()
    {
        _timer += Time.unscaledDeltaTime;
        loadingImage.fillAmount = _timer / loadingTime;
        if (_timer >= loadingTime)
        {
            _stepIndex++;
            SetupUI();
        }
    }

    private void ResetLoadingTimer()
    {
        _timer = 0;
        loadingImage.fillAmount = 0;
    }

    public override void Open(IBaseEventParamsUI baseEventParamsUI)
    {
        base.Open(baseEventParamsUI);
        InitializeUI();
        SetupUI();
#if UNITY_EDITOR
        loadingTime = EDITOR_LOADING_TIME;
#endif
    }

    private void InitializeUI()
    {
        loadingObject.SetActiveIfNeeded(false);
        uiCamera.ShowWebCam();
        uiLetter.Hide();
        startButton.gameObject.SetActiveIfNeeded(true);
        readyButton.gameObject.SetActiveIfNeeded(false);
        predictedLetter.SetText("");
        confidenceText.SetText("");
        _stepIndex = 0;
    }

    private void OnStartButtonClicked()
    {
        startButton.gameObject.SetActiveIfNeeded(false);
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
        TCPClient.Instance.OnStringReceived -= OnStringReceivedHandleHandRecognition;
        TCPClient.Instance.OnStringReceived += OnStringReceivedHandleHandRecognition;
    }

    private void OnStringReceivedHandleHandRecognition(KeyData _, string hasHandStr)
    {
        _hasHand = Convert.ToBoolean(hasHandStr);
        loadingObject.SetActiveIfNeeded(_hasHand);
    }

    public void TryRememberSign()
    {
        TCPClient.Instance.OnStringReceived -= OnStringReceivedHandleHandRecognition;
        loadingObject.SetActiveIfNeeded(false);
        uiCamera.HideWebCam();
        uiLetter.Show();
        readyButton.gameObject.SetActiveIfNeeded(true);
        uiLetter.SetLetter(letterTypeTutorial, ScriptableObjectManager.Instance.lettersConfig.Letters[letterTypeTutorial]);
    }

    private void OnReadyButtonClicked()
    {
        _stepIndex++;
        SetupUI();
        readyButton.gameObject.SetActiveIfNeeded(false);
        uiLetter.Hide();
        uiCamera.ShowWebCam();
        startButton.gameObject.SetActiveIfNeeded(true);
    }

    public void HandleLetterPrediction()
    {
        TCPClient.Instance.OnStringReceived += OnStringReceivedHandleLetterPrediction;
    }

    private void OnStringReceivedHandleLetterPrediction(KeyData _, string letterStr)
    {
        Debug.Log("letterStr: " + letterStr);
        if (letterStr.StartsWith("Predicted: "))
        {
            string[] parts = letterStr.Split(',');
            string letter = parts[0].Replace("Predicted: ", "").Trim();
            string confidenceStr = parts[1].Replace("Confidence: ", "").Trim();
            float confidence = float.Parse(confidenceStr) * 100f;

            // Xử lý predictedLetter
            bool isCorrect = letter == letterTypeTutorial.ToString();
            Color letterColor = isCorrect ? Color.green : Color.red;
            predictedLetter.SetText($"Predict: {letter}");
            predictedLetter.color = letterColor;

            // Xử lý confidence
            Color confidenceColor = (!isCorrect || confidence < 70f) ? Color.red : Color.green;
            confidenceText.SetText($"Confidence: {confidence:F2}%");
            confidenceText.color = confidenceColor;

            if (isCorrect)
            {
                _correctPredictionCount++;
                if (_correctPredictionCount >= REQUIRED_CORRECT_PREDICTIONS)
                {
                    Debug.Log("Win");
                    _correctPredictionCount = 0;
                }
            }
            else
            {
                _correctPredictionCount = 0;
            }
        }
        else
        {
            _correctPredictionCount = 0;
            predictedLetter.SetText("Cannot process image");
            predictedLetter.color = Color.red;
            confidenceText.SetText("");
        }
    }
}
