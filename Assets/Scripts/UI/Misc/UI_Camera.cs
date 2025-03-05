using UnityEngine;
using UnityEngine.UI;

public class UI_Camera : MonoBehaviour
{
    public RawImage rawImage;
    
    public void OnEnable()
    {
        WebCamManager.Instance.StartWebCam(rawImage);
    }

    public void OnDisable()
    {
        if (WebCamManager.Instance != null)
            WebCamManager.Instance.StopWebCam();
    }
}
