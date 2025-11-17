using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


public class CollectableUIManager : MonoBehaviour
{
    public static CollectableUIManager Instance;
    public Func<int, bool> OnCollectItem;

    public Action OnAllItemsCollected;
    public Action OnResetItems;

    public Action<int> OnCollectedItemCountChanged;


    public int totalItems = 5;
    private int collectedItems = 0;

    public int CollectedItems
    {
        get { return collectedItems; }
        set
        {
            collectedItems = value;
            OnCollectedItemCountChanged?.Invoke(collectedItems);
        }
    }

    private bool isFinished = false;

    public List<CollectableUIItem> items;

    public VideoPlayer videoPlayer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool CollectItem(int id)
    {
        if (isFinished)
        {

            return true;
        }

        if (id > CollectedItems + 1)
        {
            if (CollectedItems > 1)
            {
                ScoreManager.Instance.ShakeCamera(1);
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.Play("Bad");
                }
                Globaleffect.Instance.PlayEffect(EffestType.Bad);
            }
            CollectedItems = 0;
            ScoreManager.Instance.ResetScore();
            OnResetItems?.Invoke();
            return false;
        }

        if (id <= 0)
        {
            if (CollectedItems > 1)
            {
                ScoreManager.Instance.ShakeCamera(1);
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.Play("Bad");
                }
                Globaleffect.Instance.PlayEffect(EffestType.Bad);
            }
            OnResetItems?.Invoke();
            CollectedItems = 0;
            ScoreManager.Instance.ResetScore();
            return false;
        }
        if (OnCollectItem != null)
        {
            bool check = false;
            foreach (var item in items)
            {
                if (item.id == id)
                {
                    check = item.SetCollected(id);
                    break;
                }
            }
            if (!check)
            {
                ScoreManager.Instance.ResetScore();
                if (CollectedItems > 1)
                {
                    ScoreManager.Instance.ShakeCamera(1);
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.Play("Bad");
                    }
                    Globaleffect.Instance.PlayEffect(EffestType.Bad);
                }
                CollectedItems = 0;
                OnResetItems?.Invoke();
                return false;
            }
            else
            {
                if (videoRoutine != null)
                {
                    StopCoroutine(videoRoutine);
                    videoRoutine = null;
                }

                videoRoutine = StartCoroutine(ShowVideoObjectTemporarily(2.3f));

                CollectedItems++;
                ScoreManager.Instance.AddScore(10 * CollectedItems);
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.Play("Cheers");
                }
                if (CollectedItems >= totalItems)
                {
                    OnAllItemsCollected?.Invoke();
                    if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.Play("Items Collected");
                }
                    Debug.Log("All items collected! You win!");
                    if (!isFinished) isFinished = true;
                    StartCoroutine(LetsCollectAgain());
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.Play("Good");
                    }
                    ShowControlTrigger.Instance?.SendTrigger("GameTwoCollectallItems");
                    // Trigger win condition here
                }
                else
                {
                    ShowControlTrigger.Instance?.SendTrigger("GameTwoCollectNewItem");
                }
                return true;
            }
        }
        return true;
    }

    private Coroutine videoRoutine;

    IEnumerator LetsCollectAgain()
    {
        yield return new WaitForSeconds(.5f);
        ScoreManager.Instance.ResetScore();
        isFinished = false;
        CollectedItems = 0;
        OnResetItems?.Invoke();
    }

    IEnumerator ShowVideoObjectTemporarily(float duration)
    {
        Debug.Log("Coroutine STARTED");

        if (videoPlayer != null)
        {
            videoPlayer.gameObject.SetActive(true);
            Debug.Log("Video ON");
            yield return new WaitForSeconds(duration);
            videoPlayer.gameObject.SetActive(false);
            Debug.Log("Video OFF");
            videoRoutine = null;
        }
    }
}
