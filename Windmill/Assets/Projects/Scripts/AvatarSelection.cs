using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;


public class AvatarSelection : MonoBehaviour
{
    public Image avatarImage; // UI Image to display the avatar
    public Sprite[] avatarSprites; // Array of avatar sprites to choose from
    private int currentSelection = 0; // Current selected avatar index

    void Start()
    {
        // Load the saved selection from custom properties, default to 0 if not set
        currentSelection = PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("CharacterId", out object selection) ? (int)selection : 0;
        UpdateAvatarImage();
    }

    private void UpdateAvatarImage()
    {
        if (avatarImage != null && avatarSprites.Length > 0)
        {
            avatarImage.sprite = avatarSprites[currentSelection];
        }
    }

}
