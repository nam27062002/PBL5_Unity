using UnityEngine;
using UnityEngine.UI;

public class WebCamManager : SingletonMonoBehavior<WebCamManager>
{
    private static WebCamDevice[] WebCamDevices => WebCamTexture.devices;
    private readonly int _webCamIndex = 0;
    private RawImage _webCamImage;
    private WebCamTexture _webCamTexture;
    [SerializeField] private bool flipHorizontal = true;

    public void SetupWebCam(RawImage image)
    {
        _webCamImage = image;
        _webCamTexture = new WebCamTexture(GetCurrentWebCamDevice().name);
        _webCamImage.texture = _webCamTexture;
        _webCamImage.material.mainTexture = _webCamTexture;
        UpdateFlip();
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

    private void UpdateFlip()
    {
        if (_webCamImage != null)
        {
            Vector3 scale = _webCamImage.transform.localScale;
            scale.x = flipHorizontal ? -1f : 1f;
            _webCamImage.transform.localScale = scale;
        }
    }

    public void ToggleFlip()
    {
        flipHorizontal = !flipHorizontal;
        UpdateFlip();
    }
}