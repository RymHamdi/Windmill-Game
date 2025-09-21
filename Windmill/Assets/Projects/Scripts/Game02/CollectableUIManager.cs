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

    public int totalItems = 5;
    private int collectedItems = 0;

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

        if (id > collectedItems + 1)
        {
            if (collectedItems > 1)
            {
                ScoreManager.Instance.ShakeCamera(1);
                Globaleffect.Instance.PlayEffect(EffestType.Bad);
            }
            collectedItems = 0;
            ScoreManager.Instance.ResetScore();
            OnResetItems?.Invoke();
            return false;
        }

        if (id <= 0)
        {
            if (collectedItems > 1)
            {
                ScoreManager.Instance.ShakeCamera(1);
                Globaleffect.Instance.PlayEffect(EffestType.Bad);
            }
            OnResetItems?.Invoke();
            collectedItems = 0;
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
            Debug.Log($"CollectItem ID: {id}, Result: {check}, CollectedItems: {collectedItems}");
            if (!check)
            {
                ScoreManager.Instance.ResetScore();
                if (collectedItems > 1)
                {
                    ScoreManager.Instance.ShakeCamera(1);
                    Globaleffect.Instance.PlayEffect(EffestType.Bad);
                }
                collectedItems = 0;
                OnResetItems?.Invoke();
                return false;
            }
            else
            {
                if (videoRoutine != null)
                {
                    StopCoroutine(videoRoutine);
                }

                // Start a new one
                videoRoutine = StartCoroutine(ShowVideoObjectTemporarily(2f));
                collectedItems++;
                ScoreManager.Instance.AddScore(10 * collectedItems);

                if (collectedItems >= totalItems)
                {
                    OnAllItemsCollected?.Invoke();
                    Debug.Log("All items collected! You win!");
                    if (!isFinished) isFinished = true;
                    StartCoroutine(LetsCollectAgain());
                    // Trigger win condition here
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
        collectedItems = 0;
        OnResetItems?.Invoke();
    }

    IEnumerator ShowVideoObjectTemporarily(float duration)
    {
        if (videoPlayer != null)
        {
            // Make sure it starts fresh
            videoPlayer.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            videoPlayer.gameObject.SetActive(false);
        }
    }
}
