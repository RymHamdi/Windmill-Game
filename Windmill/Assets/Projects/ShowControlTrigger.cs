using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowControlTrigger : MonoBehaviour
{
    public static ShowControlTrigger Instance;

    [Header("Show Control Settings")]
    public string controllerIP = "127.0.0.1"; // Replace with their system’s IP
    public int controllerPort = 5000;             // Replace with their listening port

    private UdpClient udpClient;
    private IPEndPoint endPoint;

    public TMP_InputField ControllerPort;
    public TMP_InputField ControllerIP;
    public Button SaveButton;




    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

    }

    void OnEnable()
    {
        if (PhotonLauncher.Instance != null)
        {
            if (!PhotonLauncher.Instance.isServer)
            {
                this.enabled = false;
                return;
            }
        }
        InitiialiseUI();
    }

    private void Start()
    {
        udpClient = new UdpClient();
        endPoint = new IPEndPoint(IPAddress.Parse(controllerIP), controllerPort);
    }

    public void SendTrigger(string triggerName)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(triggerName);
            udpClient.Send(data, data.Length, endPoint);
            Debug.Log($"[ShowControl] Sent trigger: {triggerName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[ShowControl] Failed to send trigger: {ex.Message}");
        }
    }

    private void InitiialiseUI()
    {
        // Load saved IP and Port from PlayerPrefs
        controllerIP = PlayerPrefs.GetString("ControllerIP", controllerIP);
        controllerPort = PlayerPrefs.GetInt("ControllerPort", controllerPort);

        // Populate UI fields
        if (ControllerIP != null)
            ControllerIP.text = controllerIP;
        if (ControllerPort != null)
            ControllerPort.text = controllerPort.ToString();

        // Attach button listener
        if (SaveButton != null)
            SaveButton.onClick.AddListener(OnSaveButton);

        OnSaveButton();
    }

    private void OnSaveButton()
{
    // Get values from UI
    if (ControllerIP != null)
        controllerIP = ControllerIP.text;

    if (ControllerPort != null && int.TryParse(ControllerPort.text, out int port))
        controllerPort = port;

    // Save to PlayerPrefs
    PlayerPrefs.SetString("ControllerIP", controllerIP);
    PlayerPrefs.SetInt("ControllerPort", controllerPort);
    PlayerPrefs.Save();

    // FIX: Create UdpClient (only once)
    if (udpClient == null)
        udpClient = new UdpClient();

    // FIX: Always update endpoint
    endPoint = new IPEndPoint(IPAddress.Parse(controllerIP), controllerPort);

    Debug.Log($"[ShowControl] Saved IP: {controllerIP}, Port: {controllerPort}");
}



    private void OnDestroy()
    {
        udpClient?.Close();
    }
}
