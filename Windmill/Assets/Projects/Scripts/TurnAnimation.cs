using UnityEngine;

public class TurnAnimation : MonoBehaviour
{
    public LanguageSetup languageSetup;
    void OnEnable()
    {
        languageSetup.UpdateImageVisibility(false);
    }

    void OnDisable()
    {
        languageSetup.UpdateImageVisibility(true);
    }
}
