using UnityEngine;
using UnityEngine.UI;



public class PlayerRoomModel : MonoBehaviour
{
    public int characterId; // ID of the selected character
    public string playerPhotonId; // Photon player ID

    public Image characterImage;


    private WindmillCharacter currentCharacter;

    public void Init(int id, string photonId)
    {
        characterId = id;
        playerPhotonId = photonId;
        LoadCharacter();
        LobbyManager.Instance.OnPlayerLeft += RemovePlayer;
    }

    public void RemovePlayer(string photonId)
    {
        if (playerPhotonId == photonId)
        {
            Destroy(gameObject);

            Debug.Log($"Player with Photon ID {photonId} has left the room. Removing their PlayerRoomModel.");
        }
    }

    void LoadCharacter()
    {
        currentCharacter = GameManager.Instance.GetCharacterById(characterId);
        if (currentCharacter != null)
        {
            Debug.Log($"Loaded Character: {currentCharacter.characterName}");
            characterImage.sprite = currentCharacter.icon;
            // Here you can add code to update the player room model with character details
        }
        else
        {
            Debug.LogError("Failed to load character.");
        }
    }

    void OnDisable()
    {
        LobbyManager.Instance.OnPlayerLeft -= RemovePlayer;
    }
}
