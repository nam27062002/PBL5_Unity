using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(-19550)]
public class UDPClient : SingletonMonoBehavior<UDPClient>
{
    [Title("Server Config")]
    [SerializeField] private string serverIP = "127.0.0.1";
    [SerializeField] private int serverPort = 5005;
    [SerializeField] private int localPort = 5006; 
    
    private UdpClient _udpClient;
    private IPEndPoint _serverEndPoint;
    private Thread _receiveThread;
    private bool _isRunning = true;

    public event Action<byte[]> OnBytesReceived;
    public event Action<string> OnStringReceived;
    public event Action<KeyData, byte[]> OnKeyDataReceived;

    private void Start()
    {
        ConnectToServer();
    }
    
    public void ConnectToServer()
    {
        try
        {
            AlkawaDebug.Log(ELogCategory.UDP, $"Connecting to server {serverIP}:{serverPort} from local port {localPort}...");

            _serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
            _udpClient = new UdpClient(localPort);
            _udpClient.Client.SendBufferSize = 1024 * 1024;
            _udpClient.Client.ReceiveBufferSize = 1024 * 1024;

            _isRunning = true;
            _receiveThread = new Thread(ReceiveData)
            {
                IsBackground = true
            };
            _receiveThread.Start();

            AlkawaDebug.Log(ELogCategory.UDP,"Connection successful and receive thread started.");
        }
        catch (Exception ex)
        {
            AlkawaDebug.Log(ELogCategory.UDP,$"Error connecting to server: {ex.Message}");
        }
    }

    private void DisconnectToServer()
    {
        try
        {
            AlkawaDebug.Log(ELogCategory.UDP,"Disconnecting from server...");

            _isRunning = false;

            if (_udpClient != null)
            {
                _udpClient.Close();
                _udpClient = null;
                AlkawaDebug.Log(ELogCategory.UDP,"UDP client closed successfully.");
            }

            if (_receiveThread is not { IsAlive: true }) return;
            _receiveThread.Join();
            AlkawaDebug.Log(ELogCategory.UDP,"Receive thread terminated successfully.");
        }
        catch (Exception ex)
        {
            AlkawaDebug.LogError(ELogCategory.UDP,$"Error disconnecting: {ex.Message}");
        }
    }

    #region Send Data Methods
    
    public void SendData<T>(KeyData keyData, T data)
    {
        byte[] payload;

        switch (data)
        {
            case string str:
                payload = Encoding.ASCII.GetBytes(str);
                break;
            case int intVal:
                payload = BitConverter.GetBytes(intVal);
                break;
            case float floatVal:
                payload = BitConverter.GetBytes(floatVal);
                break;
            case Texture2D texture:
                payload = texture.EncodeToJPG(); 
                // payload = texture.EncodeToPNG(); 
                break;
            case byte[] bytes:
                payload = bytes;
                break;
            default:
                AlkawaDebug.LogWarning(ELogCategory.UDP,
                    $"Unsupported data type: {typeof(T)}. You might add more case if needed.");
                return;
        }
        var keyBytes = BitConverter.GetBytes((int)keyData);
        var finalData = new byte[keyBytes.Length + payload.Length];
        Buffer.BlockCopy(keyBytes, 0, finalData, 0, keyBytes.Length);
        Buffer.BlockCopy(payload, 0, finalData, keyBytes.Length, payload.Length);

        SendDataInternal(finalData);
        AlkawaDebug.Log(ELogCategory.UDP, $"Sending data to UDP | key = {keyData} | type = {data.GetType()}");
    }
    
    private void SendDataInternal(byte[] data)
    {
        if (_udpClient == null)
        {
            AlkawaDebug.LogWarning(ELogCategory.UDP, "UDP Client not connected to server");
            return;
        }
        try
        {
            _udpClient.Send(data, data.Length, _serverEndPoint);
        }
        catch (Exception e)
        {
            AlkawaDebug.LogError(ELogCategory.UDP, $"Error when send data: {e.Message}");
        }
    }

    #endregion

    #region Receive Data Methods
    
    private void ReceiveData()
    {
        while (_isRunning)
        {
            try
            {
                var anyIP = new IPEndPoint(IPAddress.Any, 0);
                var receivedBytes = _udpClient.Receive(ref anyIP);
                if (receivedBytes.Length < 4) 
                {
                    AlkawaDebug.LogWarning(ELogCategory.UDP, "Received data too short, ignore");
                    continue;
                }
                int keyValue = BitConverter.ToInt32(receivedBytes, 0);
                KeyData keyData = (KeyData)keyValue;
                byte[] payload = new byte[receivedBytes.Length - 4];
                Buffer.BlockCopy(receivedBytes, 4, payload, 0, payload.Length);

                OnKeyDataReceived?.Invoke(keyData, payload);
                OnBytesReceived?.Invoke(payload);
                string response = Encoding.ASCII.GetString(payload);
                AlkawaDebug.Log(ELogCategory.UDP, $"ReceiveData -> Key = {keyData}, Data = {response}");

                OnStringReceived?.Invoke(response);
            }
            catch (SocketException socketEx)
            {
                if (!_isRunning)
                    break;
                AlkawaDebug.LogError(ELogCategory.UDP, $"Error Socket: {socketEx.Message}");
            }
            catch (Exception e)
            {
                AlkawaDebug.LogError(ELogCategory.UDP, $"Error when receive data: {e.Message}");
            }
        }
    }

    #endregion

    protected override void OnDestroy()
    {
        base.OnDestroy();
        DisconnectToServer();
    }
}
