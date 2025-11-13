using UnityEngine;
using TMPro;
using System.Collections;

public class TextMeshProLang : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI textMeshPro;

    private int index = -1;
    private Coroutine waitForLangCoroutine;

    private void OnEnable()
    {
        TrySubscribe();
        TryInitializeText();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void TrySubscribe()
    {
        if (Language.Instance != null)
            Language.Instance.onLangUpdated += OnUserUpdateLang;
    }

    private void Unsubscribe()
    {
        if (Language.Instance != null)
            Language.Instance.onLangUpdated -= OnUserUpdateLang;
    }

    private void TryInitializeText()
    {
        if (Language.Instance == null || textMeshPro == null)
            return;

        if (index == -1)
        {
            string cleaned = Clean(textMeshPro.text);
            index = Language.Instance.languageData.ReturnIndex(cleaned);

            if (index != -1)
            {
                ApplyText(index);
            }
            else
            {
                // wait until language data is ready without lag
                if (waitForLangCoroutine != null)
                    StopCoroutine(waitForLangCoroutine);
                waitForLangCoroutine = StartCoroutine(WaitForLangReady());
            }
        }
        else
        {
            ApplyText(index);
        }
    }

    private IEnumerator WaitForLangReady()
    {
        float timeout = 1f;
        float elapsed = 0f;

        while (Language.Instance == null || index == -1)
        {
            if (elapsed > timeout)
                yield break;

            yield return new WaitForSeconds(0.05f);
            if (Language.Instance != null)
            {
                string cleaned = Clean(textMeshPro.text);
                index = Language.Instance.languageData.ReturnIndex(cleaned);
            }

            elapsed += 0.05f;
        }

        if (index != -1)
            ApplyText(index);
    }

    private void ApplyText(int idx)
    {
        if (Language.Instance != null)
            textMeshPro.text = Language.Instance.GetWord(idx);
    }

    private string Clean(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        // Simple cleanup (no regex for better performance)
        return text.Replace(" ", "")
                   .Replace("\u200B", "")
                   .Replace("\u200C", "")
                   .Replace("\u200D", "")
                   .Replace("\uFEFF", "")
                   .ToLowerInvariant();
    }

    private void OnUserUpdateLang()
    {
        if (index != -1)
            ApplyText(index);
        else
            TryInitializeText();
    }
}
