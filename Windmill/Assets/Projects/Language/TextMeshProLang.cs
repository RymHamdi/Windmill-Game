using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System;

public class TextMeshProLang : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    private bool isInitialized = false;

    public int index = -1;
    

    private void Awake()
    {

        
    }

    void OnEnable()
    {
        
        UpdateText();
        if (Language.Instance != null)
        {
            Language.Instance.onLangUpdated += OnUserUpdateLang;
        }
    }

    private void Start()
    {
        if (Language.Instance != null)
        {
            Language.Instance.onLangUpdated += OnUserUpdateLang;
        }
    }
    
    private string Clean(string text)
{
    if (string.IsNullOrWhiteSpace(text))
        return "";

    // Remove all whitespace & invisible unicode
    string cleaned = System.Text.RegularExpressions.Regex.Replace(text, @"\s+|\u200B|\u200C|\u200D|\uFEFF", "");
    return cleaned.ToLowerInvariant();
}

    public void UpdateText()
    {
        if (textMeshPro != null && Language.Instance != null && index == -1)
        {
            string originalText = Clean(textMeshPro.text);
            index = Language.Instance.languageData.ReturnIndex(originalText);
            if (index != -1)
            {
                Invoke("GetTextAfterIndexing", 0.02f);
            }
            else
            {
                Invoke("UpdateText", 0.02f);
            }
            

        }
    }
    
    private void GetTextAfterIndexing()
    {
        textMeshPro.text = Language.Instance.GetWord(index);
        isInitialized = true;
    }

    public void OnUserUpdateLang()
    {
        if (index != -1)
        {
            textMeshPro.text = Language.Instance.GetWord(index);
        }
        else
        {
            UpdateText();
        }
        
            
    }

    private void OnDisable()
    {
        isInitialized = false;
        if (Language.Instance != null)
        {
            Language.Instance.onLangUpdated -= OnUserUpdateLang;
        }
    }
}
