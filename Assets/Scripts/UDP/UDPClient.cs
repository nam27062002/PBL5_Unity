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

    #region Events
    public event Action<string> OnStringReceived;
    public event Action<byte[]> OnBytesReceived;

    #endregion

    private void Start()
    {
        ConnectToServer();
    }
    
    private void ConnectToServer()
    {
        _serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
        _udpClient = new UdpClient(localPort);
        _receiveThread = new Thread(ReceiveData)
        {
            IsBackground = true
        };
        _receiveThread.Start();
    }
    
    private void DisconnectToServer()
    {
        _isRunning = false;
        if (_udpClient != null)
        {
            _udpClient.Close();
            _udpClient = null;
        }
        if (_receiveThread is { IsAlive: true })
        {
            _receiveThread.Join();
        }
    }
    
    #region Send Data Methods

    public void SendData<T>(T data)
    {
        switch (data)
        {
            case string str:
                SendData(str);
                break;
            case int intVal:
                SendData(intVal);
                break;
            case float floatVal:
                SendData(floatVal);
                break;
            case Texture2D texture:
                SendData(texture);
                break;
            case byte[] bytes:
                SendData(bytes);
                break;
        }
        AlkawaDebug.Log(ELogCategory.API, $"Sending data to UDP | type = {data.GetType()}");
    }
    
    private void SendData(string data)
    {
        var sendBytes = Encoding.ASCII.GetBytes(data);
        SendData(sendBytes);
    }
    
    private void SendData(int data)
    {
        var intBytes = BitConverter.GetBytes(data);
        SendData(intBytes);
    }
    
    private void SendData(float data)
    {
        var floatBytes = BitConverter.GetBytes(data);
        SendData(floatBytes);
    }

    private void SendData(Texture2D texture)
    {
        var textureBytes = texture.EncodeToPNG();
        SendData(textureBytes);
    }
    private void SendData(byte[] data)
    {
        if (_udpClient == null)
        {
            AlkawaDebug.LogWarning(ELogCategory.API, "UDP Client not connected to server");
            return;
        }
        try
        {
            _udpClient.Send(data, data.Length, _serverEndPoint);
        }
        catch (Exception e)
        {
            AlkawaDebug.LogError(ELogCategory.API, $"Error when send data: {e.Message}");
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
                var data = _udpClient.Receive(ref anyIP);
                OnBytesReceived?.Invoke(data);
                var response = Encoding.ASCII.GetString(data);
                Debug.Log(response);
                OnStringReceived?.Invoke(response);
            }
            catch (SocketException socketEx)
            {
                if (!_isRunning)
                    break;
                AlkawaDebug.LogError(ELogCategory.API, $"Error Socket: {socketEx.Message}");
            }
            catch (Exception e)
            {
                AlkawaDebug.LogError(ELogCategory.API,$"Error when receive data: {e.Message}");
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
