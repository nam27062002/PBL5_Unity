using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(-19550)]
public class TCPClient : SingletonMonoBehavior<TCPClient>
{
    [Title("Server Config")]
    [SerializeField] private string serverIP = "127.0.0.1";
    [SerializeField] private int serverPort = 5005;

    private TcpClient _tcpClient;
    private NetworkStream _stream;
    private Thread _receiveThread;
    private bool _isRunning = true;
    public event Action<KeyData, string> OnStringReceived;

    private void Start()
    {
        ConnectToServer();
    }

    public void ConnectToServer()
    {
        try
        {
            AlkawaDebug.Log(ELogCategory.TCP, $"Connecting to server {serverIP}:{serverPort}...");
            _tcpClient = new TcpClient();
            _tcpClient.Connect(serverIP, serverPort);
            _stream = _tcpClient.GetStream();
            _isRunning = true;

            _receiveThread = new Thread(ReceiveData)
            {
                IsBackground = true
            };
            _receiveThread.Start();

            AlkawaDebug.Log(ELogCategory.TCP, "Connection successful and receive thread started.");
        }
        catch (Exception ex)
        {
            AlkawaDebug.LogError(ELogCategory.TCP, $"Error connecting to server: {ex.Message}");
        }
    }

    private void DisconnectToServer()
    {
        try
        {
            AlkawaDebug.Log(ELogCategory.TCP, "Disconnecting from server...");
            _isRunning = false;
            _stream?.Close();
            _tcpClient?.Close();
            if (_receiveThread is { IsAlive: true })
            {
                _receiveThread.Join();
            }
            AlkawaDebug.Log(ELogCategory.TCP, "Disconnected successfully.");
        }
        catch (Exception ex)
        {
            AlkawaDebug.LogError(ELogCategory.TCP, $"Error disconnecting: {ex.Message}");
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
                break;
            case byte[] bytes:
                payload = bytes;
                break;
            default:
                AlkawaDebug.LogWarning(ELogCategory.TCP, $"Unsupported data type: {typeof(T)}.");
                return;
        }

        var keyBytes = BitConverter.GetBytes((int)keyData);
        var lengthBytes = BitConverter.GetBytes(payload.Length);
        var finalData = new byte[4 + keyBytes.Length + payload.Length];
        Buffer.BlockCopy(lengthBytes, 0, finalData, 0, 4);
        Buffer.BlockCopy(keyBytes, 0, finalData, 4, keyBytes.Length);
        Buffer.BlockCopy(payload, 0, finalData, 8, payload.Length);

        SendDataInternal(finalData);
        // AlkawaDebug.Log(ELogCategory.TCP, $"Sending data to TCP | key = {keyData} | type = {data.GetType()} | payload = {payload.Length}");
    }

    private void SendDataInternal(byte[] data)
    {
        if (_stream == null)
        {
            AlkawaDebug.LogWarning(ELogCategory.TCP, "TCP Client not connected to server");
            return;
        }
        try
        {
            _stream.Write(data, 0, data.Length);
        }
        catch (Exception e)
        {
            AlkawaDebug.LogError(ELogCategory.TCP, $"Error when sending data: {e.Message}");
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
                if (_stream.DataAvailable)
                {
                    var lengthBuffer = new byte[4];
                    if (_stream.Read(lengthBuffer, 0, 4) < 4) continue;
                    var payloadLength = BitConverter.ToInt32(lengthBuffer, 0);

                    var keyBuffer = new byte[4];
                    if (_stream.Read(keyBuffer, 0, 4) < 4) continue;
                    var keyValue = BitConverter.ToInt32(keyBuffer, 0);
                    var keyData = (KeyData)keyValue;

                    var payloadBuffer = new byte[payloadLength];
                    int bytesRead = 0;
                    while (bytesRead < payloadLength)
                    {
                        bytesRead += _stream.Read(payloadBuffer, bytesRead, payloadLength - bytesRead);
                    }
                    var response = Encoding.ASCII.GetString(payloadBuffer);

                    OnStringReceived?.Invoke(keyData, response);
                    // AlkawaDebug.Log(ELogCategory.TCP, $"ReceiveData -> Key = {keyData}, Data = {response}");
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
            catch (Exception e)
            {
                if (_isRunning)
                {
                    AlkawaDebug.LogError(ELogCategory.TCP, $"Error when receiving data: {e.Message}");
                }
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