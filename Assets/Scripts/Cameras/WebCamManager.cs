using UnityEngine;
using UnityEngine.UI;

public class WebCamManager : SingletonMonoBehavior<WebCamManager>
{
    private static WebCamDevice[] WebCamDevices => WebCamTexture.devices;
    private readonly int _webCamIndex = 0;
    private RawImage _webCamImage;
    private WebCamTexture _webCamTexture;

    public void SetupWebCam(RawImage image)
    {
        _webCamImage = image;
        _webCamTexture = new WebCamTexture(GetCurrentWebCamDevice().name);
        _webCamImage.texture = _webCamTexture;
        _webCamImage.material.mainTexture = _webCamTexture;
        _webCamTexture.Play(); 
    }
    
    protected override void OnDestroy()
    {
        if (_webCamTexture != null)
        {
            _webCamTexture.Stop();
        }
    }

    private WebCamDevice GetCurrentWebCamDevice()
    {
        return WebCamDevices.Length < _webCamIndex ? WebCamTexture.devices[_webCamIndex] : new WebCamDevice();
    }
}