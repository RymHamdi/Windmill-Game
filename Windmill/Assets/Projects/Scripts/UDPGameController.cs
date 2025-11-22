using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;

public class UDPGameController : MonoBehaviour
{
    [Header("UI References")]
    public InputField portInputField;
    public Button saveButton;

    [Header("Settings")]
    public string portPlayerPrefKey = "GameStatePort";
    public int defaultPort = 5000;

    private int currentPort;
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isRunning = false;

    // Queue to dispatch actions to the main thread
    private readonly System.Collections.Concurrent.ConcurrentQueue<Action> mainThreadActions = new System.Collections.Concurrent.ConcurrentQueue<Action>();

    void Start()
    {
        LoadPort();
        InitializeUI();
        StartUDPListener();
    }

    void Update()
    {
        // Execute queued actions on the main thread
        while (mainThreadActions.TryDequeue(out Action action))
        {
            action?.Invoke();
        }
    }

    private void LoadPort()
    {
        currentPort = PlayerPrefs.GetInt(portPlayerPrefKey, defaultPort);
    }

    private void InitializeUI()
    {
        if (portInputField != null)
        {
            portInputField.text = currentPort.ToString();
        }

        if (saveButton != null)
        {
            saveButton.onClick.AddListener(OnSaveButtonClicked);
        }
    }

    private void OnSaveButtonClicked()
    {
        if (portInputField != null && int.TryParse(portInputField.text, out int newPort))
        {
            if (newPort > 0 && newPort <= 65535)
            {
                currentPort = newPort;
                PlayerPrefs.SetInt(portPlayerPrefKey, currentPort);
                PlayerPrefs.Save();
                Debug.Log($"UDP Port saved: {currentPort}");

                // Restart listener with new port
                StopUDPListener();
                StartUDPListener();
            }
            else
            {
                Debug.LogError("Invalid port number. Please enter a value between 1 and 65535.");
            }
        }
    }

    private void StartUDPListener()
    {
        try
        {
            udpClient = new UdpClient(currentPort);
            isRunning = true;
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
            Debug.Log($"UDP Listener started on port {currentPort}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start UDP listener: {e.Message}");
        }
    }

    private void StopUDPListener()
    {
        isRunning = false;
        if (udpClient != null)
        {
            udpClient.Close();
            udpClient = null;
        }
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
    }

    private void ReceiveData()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        while (isRunning)
        {
            try
            {
                if (udpClient == null) break;

                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string message = Encoding.UTF8.GetString(data).Trim();

                Debug.Log($"Received UDP message: {message} from {remoteEndPoint}");

                if (message == "StartGame")
                {
                    mainThreadActions.Enqueue(() =>
                    {
                        if (PhotonLauncher.Instance != null)
                        {
                            Debug.Log("Triggering StartGame via UDP");
                            PhotonLauncher.Instance.StartGame();
                        }
                    });
                }
                else if (message == "EndGame")
                {
                    mainThreadActions.Enqueue(() =>
                    {
                        if (PhotonLauncher.Instance != null)
                        {
                            Debug.Log("Triggering EndGame via UDP");
                            PhotonLauncher.Instance.EndGame();
                        }
                    });
                }
            }
            catch (SocketException)
            {
                // Socket closed or error, ignore if stopping
                if (!isRunning) return;
            }
            catch (ThreadAbortException)
            {
                // Thread aborted
                return;
            }
            catch (Exception e)
            {
                Debug.LogError($"UDP Receive Error: {e.Message}");
            }
        }
    }

    void OnDestroy()
    {
        StopUDPListener();
        if (saveButton != null)
        {
            saveButton.onClick.RemoveListener(OnSaveButtonClicked);
        }
    }
}
