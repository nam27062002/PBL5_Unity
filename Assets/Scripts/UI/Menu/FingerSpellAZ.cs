using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

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
 