using UnityEngine;
using UnityEngine.UI;

public class WebCamManager : SingletonMonoBehavior<WebCamManager>
{
    [SerializeField] private bool flipHorizontal = true;
    private static WebCamDevice[] WebCamDevices => WebCamTexture.devices;
    private WebCamTexture _webCamTexture;
    private RawImage _webCamImage;
    private const int DEFAULT_WEBCAM_INDEX = 0;
    
    public static bool HasWebCamDevice => WebCamTexture.devices.Length > 0;
    public Texture2D ProcessingTexture { get; private set; }

    public void StartWebCam(RawImage image)
    {
        if (WebCamDevices.Length == 0)
        {
            Debug.LogError("No webcam devices found!");
            return;
        }

        _webCamImage = image;
        WebCamDevice device = GetCurrentWebCamDevice();
        const int requestedFPS = ApplicationConfig.TargetFrameRate / 2;
        _webCamTexture = new WebCamTexture(device.name)
        {
            requestedFPS = requestedFPS,
            requestedWidth = 1280,
            requestedHeight = 720
        };

        _webCamImage.texture = _webCamTexture;
        _webCamImage.material.mainTexture = _webCamTexture;

        UpdateFlip();
        _webCamTexture.Play();

        ProcessingTexture = new Texture2D(_webCamTexture.width, _webCamTexture.height, TextureFormat.RGB24, false);
    }

    public void StopWebCam()
    {
        if (_webCamTexture == null || !_webCamTexture.isPlaying) return;

        try
        {
            _webCamTexture.Stop();
            if (ProcessingTexture != null)
            {
                Destroy(ProcessingTexture);
                ProcessingTexture = null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error stopping webcam: {e.Message}");
        }
    }

    public void Update()
    {
        if (_webCamTexture == null || !_webCamTexture.isPlaying) return;
        var pixels = _webCamTexture.GetPixels32();
        ProcessingTexture.SetPixels32(pixels);
        ProcessingTexture.Apply();
    }
    
    protected override void OnDestroy()
    {
        StopWebCam();
        base.OnDestroy();
    }

    private WebCamDevice GetCurrentWebCamDevice()
    {
        return WebCamDevices.Length > DEFAULT_WEBCAM_INDEX
            ? WebCamDevices[DEFAULT_WEBCAM_INDEX]
            : WebCamDevices[0];
    }

    private void UpdateFlip()
    {
        if (_webCamImage == null) return;

        Vector3 scale = _webCamImage.transform.localScale;
        scale.x = flipHorizontal ? -1f : 1f;
        _webCamImage.transform.localScale = scale;
    }

    public void ToggleFlip()
    {
        flipHorizontal = !flipHorizontal;
        UpdateFlip();
    }
}