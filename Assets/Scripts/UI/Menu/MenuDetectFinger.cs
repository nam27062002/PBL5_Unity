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
            var letterType = (LetterType)Enum.Parse(typeof(LetterType), letter);
            letterText.unknownObject.SetActiveIfNeeded(false);
            letterText.letterText.SetText($"Letter Signed: {letter}");
            letterText.confidenceText.SetText($"Confidence: {confidence:F2}%");
            letterText.confidenceText.color = confidence >= 80f ? Color.green : Color.red;
            letterText.letterImage.enabled = true;
            letterText.letterImage.sprite = ScriptableObjectManager.Instance.lettersConfig.Letters[letterType].sprite;
        }
        else
        {
            letterText.letterImage.enabled = false;
            letterText.letterText.SetText("Unknown");
            letterText.confidenceText.SetText("");
            letterText.unknownObject.SetActiveIfNeeded(true);
        }
    }
}