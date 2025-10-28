using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnglishDeutchObject", menuName = "Scriptable Objects/EnglishDeutchObject")]
public class EnglishDeutchObject : ScriptableObject
{
    public bool isEnglish = true;

    public List<WordPair> wordPairs = new List<WordPair>();


    public string GetGermanWord(string englishWord)
    {
        if (string.IsNullOrWhiteSpace(englishWord)) return "";

        var pair = wordPairs.Find(p => (p.englishWord == englishWord));
        return pair != null ? pair.germanWord : "";
    }

    public string GetEnglishWord(string germanWord)
    {
        if (string.IsNullOrWhiteSpace(germanWord)) return "";

        var pair = wordPairs.Find(p => p.germanWord == germanWord);
        return pair != null ? pair.englishWord : "";
    }

    public string ChangeToEnglishWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return "";

        var pair = wordPairs.Find(p => p.englishWord.Replace(" ", "").ToLower() == word.Replace(" ", "").ToLower());
        if (pair != null) return pair.englishWord;

        pair = wordPairs.Find(p => p.germanWord.Replace(" ", "").ToLower() == word.Replace(" ", "").ToLower());
        if (pair != null) return pair.englishWord;

        Debug.LogWarning($"⚠️ No match found for '{word}' in ChangeToEnglishWord()");
        return word;
    }

    public string ChangeGermanLang(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return "";

       

        var pair = wordPairs.Find(p => p.germanWord.Replace(" ", "").ToLower() == word.Replace(" ", "").ToLower());
        if (pair != null) return pair.germanWord;

        pair = wordPairs.Find(p => p.englishWord.Replace(" ", "").ToLower() == word.Replace(" ", "").ToLower());
        if (pair != null) return pair.germanWord;

        Debug.LogWarning($"⚠️ No match found for '{word}' in ChangeGermanLang()");
        return word;
    }
}

    [System.Serializable]
    public class WordPair
    {
        public string englishWord;
        public string germanWord;
    }
