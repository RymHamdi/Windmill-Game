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

    public void UpdateText()
    {
        if (textMeshPro != null && Language.Instance != null && !isInitialized)
        {
            string originalText = textMeshPro.text;
            textMeshPro.text = Language.Instance.GetWord(originalText);
            isInitialized = true;
        }
    }

    public void OnUserUpdateLang(bool check)
    {
        string originalText = textMeshPro.text;;
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
