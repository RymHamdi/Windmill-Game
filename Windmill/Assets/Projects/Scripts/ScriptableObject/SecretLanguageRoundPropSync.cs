using System.Collections.Generic;
using PlayFab.AuthenticationModels;
using UnityEngine;

[CreateAssetMenu(fileName = "SecretLanguageRoundPropSync", menuName = "Scriptable Objects/SecretLanguageRoundPropSync")]
public class SecretLanguageRoundPropSync : ScriptableObject
{
    public int itemId;
    public Sprite iconSprite;
    public Sprite bigIconSprite;
    public List<RandomSecretLanguageRoundProp> randomSecretLanguageRoundProps;

    public List<RandomSecretLanguageRoundProp> GetRandomSecretLanguageRoundProps(int count)
    {
        List<RandomSecretLanguageRoundProp> selectedProps = new List<RandomSecretLanguageRoundProp>();
        List<int> usedIndices = new List<int>();

        if (count > randomSecretLanguageRoundProps.Count)
        {
            Debug.LogWarning("Requested count exceeds available props. Returning all available props.");
            return new List<RandomSecretLanguageRoundProp>(randomSecretLanguageRoundProps);
        }

        while (selectedProps.Count < count)
        {
            int randomIndex = Random.Range(0, randomSecretLanguageRoundProps.Count);
            if (!usedIndices.Contains(randomIndex))
            {
                usedIndices.Add(randomIndex);
                selectedProps.Add(randomSecretLanguageRoundProps[randomIndex]);
            }
        }

        return selectedProps;
    }

    public SecretLanguageRoundPropSync CreateCopy()
    {
        SecretLanguageRoundPropSync copy = ScriptableObject.CreateInstance<SecretLanguageRoundPropSync>();
        copy.itemId = this.itemId;
        copy.iconSprite = this.iconSprite;
        copy.bigIconSprite = this.bigIconSprite;
        copy.randomSecretLanguageRoundProps = new List<RandomSecretLanguageRoundProp>(this.randomSecretLanguageRoundProps);
        return copy;
    }
}

[System.Serializable]

public struct RandomSecretLanguageRoundProp
{
    public int itemRandomId;
    public int startRotation;

    public int rightRotation;

}
