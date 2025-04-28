using UnityEngine;
using UnityEngine.UI;

public class SoundButton : Button
{
    public override void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (SoundManager.HasInstance)
        {
            SoundManager.Instance.PlaySFX(SFXType.ButtonClick);
        }
        base.OnPointerClick(eventData);
    }
}
