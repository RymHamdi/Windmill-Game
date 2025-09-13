using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using DG.Tweening;
using UnityEngine.Video;
using System;

public class ScoreManager : MonoBehaviourPunCallbacks
{
    public static ScoreManager Instance; // Singleton for easy access

    [Header("UI")]
    public TMP_Text scoreText;

    private int score = 0;
    public string ScoreKey = "Score";

    [Header("Bingo Effect")]
    public GameObject BingoEffect;
    public float BingoEffectDuration = 1.0f;
    public float timeIntervalToGetBingo = 1.5f;
    public int notesToGetBingo = 3;

    private int notesHitInInterval = 0;
    private float lastNoteHitTime = -Mathf.Infinity;

    public VideoPlayer gameVideo;


    void Awake()
    {
        // Singleton setup
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        lastNoteHitTime = Time.time;
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(ScoreKey))
            score = (int)PhotonNetwork.LocalPlayer.CustomProperties[ScoreKey];
        else
        {
            score = 0;
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props[ScoreKey] = score;
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        float currentTime = Time.time;

        // Check interval
        if (currentTime - lastNoteHitTime <= timeIntervalToGetBingo)
        {
            notesHitInInterval++;
        }
        else
        {
            // Reset streak
            notesHitInInterval = 1;
        }

        // Update last hit time every note
        lastNoteHitTime = currentTime;

        // Check Bingo
        if (notesHitInInterval >= notesToGetBingo)
        {
            TriggerBingo();
            notesHitInInterval = 0; // reset after bingo
        }

        // --- Store old score before update
        int oldScore = score;

        // Update score
        score += amount;
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props[ScoreKey] = score;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        UpdateScoreUI();

        // --- Debug check for 100 milestones
        if (gameVideo != null)
        {
            if ((score / 150) > (oldScore / 150))
            {
                UpdateVideoSpeed();
            }
        }

    }

    public void ResetScore()
    {
        score = 0;
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props[ScoreKey] = score;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        UpdateScoreUI();
    }

    private void UpdateVideoSpeed()
    {
        gameVideo.playbackSpeed -= 0.05f;
    }

    private void TriggerBingo()
    {
        if (BingoEffect != null)
        {
            // Reset if already active
            CancelInvoke(nameof(DisableBingoEffect));
            BingoEffect.SetActive(false);
            BingoEffect.SetActive(true);
            StartCoroutine(PlayScoreTexteffect());
            Invoke(nameof(DisableBingoEffect), BingoEffectDuration);
        }
    }

    public IEnumerator PlayScoreTexteffect()
    {
        ShakeCamera(0.55f);
        for (int i = 0; i < 3; i++)
        {
            scoreText.transform.DOScale(1.2f, 0.1f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.1f);
            score += 1;
            UpdateScoreUI();
            scoreText.transform.DOScale(1.0f, 0.1f).SetEase(Ease.InBack);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ShakeCamera(float duration)
    {
        Camera.main.transform.DOShakePosition(duration, 0.2f, 10, 90, false, true);
    }

    private void DisableBingoEffect()
    {
        if (BingoEffect != null)
            BingoEffect.SetActive(false);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = score.ToString() + "X";
    }
}
