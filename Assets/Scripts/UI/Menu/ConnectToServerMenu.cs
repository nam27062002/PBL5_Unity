using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectToServerMenu : MenuBase
{
    [Title("On Boarding"), Space]
    [SerializeField] private TMP_InputField ip;
    [SerializeField] private TMP_InputField port;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private Button connectToServerButton;

    private bool ipFocused = false;
    private bool portFocused = false;

    private void Start()
    {
        connectToServerButton.onClick.AddListener(OnConnectButtonClicked);
        TCPClient.Instance.OnConnectResult += OnConnectResult;
        errorText.text = string.Empty;
        ip.onValueChanged.AddListener(_ => errorText.text = string.Empty);
        port.onValueChanged.AddListener(_ => errorText.text = string.Empty);
    }

    private void OnEnable()
    {
        ipFocused = false;
        portFocused = false;
// #if UNITY_EDITOR
//         ip.text = "127.0.0.1";
//         port.text = "5005";
//         OnConnectButtonClicked();
// #endif
    }

    private void Update()
    {
        // Focus vào IP lần đầu khi menu mở
        if (!ipFocused && ip != null && ip.gameObject.activeInHierarchy)
        {
            ip.Select();
            ipFocused = true;
        }

        // Xử lý Tab và Enter
        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == ip.gameObject)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                port.Select();
                portFocused = true;
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                connectToServerButton.onClick.Invoke();
            }
        }
        else if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == port.gameObject)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ip.Select();
                ipFocused = true;
            }
            else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                connectToServerButton.onClick.Invoke();
            }
        }
    }

    private bool IsValidIP(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress)) return false;
        var parts = ipAddress.Split('.');
        if (parts.Length != 4) return false;
        foreach (var part in parts)
        {
            if (!int.TryParse(part, out int num) || num < 0 || num > 255)
                return false;
        }
        return true;
    }

    private void OnConnectButtonClicked()
    {
        string ipAddress = ip.text.Trim();
        string portStr = port.text.Trim();
        errorText.text = string.Empty;

        if (string.IsNullOrEmpty(ipAddress))
        {
            errorText.text = "Please enter the server IP address.";
            return;
        }
        if (!IsValidIP(ipAddress))
        {
            errorText.text = "Invalid IP address format. Example: 192.168.1.100";
            return;
        }
        if (string.IsNullOrEmpty(portStr))
        {
            errorText.text = "Please enter the server port.";
            return;
        }
        if (!int.TryParse(portStr, out int portNumber) || portNumber < 1 || portNumber > 65535)
        {
            errorText.text = "Invalid port. Port must be a number between 1 and 65535.";
            return;
        }

        TCPClient.Instance.SetServerConfig(ipAddress, portNumber);
        _ = TryConnectAsync();
        UIManager.OpenPopup(PopupType.Loading);
    }

    private async System.Threading.Tasks.Task TryConnectAsync()
    {
        errorText.text = string.Empty;
        bool isSuccess = await TCPClient.Instance.TryConnectToServerAsync();
        // Kết quả sẽ được xử lý ở OnConnectResult
    }

    private void OnConnectResult(bool isSuccess)
    {
        UIManager.ClosePopup();
        if (isSuccess)
        {
            errorText.text = string.Empty;
            Debug.Log("Connected successfully!");
            UIManager.OpenMenu(MenuType.OnBoarding);
        }
        else
        {
            errorText.text = "Failed to connect to server. Please check your IP and port, and make sure the server is running.";
            Debug.LogError("Connection failed!");
            // TODO: Xử lý khi kết nối thất bại
        }
    }
}