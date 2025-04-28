using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using System;
public class MenuDetectFinger : MenuBase
{
    [Title("Detect Finger"), Space]
    [SerializeField] private Button backButton;
    [SerializeField] private UI_Camera uiCamera;
    [SerializeField] private UI_Letter letterText;
    [SerializeField] private float sendInterval = 0.1f;
    private float _timeSinceLastSend = 0f;

    // Replace consecutive matches with time-based detection
    private string _currentLetter = string.Empty;
    private float _letterDetectionTimer = 0f;
    private const float LETTER_CONFIRMATION_TIME = 0.5f; // 0.5 seconds
    private bool _isConfirmingLetter = false;

    protected override void OnRegisterEvents()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
        TCPClient.Instance.OnDataReceived += OnStringReceivedHandleLetterPrediction;
    }

    protected override void OnUnRegisterEvents()
    {
        backButton.onClick.RemoveListener(OnBackButtonClicked);
        if (TCPClient.HasInstance)
            TCPClient.Instance.OnDataReceived -= OnStringReceivedHandleLetterPrediction;
    }

    private void OnBackButtonClicked()
    {
        UIManager.Instance.OpenMenu(MenuType.Game, null);
    }

    public override void Open(IBaseEventParamsUI baseEventParamsUI)
    {
        base.Open(baseEventParamsUI);
        uiCamera.ShowWebCam();
        // Reset variables
        _currentLetter = string.Empty;
        _letterDetectionTimer = 0f;
        _isConfirmingLetter = false;
    }

    public override void Close()
    {
        base.Close();
        uiCamera.HideWebCam();
    }

    protected override void OnUpdate()
    {
        UpdateSendInterval();

        // Update letter confirmation timer
        if (_isConfirmingLetter)
        {
            _letterDetectionTimer += Time.deltaTime;

            // If time threshold reached, confirm the letter
            if (_letterDetectionTimer >= LETTER_CONFIRMATION_TIME)
            {
                _isConfirmingLetter = false;
                UpdateLetterUI(_currentLetter, _currentConfidence);
            }
        }
    }

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
        TCPClient.Instance.SendData(KeyData.LetterPrediction, WebCamManager.Instance.ProcessingTexture);
    }

    // Store current confidence for the UI update
    private float _currentConfidence = 0f;

    private void OnStringReceivedHandleLetterPrediction(KeyData _, byte[] data)
    {
        string letterStr = System.Text.Encoding.UTF8.GetString(data);
        if (letterStr.StartsWith("Predicted: "))
        {
            string[] parts = letterStr.Split(',');
            string letter = parts[0].Replace("Predicted: ", "").Trim();
            string confidenceStr = parts[1].Replace("Confidence: ", "").Trim();
            float confidence = float.Parse(confidenceStr) * 100f;

            // New letter detected
            if (letter != _currentLetter)
            {
                _currentLetter = letter;
                _currentConfidence = confidence;
                _letterDetectionTimer = 0f;
                _isConfirmingLetter = true;

                // Show detecting state
                letterText.letterImage.enabled = false;
                letterText.unknownObject.SetActiveIfNeeded(true);
                letterText.letterText.SetText("Detecting...");
                letterText.confidenceText.SetText("");
            }
            else
            {
                // Update confidence for the same letter
                _currentConfidence = confidence;
            }
        }
        else
        {
            // Reset when no letter is recognized
            _currentLetter = string.Empty;
            _isConfirmingLetter = false;

            letterText.letterImage.enabled = false;
            letterText.letterText.SetText("Unknown");
            letterText.confidenceText.SetText("");
            letterText.unknownObject.SetActiveIfNeeded(true);
        }
    }

    // Separate method for updating the UI
    private void UpdateLetterUI(string letter, float confidence)
    {
        var letterType = (LetterType)Enum.Parse(typeof(LetterType), letter);
        letterText.unknownObject.SetActiveIfNeeded(false);
        letterText.letterText.SetText($"Letter Signed: {letter}");
        letterText.confidenceText.SetText($"Confidence: {confidence:F2}%");
        letterText.confidenceText.color = confidence >= 80f ? Color.green : Color.red;
        letterText.letterImage.enabled = true;
        letterText.letterImage.sprite = ScriptableObjectManager.Instance.lettersConfig.Letters[letterType].sprite;
    }
}