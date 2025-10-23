using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnglishDeutchObject", menuName = "Scriptable Objects/EnglishDeutchObject")]
public class EnglishDeutchObject : ScriptableObject
{
    public bool isEnglish = true;

    public List<WordPair> wordPairs = new List<WordPair>();

    public string GetGermanWord(string englishWord)
    {
        var pair = wordPairs.Find(p => p.englishWord.ToLower() == englishWord.ToLower());
        return pair?.germanWord ?? "";
    }

    public string GetEnglishWord(string germanWord)
    {
        var pair = wordPairs.Find(p => p.germanWord.ToLower() == germanWord.ToLower());
        return pair?.englishWord ?? "";
    }
}

    [System.Serializable]
    public class WordPair
    {
        public string englishWord;
        public string germanWord;
    }
