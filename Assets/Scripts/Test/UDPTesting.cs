using Sirenix.OdinInspector;
using UnityEngine;

public enum DataType
{
    None,
    Texture,
    Message,
}

public class UDPTesting : MonoBehaviour
{
    [SerializeField] private UDPClient udpClient;
    [SerializeField] private DataType dataType;
    
    [SerializeField, ShowIf(nameof(IsTexture))]
    private Texture2D texture;

    [SerializeField, ShowIf(nameof(IsMessage))]
    private string message;

    private bool IsMessage => dataType == DataType.Message;
    private bool IsTexture => dataType == DataType.Texture;

    [Button("Send Data To Server")]
    private void SendDataToServer()
    {
        udpClient.ConnectToServer();

        if (IsTexture)
        {
            Texture2D resizedTexture = new Texture2D(224, 224, TextureFormat.RGB24, false);
            RenderTexture rt = RenderTexture.GetTemporary(224, 224);
            Graphics.Blit(texture, rt);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            resizedTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            resizedTexture.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            udpClient.SendData(KeyData.LetterPrediction, resizedTexture);
        }
        else if (IsMessage)
        {
            udpClient.SendData(KeyData.LetterPrediction, message);
        }
    }
}