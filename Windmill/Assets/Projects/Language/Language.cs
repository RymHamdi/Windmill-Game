using System;
using UnityEngine;

public class Language : MonoBehaviour
{

    public EnglishDeutchObject languageData;

    public static Language Instance;
    public Action<bool> onLangUpdated;
    
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
        string engWord = languageData.ChangeToEnglishWord(word);
        string germandLang = languageData.ChangeGermanLang(word);

        if (languageData.isEnglish)
        {
            string str = languageData.GetEnglishWord(germandLang);
            if (str != "")
            {
                return str;
            }
            return word;
        }
        else
        {
            return languageData.GetGermanWord(engWord);
        }
    }

    public void ChangeLanguage()
    {
        bool currentLang = languageData.isEnglish;
        bool updateLang = !currentLang;
        languageData.isEnglish = updateLang;
        //if true => so the current lang is english
        onLangUpdated(updateLang);
    }


}
