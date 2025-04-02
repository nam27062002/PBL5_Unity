using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Letter : MonoBehaviour
{
    private const string NULL_LETTER_WARNING = "Letter is null";
    private const string NULL_SPRITE_WARNING = "Letter sprite is null";
    private const string NULL_IMAGE_WARNING = "Letter image component is null";
    private const string NULL_TEXT_WARNING = "Letter text component is null";
    private const string NULL_CONFIG_ERROR = "Letters config is not assigned";
    private const string NULL_DICT_ERROR = "Letters dictionary is null";
    private const string LETTER_NOT_FOUND_WARNING = "Letter type {0} not found in config";

    public Image letterImage;
    public TextMeshProUGUI letterText;
    public TextMeshProUGUI confidenceText;
    public GameObject unknownObject;
    public LetterType letterType;
    public LettersConfig lettersConfig;

    public void SetLetter(LetterType letterType, Letter letter)
    {
        if (!ValidateComponents()) return;
        if (!ValidateLetter(letter)) return;

        UpdateLetterUI(letterType, letter);
    }

    private bool ValidateComponents()
    {
        if (letterImage == null)
        {
            Debug.LogWarning(NULL_IMAGE_WARNING);
            return false;
        }
        if (letterText == null)
        {
            Debug.LogWarning(NULL_TEXT_WARNING);
            return false;
        }
        return true;
    }

    private bool ValidateLetter(Letter letter)
    {
        if (letter == null)
        {
            Debug.LogWarning(NULL_LETTER_WARNING);
            return false;
        }
        if (letter.sprite == null)
        {
            Debug.LogWarning(NULL_SPRITE_WARNING);
            return false;
        }
        return true;
    }

    private void UpdateLetterUI(LetterType letterType, Letter letter)
    {
        letterImage.sprite = letter.sprite;
        letterText.text = letterType.ToString();
    }

    public void Show() => gameObject.SetActiveIfNeeded(true);
    public void Hide() => gameObject.SetActiveIfNeeded(false);

    private void OnValidate()
    {
        // if (!ValidateConfig()) return;
        // if (!ValidateLetterType()) return;

        // var letter = lettersConfig.Letters[letterType];
        // if (letter != null)
        // {
        //     SetLetter(letterType, letter);
        // }
    }

    private bool ValidateConfig()
    {
        if (lettersConfig == null)
        {
            Debug.LogError(NULL_CONFIG_ERROR);
            return false;
        }
        if (lettersConfig.Letters == null)
        {
            Debug.LogError(NULL_DICT_ERROR);
            return false;
        }
        return true;
    }

    private bool ValidateLetterType()
    {
        if (!lettersConfig.Letters.ContainsKey(letterType))
        {
            Debug.LogWarning(string.Format(LETTER_NOT_FOUND_WARNING, letterType));
            return false;
        }
        return true;
    }
}