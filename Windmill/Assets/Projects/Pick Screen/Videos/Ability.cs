using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Video;
using TMPro;
using DG.Tweening;

public class Ability : MonoBehaviour
{
    public List<AbilityData> abilities;
    public List<StatsBarData> statsBars;

    public GameObject[] statsParent;

    public GameObject langGameObject;
    public TMP_Text descriptionText;
    public TMP_Text titleText;
    public TMP_Text windmillNameText;
    public LanguageSetup languageSetup;
    public VideoPlayer videoPlayer;
    float fadeDuration = 2f;
    bool completewaiting;


    private int GetPlayerData()
    {
        
        if ( PhotonLauncher.Instance == null)
        {
            // let just choose the first screen if no player found
            
            return 0;
        }
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("CharacterId"))
        {
            int pickedScreenIndex = (int)PhotonNetwork.LocalPlayer.CustomProperties["CharacterId"];
            return pickedScreenIndex;
        }
        else
        {
            // default to last ability if no character id found
            int pickedScreenIndex = abilities.Count - 1;
            return pickedScreenIndex;
        }
        
    }

    private void ApplyPlayerData()
    {
        int playerIndex = GetPlayerData();
        AbilityData playerAbility = abilities[playerIndex];
        int index =0;
        descriptionText.text = playerAbility.description;
        windmillNameText.text = playerAbility.windmillName;
        videoPlayer.clip = playerAbility.clip;
        videoPlayer.Prepare();
        
        foreach (var statsBarData in statsBars)
        {
            if (statsBarData.statsBar != null)
            {
                
                statsBarData.statsBar.Value = playerAbility.values[index];
                index++;
            }
        }
         StartCoroutine(LighhtAndPlayVideo(playerIndex));

        
    }
    public CanvasGroup lightCanvasGroup;
    IEnumerator LighhtAndPlayVideo(int index)
    {
        
        
        switch (index)
        {
            case 0: fadeDuration = 0f; break;
            case 1: fadeDuration = 2f; break;
            case 2: fadeDuration = 6f; break;
            case 3: fadeDuration = 8f; break;
            case 4: fadeDuration = 4f; break;
            default: fadeDuration = 0f; break;
        }
        
        videoPlayer.playbackSpeed = 1.0f;
        videoPlayer.Play();
        //yield return new WaitForSeconds(fadeDuration);
        completewaiting = true;
        // Start slow effects before fade
        yield return new WaitForSeconds(fadeDuration);
        
        AnimateTitle();
        StartCoroutine(ActivateStarBar());
        lightCanvasGroup.DOFade(0f,  1f);
        
    }

    private void Update()
    {
        if (completewaiting)
        {
            /*videoPlayer.playbackSpeed += (Time.deltaTime * 0.5f );
                if (videoPlayer.playbackSpeed > 1)
                {
                    videoPlayer.playbackSpeed = 1f;
                }
                DOTween.timeScale += Time.deltaTime;
                if (DOTween.timeScale > 1f)
                {
                    DOTween.timeScale = 1f;
                }*/
        }
    }


    private void AnimateTitle()
    {
        titleText.transform.localScale = Vector3.one;
        titleText.transform.DOScale(new Vector3(1.1f, 1.1f, 1f), 1.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetId(titleText.transform);
    }

    private IEnumerator ActivateStarBar()
    {
        yield return new WaitForSeconds(1f);
        foreach (var item in statsParent)
        {
            if (item != null)
            {
                item.gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    void OnEnable()
    {
        ApplyPlayerData();
        //langGameObject.SetActive(true);
    }
}

[System.Serializable]
public struct AbilityData
{
    public string screenName;
    public VideoClip clip;
    public int[] values;

    public string description;
    public string windmillName;
}

[System.Serializable]

public struct StatsBarData
{
    public StatsBar statsBar;
}
