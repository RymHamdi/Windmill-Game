using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System;

public class SecretLanguageManager : MonoBehaviour
{
    public static SecretLanguageManager Instance;

    //public Transform formUiParent;
    //public FormModellUI formModellPrefab;

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
    public Image ClothChild;
    public GameObject ToggleBtn;
    public TextMeshProUGUI infoText;
    //public ScrollRect scrollRect;

    public float currentDividerangle = 180;

    bool isroundCompleted;

    public List<BladesController> bladesControllers;
    private BladesController currentBladesController;
    public bool IsCloth = true;
    public Action<bool> OnClothChange;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void DisbaleAllBladesController()
    {
        foreach (var item in bladesControllers)
        {
            item.gameObject.SetActive(false);
        }
    }

    public void StartRound(int roundIndex)
    {
        currentBladeImage.enabled = false;
        ClothChild.gameObject.SetActive(false);
        //ToggleBtn.gameObject.SetActive(false);
        currentRoundIndex = roundIndex;
        int totalModelsNeeded = 0;
        DisbaleAllBladesController();
        //RectTransform rt = formUiParent.GetComponent<RectTransform>();
        if (currentRoundIndex == 1)
        {
            /*rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);*/

            totalModelsNeeded = 3;
            saveTime = 8;
            timeToResolve = 5;
            currentBladesController = bladesControllers[0];
            currentBladesController.gameObject.SetActive(true);
        }
        else if (currentRoundIndex == 2)
        {
            /*rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);*/
            totalModelsNeeded = 5;
            saveTime = 15;
            timeToResolve = 5;
            currentBladesController = bladesControllers[1];
            currentBladesController.gameObject.SetActive(true);
        }
        else if (currentRoundIndex == 3)
        {
            /*rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);*/
            totalModelsNeeded = 7;
            saveTime = 25;
            timeToResolve = 5;
            currentBladesController = bladesControllers[2];
            currentBladesController.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("All rounds completed!");
            StartCoroutine(ShowLeaderboardAfterDelay(1));
            //ToDo Show Leaderboard
            // Handle end of game logic here
            return;
        }
        modelIndex = 0;
        for (int i = 0; i < totalModelsNeeded; i++)
        {
            PrepareFormModellUI(1, i);
        }
        //scrollRect.horizontalNormalizedPosition = 0;
        currentRoundPropSync = null;
        StartCoroutine(StartPlayinngRoundAfterDelay(saveTime));
    }

