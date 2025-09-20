using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class SecretLanguageRoundSyncManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static SecretLanguageRoundSyncManager Instance;

    [Header("Game Data")]
    public List<SecretLanguageRoundPropSync> allPropAssets; // assign ScriptableObjects in inspector
    public List<SyncedRoundProp> currentSequence = new List<SyncedRoundProp>();

    [Header("Photon")]
    private const byte SyncRoundEventCode = 101;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable() => PhotonNetwork.AddCallbackTarget(this);
    private void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);

    void Start()
    {
        GenerateAndSyncSequence(17);
    }

    // Master generates a sequence and syncs to others
    public void GenerateAndSyncSequence(int totalNeeded)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        currentSequence.Clear();

        for (int i = 0; i < totalNeeded; i++)
        {
            // pick random asset
            int randomAssetIndex = Random.Range(0, allPropAssets.Count);
            var selectedAsset = allPropAssets[randomAssetIndex];

            // pick random prop inside it
            var randomProp = selectedAsset.randomSecretLanguageRoundProps[
                Random.Range(0, selectedAsset.randomSecretLanguageRoundProps.Count)
            ];

            currentSequence.Add(new SyncedRoundProp
            {
                itemId = selectedAsset.itemId,
                itemRandomId = randomProp.itemRandomId
            });
        }

        Debug.Log($"[Master] Generated sequence of {currentSequence.Count} items");

        // pack into flat object[] for Photon
        object[] data = new object[currentSequence.Count * 2];
        for (int i = 0; i < currentSequence.Count; i++)
        {
            data[i * 2] = currentSequence[i].itemId;
            data[i * 2 + 1] = currentSequence[i].itemRandomId;
        }

        // raise event
        PhotonNetwork.RaiseEvent(
            SyncRoundEventCode,
            data,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable
        );
    }

    // Called when Photon events are received
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != SyncRoundEventCode) return;

        object[] data = (object[])photonEvent.CustomData;
        currentSequence.Clear();

        for (int i = 0; i < data.Length; i += 2)
        {
            currentSequence.Add(new SyncedRoundProp
            {
                itemId = (int)data[i],
                itemRandomId = (int)data[i + 1]
            });
        }

        Debug.Log($"[Client] Received synced sequence of {currentSequence.Count} items");

        // Here you notify SecretLanguageManager to start round with this sequence
        SecretLanguageManager.Instance.SetupWithSyncedSequence(currentSequence);
    }
}

[System.Serializable]
public class SyncedRoundProp
{
    public int itemId;
    public int itemRandomId;
}
