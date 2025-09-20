using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SecretLanguageManager : MonoBehaviour
{
    public static SecretLanguageManager Instance;

    public Transform formUiParent;
    public FormModellUI formModellPrefab;

    public List<SecretLanguageRoundPropSync> secretLanguageRoundPropSyncs;
    private SecretLanguageRoundPropSync currentRoundPropSync;

    private int currentRoundIndex = 0;
    private int modelIndex = 0;
    public List<SecretLanguageRoundPropSync> currentUsedRoundPropSyncs = new List<SecretLanguageRoundPropSync>();
    public List<SecretLanguageRoundPropSync> fullUsedRoundPropSyncs = new List<SecretLanguageRoundPropSync>();
    public BladeMouseRotator bladeMouseRotator;

    public float saveTime;
    public float timeToResolve;

    public Image currentBladeImage;
    public TextMeshProUGUI infoText;
    public ScrollRect scrollRect;

    bool isroundCompleted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void StartRound(int roundIndex)
    {
        currentBladeImage.enabled = false;
        currentRoundIndex = roundIndex;
        int totalModelsNeeded = 0;
        RectTransform rt = formUiParent.GetComponent<RectTransform>();
        if (currentRoundIndex == 1)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            totalModelsNeeded = 3;
            saveTime = 8;
            timeToResolve = 5;
        }
        else if (currentRoundIndex == 2)
        {
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            totalModelsNeeded = 5;
            saveTime = 15;
            timeToResolve = 5;
        }
        else if (currentRoundIndex == 3)
        {
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            totalModelsNeeded = 7;
            saveTime = 25;
            timeToResolve = 5;
        }
        else
        {
            Debug.Log("All rounds completed!");
            //ToDo Show Leaderboard
            // Handle end of game logic here
            return;
        }
        modelIndex = 0;
        for (int i = 0; i < totalModelsNeeded; i++)
        {
            PrepareFormModellUI(1, i);
        }
        scrollRect.horizontalNormalizedPosition = -4;
        currentRoundPropSync = null;
        StartCoroutine(StartPlayinngRoundAfterDelay(saveTime));
    }

    IEnumerator StartPlayinngRoundAfterDelay(float delay)
    {
        for (int i = (int)delay; i > 0; i--)
        {
            //Todo @Rim fix text
            infoText.text = $"Try to save all image indicators before: {i} seconds";
            yield return new WaitForSeconds(1f);
        }
        infoText.text = "";
        currentBladeImage.enabled = true;
        StartPlayerRound(true);
    }

    public void StartPlayerRound(bool needToclear)
    {
        if (needToclear)
        {
            ClearFormModellUI();
        }
        StartCoroutine(RunCrono());
        scrollRect.horizontal = false;
        currentRoundPropSync = currentUsedRoundPropSyncs[modelIndex];
        currentBladeImage.sprite = currentRoundPropSync.bigIconSprite;
        bladeMouseRotator.EnableRotation();
        bladeMouseRotator.ResetRotation();
    }

    IEnumerator RunCrono()
    {
        for (int i = (int)timeToResolve; i >= 0; i--)
        {
            //Todo @Rim fix text
            infoText.text = $"You need to resolve blade in: {i} seconds";
            yield return new WaitForSeconds(1f);
        }

        //Todo @Rim fix text
        infoText.text = "Time is over!";
        bladeMouseRotator.DisableRotation();
        modelIndex++;
        if (modelIndex == currentUsedRoundPropSyncs.Count)
        {
            isroundCompleted = true;
        }

        if (modelIndex > formUiParent.childCount && modelIndex <= currentUsedRoundPropSyncs.Count)
        {
            //ToDo @Rim Here the timer complete and is instatited alone
            InstantiteModelUI(bladeMouseRotator.GetCurrentRotation());
            scrollRect.horizontalNormalizedPosition = 1f;
        }


        if (isroundCompleted)
        {
            //Todo @Rim fix text
            infoText.text = "Round Completed!";
            StartCoroutine(NextRoundAfterDelay(5));
        }
        else
        {
            StartPlayerRound(false);
        }

    }

    private void ClearFormModellUI()
    {
        foreach (Transform child in formUiParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void ValidateBlade(float rotationZ)
    {
        if (isroundCompleted) return;

        if (currentRoundPropSync == null)
        {
            Debug.LogWarning("No current round prop sync selected.");
            return;
        }
        if (scrollRect.horizontalNormalizedPosition < 0)
        {
            scrollRect.horizontalNormalizedPosition = 0;
        }
        InstantiteModelUI(rotationZ);
        scrollRect.horizontalNormalizedPosition = 1f;

    }

    private void InstantiteModelUI(float rotationZ)
    {
        FormModellUI newFormModellUI = Instantiate(formModellPrefab, formUiParent);
        float rightRotation = currentRoundPropSync.randomSecretLanguageRoundProps[0].rightRotation;
        if (rotationZ == rightRotation)
        {
            //Todo @Rim You can Play Good Effect
            //ToDo @Hakim you need to start woirking on the score
            newFormModellUI.InitModellUI(currentRoundPropSync.iconSprite, FormModellState.Right, currentRoundPropSync.randomSecretLanguageRoundProps[0].rightRotation);
        }
        else if (rotationZ >= rightRotation - 15f && rotationZ <= rightRotation + 15f)
        {
            //Todo @Rim You can Play almost good  Effect
            //ToDo @Hakim you need to start woirking on the score
            newFormModellUI.InitModellUI(currentRoundPropSync.iconSprite, FormModellState.Right, currentRoundPropSync.randomSecretLanguageRoundProps[0].rightRotation);

        }

        else
        {
            //Todo @Rim You can Play Bad Effect
            newFormModellUI.InitModellUI(currentRoundPropSync.iconSprite, FormModellState.Wrong, currentRoundPropSync.randomSecretLanguageRoundProps[0].rightRotation);
        }
    }

    IEnumerator NextRoundAfterDelay(float delay)
    {
        StopCoroutine(RunCrono());
        yield return new WaitForSeconds(0.1f);
        isroundCompleted = false;
        yield return new WaitForSeconds(delay);
        scrollRect.horizontal = true;
        currentBladeImage.enabled = false;
        currentUsedRoundPropSyncs.Clear();
        ClearFormModellUI();
        currentRoundIndex++;
        modelIndex = 0;
        StartRound(currentRoundIndex);
    }

    private void SelectRandomSecretLanguageRoundPropSync(int roundIndex, int index)
    {
        /*if (secretLanguageRoundPropSyncs.Count == 0)
        {
            Debug.LogWarning("No SecretLanguageRoundPropSyncs available.");
        }

        int randomIndex = Random.Range(0, secretLanguageRoundPropSyncs.Count);
        SecretLanguageRoundPropSync selectedSync = secretLanguageRoundPropSyncs[randomIndex];
        List<RandomSecretLanguageRoundProp> randomSecretLanguageRoundProps = selectedSync.GetRandomSecretLanguageRoundProps(roundIndex);

        currentRoundPropSync = selectedSync.CreateCopy();
        currentRoundPropSync.randomSecretLanguageRoundProps = randomSecretLanguageRoundProps;
        currentUsedRoundPropSyncs.Add(currentRoundPropSync);*/
        if (fullUsedRoundPropSyncs.Count == 0)
        {
            Debug.LogWarning("No fullUsedRoundPropSyncs available.");
            return;
        }

        switch (currentRoundIndex)
        {
            case 1:
                currentUsedRoundPropSyncs = fullUsedRoundPropSyncs.GetRange(0, 3);

                break;
            case 2:
                currentUsedRoundPropSyncs = fullUsedRoundPropSyncs.GetRange(3, 5);
                break;
            case 3:
                currentUsedRoundPropSyncs = fullUsedRoundPropSyncs.GetRange(8, 7);
                break;
            default:
                Debug.LogWarning("Invalid round index.");
                break;
        }
        currentRoundPropSync = currentUsedRoundPropSyncs[index];
    }

    private void PrepareFormModellUI(int roundIndex, int index)
    {

        SelectRandomSecretLanguageRoundPropSync(roundIndex, index);
        if (currentRoundPropSync == null)
        {
            return;
        }

        // Instantiate new UI elements based on the selected round props
        foreach (var prop in currentRoundPropSync.randomSecretLanguageRoundProps)
        {
            FormModellUI newFormModellUI = Instantiate(formModellPrefab, formUiParent);
            newFormModellUI.InitModellUI(currentRoundPropSync.iconSprite, FormModellState.Normal, prop.rightRotation);
        }

    }

    public float GetCanonicalBladeAngle(float rotationZ)
    {
        // Normalize the angle to be within -360 to 360
        rotationZ = Mathf.Repeat(rotationZ, 360f);

        // Map the angle to the range [0, 180] to match the "X" design symmetry
        float canonicalAngle = Mathf.Abs(rotationZ % 180f);
        return canonicalAngle;
    }

    public void SetupWithSyncedSequence(List<SyncedRoundProp> currentSequence)
    {
        foreach (var syncedProp in currentSequence)
        {
            var propSync = secretLanguageRoundPropSyncs.Find(p => p.itemId == syncedProp.itemId);
            if (propSync != null)
            {
                var propCopy = propSync.CreateCopy();
                propCopy.randomSecretLanguageRoundProps = propSync.randomSecretLanguageRoundProps.FindAll(r => r.itemRandomId == syncedProp.itemRandomId);
                if (propCopy.randomSecretLanguageRoundProps.Count > 0)
                {
                    fullUsedRoundPropSyncs.Add(propCopy);
                }
                else
                {
                    Debug.LogWarning($"No matching random props found for itemRandomId: {syncedProp.itemRandomId} in itemId: {syncedProp.itemId}");
                }
            }
            else
            {
                Debug.LogWarning($"No matching SecretLanguageRoundPropSync found for itemId: {syncedProp.itemId}");
            }
        }
    }

    public IEnumerator RunAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartRound(1);
    }
}