    public void OnClickOnClothChange()
    {
        IsCloth = !IsCloth;
        OnClothChange?.Invoke(IsCloth);
    }
    public GameObject leaderboard;
    IEnumerator ShowLeaderboardAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        leaderboard.SetActive(true);
    }

    IEnumerator StartPlayinngRoundAfterDelay(float delay)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ShowControlTrigger.Instance?.SendTrigger("GameThreeStartMemorize");
        }
        for (int i = (int)delay; i > 0; i--)
        {
            //Todo @Rim fix text
            switch (Language.Instance.languageData.currentLangState)
            {
                case LangState.English:
                    infoText.text = $"Memorize the secret message: {i} seconds";
                    break;

                case LangState.Frensh:
                    infoText.text = $"Mémorisez le message secret : {i} secondes";
                    break;

                case LangState.Netherland:
                    infoText.text = $"Onthoud het geheime bericht: {i} seconden";
                    break;

                case LangState.Germand:
                    infoText.text = $"Merken Sie sich die geheime Nachricht: {i} Sekunden";
                    break;

                case LangState.Spanish:
                    infoText.text = $"Memoriza el mensaje secreto: {i} segundos";
                    break;

                case LangState.Chineese:
                    infoText.text = $"记住秘密信息：{i} 秒";
                    break;

                default:
                    infoText.text = $"Memorize the secret message: {i} seconds";
                    break;
            }

            yield return new WaitForSeconds(1f);
        }
        infoText.text = "";
        currentBladeImage.enabled = true;
        ClothChild.gameObject.SetActive(true);
        ToggleBtn.gameObject.SetActive(true);
        StartPlayerRound(true);
    }

    public void StartPlayerRound(bool needToclear)
    {
        if (needToclear)
        {
            ClearFormModellUI();
        }
        StartCoroutine(RunCrono());
        //scrollRect.horizontal = false;
        currentRoundPropSync = currentUsedRoundPropSyncs[modelIndex];
        currentBladeImage.sprite = currentRoundPropSync.bigIconSprite;
        bladeMouseRotator.EnableRotation();
        bladeMouseRotator.ResetRotation();
    }

    IEnumerator RunCrono()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ShowControlTrigger.Instance?.SendTrigger("GameThreePlaceBlade");
        }
        for (int i = (int)timeToResolve; i >= 0; i--)
        {
            switch (Language.Instance.languageData.currentLangState)
            {
                case LangState.English:
                    infoText.text = $"Place the blade in the right position in: {i} seconds";
                    break;

                case LangState.Frensh:
                    infoText.text = $"Placez la lame dans la bonne position dans : {i} secondes";
                    break;

                case LangState.Netherland:
                    infoText.text = $"Plaats het blad in de juiste positie over: {i} seconden";
                    break;

                case LangState.Germand:
                    infoText.text = $"Bringen Sie die Klinge in die richtige Position in: {i} Sekunden";
                    break;

                case LangState.Spanish:
                    infoText.text = $"Coloca la hoja en la posición correcta en: {i} segundos";
                    break;

                case LangState.Chineese:
                    infoText.text = $"在 {i} 秒内将刀片放在正确的位置";
                    break;

                default:
                    infoText.text = $"Place the blade in the right position in: {i} seconds";
                    break;
            }

            yield return new WaitForSeconds(1f);
        }

        switch (Language.Instance.languageData.currentLangState)
        {
            case LangState.English:
                infoText.text = "Time is over!";
                break;

            case LangState.Frensh:
                infoText.text = "Le temps est écoulé !";
                break;

            case LangState.Netherland:
                infoText.text = "De tijd is om!";
                break;

            case LangState.Germand:
                infoText.text = "Die Zeit ist vorbei!";
                break;

            case LangState.Spanish:
                infoText.text = "¡Se acabó el tiempo!";
                break;

            case LangState.Chineese:
                infoText.text = "时间到了！";
                break;

            default:
                infoText.text = "Time is over!";
                break;
        }


        bladeMouseRotator.DisableRotation();
        modelIndex++;
        if (modelIndex == currentUsedRoundPropSyncs.Count)
        {
            isroundCompleted = true;
        }

        if (modelIndex > currentBladesController.GetActivatedImageCount() && modelIndex <= currentUsedRoundPropSyncs.Count)
        {
            //ToDo @Rim Here the timer complete and is instatited alone
            InstantiteModelUI(bladeMouseRotator.GetCurrentRotation());
            //StartCoroutine(UpdateScrollView());
        }


        if (isroundCompleted)
        {
            //Todo @Rim fix text
            if (PhotonNetwork.IsMasterClient)
            {
                ShowControlTrigger.Instance?.SendTrigger("GameThreeRoundCompleted");
            }

            switch (Language.Instance.languageData.currentLangState)
            {
                case LangState.English:
                    infoText.text = "Round Completed!";
                    break;

                case LangState.Frensh:
                    infoText.text = "Manche terminée !";
                    break;

                case LangState.Netherland:
                    infoText.text = "Ronde voltooid!";
                    break;

                case LangState.Germand:
                    infoText.text = "Runde abgeschlossen!";
                    break;

                case LangState.Spanish:
                    infoText.text = "¡Ronda completada!";
                    break;

                case LangState.Chineese:
                    infoText.text = "回合完成！";
                    break;

                default:
                    infoText.text = "Round Completed!";
                    break;
            }



            StartCoroutine(NextRoundAfterDelay(5));
        }
        else
        {
            StartPlayerRound(false);
        }

    }

    private void ClearFormModellUI()
    {

        currentBladesController.ResetBlades();
    }

    public void ValidateBlade(float rotationZ)
    {
        if (isroundCompleted) return;

        if (currentRoundPropSync == null)
        {
            
            return;
        }
        /* if (scrollRect.horizontalNormalizedPosition < 0)
         {
             scrollRect.horizontalNormalizedPosition = 0;
         }*/
        InstantiteModelUI(rotationZ);
        ///StartCoroutine(UpdateScrollView());

    }
    [Header("Feedback FX")]
    public GameObject goodSpriteFXPrefab;
    public GameObject badSpriteFXPrefab;
    public Transform fxParent;
    private void InstantiteModelUI(float rotationZ)
    {
        FormModellUI newFormModellUI = currentBladesController.GetNextOneNeedToBeUpdated();
        float rightRotation = currentRoundPropSync.randomSecretLanguageRoundProps[0].rightRotation;
        if (rotationZ == rightRotation)
        {
            //Todo @Rim You can Play Good Effect
            Instantiate(goodSpriteFXPrefab, fxParent);
            //ToDo @Hakim you need to start woirking on the score
            ScoreManager.Instance.AddScore(10);
            newFormModellUI.InitModellUI(currentRoundPropSync.iconSprite, FormModellState.Right, currentRoundPropSync.randomSecretLanguageRoundProps[0].rightRotation);
        }
        else if (rotationZ >= rightRotation - 15f && rotationZ <= rightRotation + 15f)
        {
            //Todo @Rim You can Play almost good  Effect
            Instantiate(goodSpriteFXPrefab, fxParent);
            ScoreManager.Instance.AddScore(5);
            //ToDo @Hakim you need to start woirking on the score
            newFormModellUI.InitModellUI(currentRoundPropSync.iconSprite, FormModellState.Right, currentRoundPropSync.randomSecretLanguageRoundProps[0].rightRotation);

        }

        else
        {
            //Todo @Rim You can Play Bad Effect
            Instantiate(badSpriteFXPrefab, fxParent);
            newFormModellUI.InitModellUI(currentRoundPropSync.iconSprite, FormModellState.Wrong, currentRoundPropSync.randomSecretLanguageRoundProps[0].rightRotation);
        }
    }

    private IEnumerator UpdateScrollView()
    {
        WaitForSeconds waitShort = new WaitForSeconds(0.1f);
        yield return waitShort;
        WaitForSeconds waitFrame = new WaitForSeconds(Time.deltaTime);
        /*while (scrollRect.horizontalNormalizedPosition < 1)
        {
            scrollRect.horizontalNormalizedPosition += Time.deltaTime * 10;
            yield return waitFrame;
            yield return null;
        }*/
    }

    IEnumerator NextRoundAfterDelay(float delay)
    {
        StopCoroutine(RunCrono());
        yield return new WaitForSeconds(0.1f);
        isroundCompleted = false;
        yield return new WaitForSeconds(delay);
        //scrollRect.horizontal = true;
        currentBladeImage.enabled = false;
        ClothChild.gameObject.SetActive(false);
        //ToggleBtn.gameObject.SetActive(false);
        currentUsedRoundPropSyncs.Clear();
        //ClearFormModellUI();
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
        if (currentRoundPropSync == null || currentBladesController == null)
        {
            return;
        }

        // Instantiate new UI elements based on the selected round props
        foreach (var prop in currentRoundPropSync.randomSecretLanguageRoundProps)
        {
            //FormModellUI newFormModellUI = Instantiate(formModellPrefab, formUiParent);
            //newFormModellUI.InitModellUI(currentRoundPropSync.iconSprite, FormModellState.Normal, prop.rightRotation);
            FormModellUI formModellUI = currentBladesController.bladeSlots[index].formModellUI;
            if (formModellUI != null)
            {
                formModellUI.InitModellUI(currentRoundPropSync.iconSprite, FormModellState.Normal, prop.rightRotation);
            }
        }

    }

    public float GetCanonicalBladeAngle(float rotationZ)
    {
        currentDividerangle = currentRoundPropSync.divider;
        // Normalize the angle to be within -360 to 360
        rotationZ = Mathf.Repeat(rotationZ, 360f);

        // Map the angle to the range [0, 180] to match the "X" design symmetry
        float canonicalAngle = Mathf.Abs(rotationZ % currentDividerangle);
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

    public List<float> newAngles = new List<float> { 5, 42, 72, 102, 130 };

    public float GetClosestCanonicalAngle(float canonicalAngle)
    {

        float closestAngle = newAngles[0];
        float smallestDifference = Mathf.Abs(canonicalAngle - closestAngle);

        foreach (float angle in newAngles)
        {
            float difference = Mathf.Abs(canonicalAngle - angle);
            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                closestAngle = angle;
            }
        }
        
        return closestAngle;
    }
}
