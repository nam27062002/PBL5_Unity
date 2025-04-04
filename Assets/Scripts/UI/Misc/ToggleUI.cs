using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ToggleUI : MonoBehaviour
{
    public Toggle toggle;
    public Image image;
    public Sprite onSprite;
    public Sprite offSprite;
    public TextMeshProUGUI text;
    
    public string onText = "Random: On";
    public string offText = "Random: Off";

    private void Awake()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }
    }

    private void OnEnable()
    {
        if (toggle != null && image != null)
        {
            image.sprite = toggle.isOn ? onSprite : offSprite;
        }
    }

    private void OnDestroy()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }
    }
    
    private void OnValidate()
    {
        if (toggle == null)
            toggle = GetComponent<Toggle>();
            
        if (image == null && toggle != null)
            image = toggle.targetGraphic as Image;
            
        if (toggle != null && image != null)
        {
            image.sprite = toggle.isOn ? onSprite : offSprite;
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(image);
            #endif
        }
    }

    public void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            image.sprite = onSprite;
            if (text != null)
            {
                text.text = onText;
                text.color = Color.green;
            }
        }
        else
        {
            image.sprite = offSprite;
            if (text != null)
            {
                text.text = offText;
                text.color = Color.red;
            }
        }
    }

    private void Start()
    {
        OnToggleValueChanged(toggle.isOn);
    }
}

