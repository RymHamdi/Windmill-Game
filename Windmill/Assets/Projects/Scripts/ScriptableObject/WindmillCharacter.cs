using UnityEngine;

[CreateAssetMenu(fileName = "WindmillCharacter", menuName = "Scriptable Objects/WindmillCharacter")]
public class WindmillCharacter : ScriptableObject
{
    [Header("Identification")]
    public int id;
    public Sprite icon;       // Character icon for UI

    [Header("Stats")]
    public float wiekslag;    // Blade length / swing
    public float hoogte;      // Height of windmill
    public float design;      // Design rating or style factor

    public string characterName; // Character name

    [TextArea]
    public string description;   // Character description or lore
}
