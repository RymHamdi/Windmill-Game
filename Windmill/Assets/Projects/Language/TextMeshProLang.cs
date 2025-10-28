using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System;

public class TextMeshProLang : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    private bool isInitialized = false;
    

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
        if (textMeshPro != null && Language.Instance != null && !isInitialized)
        {
            string originalText = Clean(textMeshPro.text);
            textMeshPro.text = Language.Instance.GetWord(originalText);
            isInitialized = true;
        }
    }

    public void OnUserUpdateLang(bool check)
    {
        string originalText = Clean(textMeshPro.text);
        textMeshPro.text = Language.Instance.GetWord(originalText);
            
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
