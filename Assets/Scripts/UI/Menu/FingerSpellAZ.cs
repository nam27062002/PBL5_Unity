using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class FingerSpellAZ : MenuBase
{
    [Title("Detect Finger"), Space]
    [SerializeField] private Button backButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private UI_Letter letter;
    [SerializeField] private List<LetterType> letterTypes;

    [SerializeField] private LetterType currentLetterType;

    [SerializeField] private LettersConfig lettersConfig;
    [SerializeField] private float sendInterval = 0.1f;
    [SerializeField] private GameObject correctGameObject;
    private float _timeSinceLastSend = 0f;
    private int _correctPredictionCount = 0;
    private const int REQUIRED_CORRECT_PREDICTIONS = 5;

    public override void Open(IBaseEventParamsUI baseEventParamsUI)
    {
        base.Open(baseEventParamsUI);
        _correctPredictionCount = 0;
        SetupUI();
    }

    protected override void OnRegisterEvents()
    {
        backButton.onClick.AddListener(OnBackButtonClicked);
        nextButton.onClick.AddListener(OnNextButtonClicked);
        prevButton.onClick.AddListener(OnPrevButtonClicked);
    }

    protected override void OnUnRegisterEvents()
    {
        backButton.onClick.RemoveListener(OnBackButtonClicked);
        nextButton.onClick.RemoveListener(OnNextButtonClicked);
        prevButton.onClick.RemoveListener(OnPrevButtonClicked);
    }

    private void OnBackButtonClicked()
    {
        UIManager.Instance.OpenMenu(MenuType.Game, null);
    }
    
    [Button("On Next Button Clicked")]
    private void OnNextButtonClicked()
    {
        if (currentLetterType == LetterType.Z)
            currentLetterType = LetterType.A;
        else
            currentLetterType++;
        
        SetupUI();
    }
    
    [Button("On Prev Button Clicked")]
    private void OnPrevButtonClicked()
    {
        if (currentLetterType == LetterType.A)
            currentLetterType = LetterType.Z;
        else
            currentLetterType--;
        
        SetupUI();
    }
    
    [Button("Setup UI")]
    private void SetupUI()
    {
        correctGameObject.SetActiveIfNeeded(false);
        letter.SetLetter(currentLetterType, lettersConfig.Letters[currentLetterType]);
        TCPClient.Instance.OnDataReceived += OnStringReceivedHandleLetterPrediction;
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
        if (TCPClient.Instance.HasReceiverRegistered())
        {
            TCPClient.Instance.SendData(KeyData.LetterPrediction, WebCamManager.Instance.ProcessingTexture);
        }
    }

    private void OnStringReceivedHandleLetterPrediction(KeyData _, byte[] data)
    {
        string letterStr = System.Text.Encoding.UTF8.GetString(data);
        if (letterStr.StartsWith("Predicted: "))
        {
            string[] parts = letterStr.Split(',');
            string letterPredicted = parts[0].Replace("Predicted: ", "").Trim();
            string confidenceStr = parts[1].Replace("Confidence: ", "").Trim();
            float confidence = float.Parse(confidenceStr) * 100f;
            var letterType = (LetterType)Enum.Parse(typeof(LetterType), letterPredicted);
            
            bool isCorrect = letterType == currentLetterType;
            
            letter.predictedText.SetText($"Letter Signed: {letterPredicted}");
            letter.predictedText.color = isCorrect ? Color.green : Color.red;
            
            letter.confidenceText.SetText($"Confidence: {confidence:F2}%");
            letter.confidenceText.color = (isCorrect && confidence > 80f) ? Color.green : Color.red;

            if (isCorrect && confidence > 80f)
            {
                _correctPredictionCount++;
                if (_correctPredictionCount >= REQUIRED_CORRECT_PREDICTIONS)
                {
                    HandleCorrectLetter();
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
            letter.predictedText.SetText("Unknown");
            letter.predictedText.color = Color.red;
            letter.confidenceText.SetText("");
            _correctPredictionCount = 0;
        }
    }

    private void HandleCorrectLetter()
    {
        TCPClient.Instance.OnDataReceived -= OnStringReceivedHandleLetterPrediction;
        correctGameObject.SetActiveIfNeeded(true);
        StartCoroutine(ShowCorrectAndNext());
    }

    private System.Collections.IEnumerator ShowCorrectAndNext()
    {
        yield return new WaitForSecondsRealtime(2f);
        correctGameObject.SetActiveIfNeeded(false);
        OnNextButtonClicked();
    }

    [Button("Set Letters Type")]
    private void SetLettersType()
    {
        letterTypes.Clear();
        for (int i = (int)LetterType.A; i <= (int)LetterType.Z; i++)
        {
            letterTypes.Add((LetterType)i);
        }
    }
}
 