using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class ShowControlTrigger : MonoBehaviour
{
    public static ShowControlTrigger Instance;

    [Header("Show Control Settings")]
    public string controllerIP = "192.168.0.100"; // Replace with their system’s IP
    public int controllerPort = 5000;             // Replace with their listening port

    private UdpClient udpClient;
    private IPEndPoint endPoint;

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

    private void OnDestroy()
    {
        udpClient?.Close();
    }
}
