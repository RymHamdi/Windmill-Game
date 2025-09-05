using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Characters")]
    public WindmillCharacter[] characters; // Assign in Inspector

    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // keep across scenes
    }

    /// <summary>
    /// Get a character by its ID
    /// </summary>
    public WindmillCharacter GetCharacterById(int id)
    {
        foreach (var character in characters)
        {
            if (character.id == id)
                return character;
        }

        Debug.LogWarning($"Character with ID {id} not found!");
        return null;
    }
}
