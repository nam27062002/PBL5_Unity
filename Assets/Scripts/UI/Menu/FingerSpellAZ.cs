using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using System;

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

    public override void Open(IBaseEventParamsUI baseEventParamsUI)
    {
        base.Open(baseEventParamsUI);
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
        letter.SetLetter(currentLetterType, lettersConfig.Letters[currentLetterType]);
        TCPClient.Instance.OnDataReceived += OnStringReceivedHandleLetterPrediction;
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
            if (letterType == currentLetterType)
            {
                Debug.Log("OKE NE");
            }
            letter.letterText.SetText($"Letter Signed: {letterPredicted}");
            letter.confidenceText.SetText($"Confidence: {confidence:F2}%");
            letter.confidenceText.color = confidence >= 80f ? Color.green : Color.red;
            letter.letterImage.enabled = true;
            letter.letterImage.sprite = ScriptableObjectManager.Instance.lettersConfig.Letters[letterType].sprite;
        }
        else
        {
            letter.letterImage.enabled = false;
            letter.letterText.SetText("Unknown");
            letter.confidenceText.SetText("");
        }
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
 