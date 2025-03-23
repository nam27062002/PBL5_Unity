using UnityEngine;
using UnityEngine.UI;

public class UI_Camera : MonoBehaviour
{
    [SerializeField] private RawImage rawImage;

    public void OnEnable()
    {
        WebCamManager.Instance.StartWebCam(rawImage);
    }

    public void OnDisable()
    {
        if (WebCamManager.Instance != null)
            WebCamManager.Instance.StopWebCam();
    }

    public void ShowWebCam()
    {
        gameObject.SetActiveIfNeeded(true);
    }

    public void HideWebCam()
    {
        gameObject.SetActiveIfNeeded(false);
    }
}
