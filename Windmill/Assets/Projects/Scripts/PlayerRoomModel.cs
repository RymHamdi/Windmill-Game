using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;



public class PlayerRoomModel : MonoBehaviour
{
    public int characterId; // ID of the selected character
    public string playerPhotonId; // Photon player ID

    public Image characterImage;

    public bool hasAPlayer = false;
    public bool isLocalPlayer = false;

    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI strenthText;
    public Image localPlayerIndicator;


    private WindmillCharacter currentCharacter;

    public void Init(int id, string photonId, bool isLocal)
    {
        characterId = id;
        playerPhotonId = photonId;
        isLocalPlayer = isLocal;
        hasAPlayer = true;
        LoadCharacter();
        LobbyManager.Instance.OnPlayerLeft += RemovePlayer;
    }

    public void RemovePlayer(string photonId)
    {
        if (playerPhotonId == photonId)
        {
            hasAPlayer = false;
            isLocalPlayer = false;
            characterImage.sprite = null;
            playerNameText.text = "";
            strenthText.text = "";
            gameObject.SetActive(false);
            

            Debug.Log($"Player with Photon ID {photonId} has left the room. Disabling their PlayerRoomModel.");
        }
    }

    void LoadCharacter()
    {
        currentCharacter = GameManager.Instance.GetCharacterById(characterId);
        if (currentCharacter != null)
        {
            Debug.Log($"Loaded Character: {currentCharacter.characterName}");
            characterImage.sprite = currentCharacter.icon;
            playerNameText.text = currentCharacter.characterName;
            strenthText.text = "Strength: " + currentCharacter.hoogte.ToString();
            localPlayerIndicator.gameObject.SetActive(isLocalPlayer);
            // Add a smooth floating animation using DOTween
            // Moves the GameObject's RectTransform up and down in a loop
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + 10f, 1.2f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
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
