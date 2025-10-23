using UnityEngine;
using TMPro;
using Unity.VisualScripting;

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
    }

    public void UpdateText()
    {
        if (textMeshPro != null && Language.Instance != null && !isInitialized) 
        {
            string originalText = textMeshPro.text;
            Debug.Log(originalText);
            textMeshPro.text = Language.Instance.GetWord(originalText);
            isInitialized = true;
        }
    }
}
