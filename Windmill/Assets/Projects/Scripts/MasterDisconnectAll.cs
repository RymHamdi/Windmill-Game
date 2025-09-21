using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MasterDisconnectAll : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [Header("UI")]
    public Button disconnectButton; // assign in Inspector
    public string firstSceneName = "Launcher"; // change to your first scene

    private const byte KickEventCode = 0; // custom Photon event

    void Start()
    {
        // Show button only for Master
        if (disconnectButton != null)
        {
            disconnectButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
            disconnectButton.onClick.AddListener(OnMasterDisconnectClicked);
        }
    }

    private void OnMasterDisconnectClicked()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Debug.Log("[Master] Sending kick event to all clients.");

        PhotonNetwork.RaiseEvent(
            KickEventCode,
            null, // no payload needed
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable
        );
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == KickEventCode)
        {
            Debug.Log("[Client] Received kick event, disconnecting...");
            PhotonNetwork.Disconnect();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("[Client] Disconnected. Returning to first scene.");
        SceneManager.LoadScene(firstSceneName);
    }

    private void OnEnable() => PhotonNetwork.AddCallbackTarget(this);
    private void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);
}
