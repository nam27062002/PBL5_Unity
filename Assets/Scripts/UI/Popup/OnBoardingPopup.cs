using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OnBoardingPopup : PopupBase
{
    #region Constants

    private const int HAND_RECOGNITION_STEP = 1;
    private const int LETTER_PREDICTION_STEP = 4;
    private const float EDITOR_LOADING_TIME = 0.5f;
    private const int REQUIRED_CORRECT_PREDICTIONS = 5;

    #endregion

    #region Classes

    [Serializable]
    public class StepConfig
    {
        public string message;
        public UnityEvent onStart;
    }

    #endregion

    #region SerializeFields

    [Title("OnBoarding Popup"), Space]
    [SerializeField] private Button startButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button tryAgainButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button playButton;

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

    [Title("Final Step")]
    [SerializeField] private GameObject cameraFrameObject;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject readyToPlayObject;

    [SerializeField] private Image sampleImage;
    [SerializeField] private TextMeshProUGUI labelText;

    [SerializeField] private Image predictionImage;
    [SerializeField] private TextMeshProUGUI predictedText;
    [SerializeField] private TextMeshProUGUI confidence2Text;

    #endregion

    #region Private Fields

    private float _timer;
    private int _stepIndex;
    private float _timeSinceLastSend = 0f;
    private bool _hasHand;
    private int _correctPredictionCount = 0;
    private float _highestConfidence = 0f;
    private Texture2D _bestTexture;

    #endregion

    #region Lifecycle Methods

    protected override void OnRegisterEvents()
    {
        base.OnRegisterEvents();
        startButton.onClick.AddListener(OnStartButtonClicked);
        readyButton.onClick.AddListener(OnReadyButtonClicked);
        tryAgainButton.onClick.AddListener(OnTryAgainButtonClicked);
        nextButton.onClick.AddListener(OnNextButtonClicked);
        playButton.onClick.AddListener(OnPlayButtonClicked);
    }

    protected override void OnUnRegisterEvents()
    {
        base.OnUnRegisterEvents();
        startButton.onClick.RemoveListener(OnStartButtonClicked);
        readyButton.onClick.RemoveListener(OnReadyButtonClicked);
        tryAgainButton.onClick.RemoveListener(OnTryAgainButtonClicked);
        nextButton.onClick.RemoveListener(OnNextButtonClicked);
        playButton.onClick.RemoveListener(OnPlayButtonClicked);
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        HandleStepUpdate();
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

    #endregion

    #region Initialization

    private void InitializeUI()
    {
        loadingObject.SetActiveIfNeeded(false);
        uiCamera.ShowWebCam();
        uiLetter.Hide();
        startButton.gameObject.SetActiveIfNeeded(true);
        readyButton.gameObject.SetActiveIfNeeded(false);
        resultPanel.SetActiveIfNeeded(false);
        tryAgainButton.gameObject.SetActiveIfNeeded(false);
        nextButton.gameObject.SetActiveIfNeeded(false);
        predictedLetter.SetText("");
        confidenceText.SetText("");
        _stepIndex = 0;
    }

    private void SetupUI()
    {
        if (_stepIndex >= stepConfigs.Count) return;
        var stepConfig = stepConfigs[_stepIndex];
        message.SetText(stepConfig.message);
        stepConfig.onStart?.Invoke();
    }

    #endregion

    #region Step Handling

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

    #endregion

    #region TCP Communication

    private void UpdateSendInterval()
    {
        _timeSinceLastSend += Time.unscaledDeltaTime;
        if (_timeSinceLastSend >= sendInterval)
        {
            _timeSinceLastSend = 0f;
            SendData();
        }
    }

    private void SendData()
    {
        if (_stepIndex == HAND_RECOGNITION_STEP)
        {
            TCPClient.Instance.SendData(
                KeyData.HandRecognition,
                WebCamManager.Instance.ProcessingTexture
            );
        }
        else if (_stepIndex == LETTER_PREDICTION_STEP)
        {
            TCPClient.Instance.SendData(
                KeyData.LetterPrediction,
                WebCamManager.Instance.ProcessingTexture
            );
        }
    }

    public void HandleHandRecognition()
    {
        TCPClient.Instance.OnDataReceived -= OnStringReceivedHandleHandRecognition;
        TCPClient.Instance.OnDataReceived += OnStringReceivedHandleHandRecognition;
    }

    private void OnStringReceivedHandleHandRecognition(KeyData _, byte[] data)
    {
        string hasHandStr = System.Text.Encoding.UTF8.GetString(data);
        _hasHand = Convert.ToBoolean(hasHandStr);
        loadingObject.SetActiveIfNeeded(_hasHand);
    }

    public void HandleLetterPrediction()
    {
        TCPClient.Instance.OnDataReceived += OnStringReceivedHandleLetterPrediction;
    }

    private void OnStringReceivedHandleLetterPrediction(KeyData _, byte[] data)
    {
        string letterStr = System.Text.Encoding.UTF8.GetString(data);
        if (letterStr.StartsWith("Predicted: "))
        {
            string[] parts = letterStr.Split(',');
            string letter = parts[0].Replace("Predicted: ", "").Trim();
            string confidenceStr = parts[1].Replace("Confidence: ", "").Trim();
            float confidence = float.Parse(confidenceStr) * 100f;

            bool isCorrect = letter == letterTypeTutorial.ToString();
            Color letterColor = isCorrect ? Color.green : Color.red;
            predictedLetter.SetText($"Predict: {letter}");
            predictedLetter.color = letterColor;

            Color confidenceColor = confidence >= 80f ? Color.green : Color.red;
            confidenceText.SetText($"Confidence: {confidence:F2}%");
            confidenceText.color = confidenceColor;

            if (isCorrect)
            {
                if (confidence > _highestConfidence)
                {
                    _highestConfidence = confidence;
                    _bestTexture = WebCamManager.Instance.ProcessingTexture.CloneTexture();
                }

                _correctPredictionCount++;
                if (_correctPredictionCount >= REQUIRED_CORRECT_PREDICTIONS)
                {
                    _correctPredictionCount = 0;
                    _stepIndex++;
                    TCPClient.Instance.OnDataReceived -= OnStringReceivedHandleLetterPrediction;
                    SetupUI();
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
            predictedLetter.SetText("Unknown");
            predictedLetter.color = Color.red;
            confidenceText.SetText("");
        }
    }

    private void OnProcessedImageReceived(KeyData keyData, byte[] imageBytes)
    {
        if (keyData != KeyData.RawImageProcessing) return;

        try
        {
            Texture2D processedTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            bool isLoaded = processedTexture.LoadImage(imageBytes);

            if (isLoaded && processedTexture.width > 2 && processedTexture.height > 2)
            {
                Sprite newSprite = Sprite.Create(
                    processedTexture,
                    new Rect(0, 0, processedTexture.width, processedTexture.height),
                    new Vector2(0.5f, 0.5f)
                );
                predictionImage.sprite = newSprite;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing image: {e.Message}");
        }
    }

    #endregion

    #region UI Utilities

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

    private void ClearAllText()
    {
        if (message != null)
            message.SetText("");

        if (predictedLetter != null)
            predictedLetter.SetText("");

        if (confidenceText != null)
            confidenceText.SetText("");

        if (labelText != null)
            labelText.SetText("");

        if (predictedText != null)
            predictedText.SetText("");

        if (confidence2Text != null)
            confidence2Text.SetText("");

        _highestConfidence = 0f;
        _correctPredictionCount = 0;
    }

    private void HideAllObjects()
    {
        if (startButton != null)
            startButton.gameObject.SetActiveIfNeeded(false);

        if (loadingObject != null)
            loadingObject.SetActiveIfNeeded(false);

        if (uiCamera != null)
            uiCamera.HideWebCam();

        if (uiLetter != null)
            uiLetter.Hide();

        if (resultPanel != null)
            resultPanel.SetActiveIfNeeded(false);

        if (readyButton != null)
            readyButton.gameObject.SetActiveIfNeeded(false);

        if (tryAgainButton != null)
            tryAgainButton.gameObject.SetActiveIfNeeded(false);

        if (nextButton != null)
            nextButton.gameObject.SetActiveIfNeeded(false);
    }

    #endregion

    #region Button Event Handlers

    private void OnStartButtonClicked()
    {
        startButton.gameObject.SetActiveIfNeeded(false);
        _stepIndex++;
        SetupUI();
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

    private void OnTryAgainButtonClicked()
    {
        _stepIndex -= 2;
        tryAgainButton.gameObject.SetActiveIfNeeded(false);
        nextButton.gameObject.SetActiveIfNeeded(false);
        cameraFrameObject.SetActiveIfNeeded(true);
        resultPanel.SetActiveIfNeeded(false);

        ClearAllText();

        _bestTexture = null;

        uiCamera.HideWebCam();
        uiLetter.Show();
        readyButton.gameObject.SetActiveIfNeeded(true);
        uiLetter.SetLetter(letterTypeTutorial, ScriptableObjectManager.Instance.lettersConfig.Letters[letterTypeTutorial]);

        SetupUI();
    }

    private void OnNextButtonClicked()
    {
        tryAgainButton.gameObject.SetActiveIfNeeded(false);
        nextButton.gameObject.SetActiveIfNeeded(false);
        cameraFrameObject.SetActiveIfNeeded(false);
        resultPanel.SetActiveIfNeeded(false);
        readyToPlayObject.SetActiveIfNeeded(true);
        message.SetText("");
    }

    private void OnPlayButtonClicked()
    {
        LoadSaveManager.Instance.OnBoardingFinished = true;
        ClosePopup();
        UIManager.Instance.OpenMenu(MenuType.Game, null);
    }

    #endregion

    #region Step Actions

    public void TryRememberSign()
    {
        TCPClient.Instance.OnDataReceived -= OnStringReceivedHandleHandRecognition;
        loadingObject.SetActiveIfNeeded(false);
        uiCamera.HideWebCam();
        uiLetter.Show();
        readyButton.gameObject.SetActiveIfNeeded(true);
        uiLetter.SetLetter(letterTypeTutorial, ScriptableObjectManager.Instance.lettersConfig.Letters[letterTypeTutorial]);
    }

    public void ShowResult()
    {
        cameraFrameObject.SetActiveIfNeeded(false);
        resultPanel.SetActiveIfNeeded(true);
        tryAgainButton.gameObject.SetActiveIfNeeded(true);
        nextButton.gameObject.SetActiveIfNeeded(true);
        var letterData = ScriptableObjectManager.Instance.lettersConfig.Letters[letterTypeTutorial];
        sampleImage.sprite = letterData.sprite;
        labelText.SetText(letterTypeTutorial.ToString());

        if (_bestTexture != null)
        {
            predictionImage.sprite = Sprite.Create(_bestTexture, new Rect(0, 0, _bestTexture.width, _bestTexture.height), Vector2.one * 0.5f);

            TCPClient.Instance.OnDataReceived += OnProcessedImageReceived;
            TCPClient.Instance.SendData(KeyData.RawImageProcessing, _bestTexture);
        }

        predictedText.SetText($"Predict: {letterTypeTutorial.ToString()}");
        predictedText.color = Color.green;
        confidence2Text.SetText($"Confidence: {_highestConfidence:F2}%");
        confidence2Text.color = Color.green;
    }

    #endregion
}
