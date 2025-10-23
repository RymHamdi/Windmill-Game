using UnityEngine;

public class Language : MonoBehaviour
{

    public EnglishDeutchObject languageData;

    public static Language Instance;

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
        }
    }

    public string GetWord(string word)
    {
        if (languageData.isEnglish)
        {
            return word;
        }
        else
        {
            return languageData.GetGermanWord(word);
        }
    }


}
