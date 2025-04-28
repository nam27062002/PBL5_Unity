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

    // Variables to track letter and consecutive matches
    private string _previousLetter = string.Empty;
    private int _sameLetterCount = 0;
    private const int REQUIRED_CONSECUTIVE_MATCHES = 3;

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
        // Reset tracking variables when opening menu
        _previousLetter = string.Empty;
        _sameLetterCount = 0;
    }

    public override void Close()
    {
        base.Close();
        uiCamera.HideWebCam();
    }

    protected override void OnUpdate()
    {
        UpdateSendInterval();
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

    private void OnStringReceivedHandleLetterPrediction(KeyData _, byte[] data)
    {
        string letterStr = System.Text.Encoding.UTF8.GetString(data);
        if (letterStr.StartsWith("Predicted: "))
        {
            string[] parts = letterStr.Split(',');
            string letter = parts[0].Replace("Predicted: ", "").Trim();
            string confidenceStr = parts[1].Replace("Confidence: ", "").Trim();
            float confidence = float.Parse(confidenceStr) * 100f;

            // Check if the letter matches the previous one
            if (letter == _previousLetter)
            {
                _sameLetterCount++;

                // Only update UI when enough consecutive matches
                if (_sameLetterCount >= REQUIRED_CONSECUTIVE_MATCHES)
                {
                    UpdateLetterUI(letter, confidence);
                }
            }
            else
            {
                // New letter, reset counter
                _previousLetter = letter;
                _sameLetterCount = 1;

                // Hide previous letter when detecting a new one
                letterText.letterImage.enabled = false;
                letterText.unknownObject.SetActiveIfNeeded(true);
                letterText.letterText.SetText("Detecting...");
                letterText.confidenceText.SetText("");
            }
        }
        else
        {
            // Reset when no letter is recognized
            _previousLetter = string.Empty;
            _sameLetterCount = 0;

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