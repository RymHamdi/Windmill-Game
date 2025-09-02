using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerRoomUIModel", menuName = "Game/Player Room UI Model")]
public class PlayerRoomUIModel : ScriptableObject
{
    [Header("Mill Info")]
    public string millName;
    public Sprite millImage;

    [Header("UI Bars")]
    [Range(0f, 1f)] public float staminaFill = 1f;
    [Range(0f, 1f)] public float lifeFill = 1f;
}
