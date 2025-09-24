using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class ShowControlReceiver : MonoBehaviour
{
    public int listenPort = 5000;
    private UdpClient udpClient;

    private void Start()
    {
        udpClient = new UdpClient(listenPort);
        udpClient.BeginReceive(OnReceive, null);
        Debug.Log($"[ShowControlReceiver] Listening on port {listenPort}...");
    }

    private void OnReceive(System.IAsyncResult ar)
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, listenPort);
        byte[] data = udpClient.EndReceive(ar, ref remoteEP);
        string message = Encoding.UTF8.GetString(data);

        Debug.Log($"[ShowControlReceiver] Got message: {message}");

        // Keep listening
        udpClient.BeginReceive(OnReceive, null);
    }

    private void OnDestroy()
    {
        udpClient?.Close();
    }
}
