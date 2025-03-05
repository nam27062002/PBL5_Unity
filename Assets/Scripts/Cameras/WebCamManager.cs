using UnityEngine;
using UnityEngine.UI;

public class WebCamManager : SingletonMonoBehavior<WebCamManager>
{
    public static WebCamDevice[] WebCamDevices => WebCamTexture.devices;
    private WebCamTexture _webCamTexture;
    private RawImage _webCamImage;
    [SerializeField] private bool flipHorizontal = true;
    private Texture2D _processingTexture;
    private const int DEFAULT_WEBCAM_INDEX = 0;

    public void StartWebCam(RawImage image)
    {
        if (WebCamDevices.Length == 0)
        {
            Debug.LogError("No webcam devices found!");
            return;
        }

        _webCamImage = image;
        WebCamDevice device = GetCurrentWebCamDevice();
        
        _webCamTexture = new WebCamTexture(device.name)
        {
            requestedFPS = ApplicationConfig.TargetFrameRate,
            requestedWidth = 1280,
            requestedHeight = 720
        };

        _webCamImage.texture = _webCamTexture;
        _webCamImage.material.mainTexture = _webCamTexture;
        
        UpdateFlip();
        _webCamTexture.Play();

        _processingTexture = new Texture2D(_webCamTexture.width, 
            _webCamTexture.height, TextureFormat.RGB24, false);
    }

    public void StopWebCam()
    {
        if (_webCamTexture == null || !_webCamTexture.isPlaying) return;

        try
        {
            _webCamTexture.Stop();
            if (_processingTexture != null)
            {
                Destroy(_processingTexture);
                _processingTexture = null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error stopping webcam: {e.Message}");
        }
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

    private void Update()
    {
        if (_webCamTexture == null || !_webCamTexture.isPlaying) return;

        try
        {
            ProcessWebcamFrame();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error processing webcam frame: {e.Message}");
        }
    }

    private void ProcessWebcamFrame()
    {
        Color32[] pixels = _webCamTexture.GetPixels32();
        _processingTexture.SetPixels32(pixels);
        _processingTexture.Apply();
        UDPClient.Instance.SendData(KeyData.LetterPrediction, _processingTexture);
    }
}