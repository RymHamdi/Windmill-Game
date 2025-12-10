using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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

    // Sequence enforcement
    private List<int> sequenceIds = new List<int>();
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
        // Prepare sequence of IDs (ascending). This enforces collection order.
        sequenceIds.Clear();
        if (items != null && items.Count > 0)
        {
            foreach (var it in items)
            {
                sequenceIds.Add(it.id);
            }
            sequenceIds.Sort();
            // Ensure totalItems matches items count unless explicitly set
            totalItems = Mathf.Max(totalItems, sequenceIds.Count);
        }
    }

    private Coroutine pendingPenaltyCoroutine;
    private float lastGoodSliceTime;

    public bool CollectItem(int id)
    {
        if (isFinished)
        {
            return true;
        }

        // Check if it's a good slice
        bool isGoodSlice = false;
        // Determine expected id based on current collected count
        int expectedId = -1;
        if (sequenceIds != null && CollectedItems < sequenceIds.Count)
        {
            expectedId = sequenceIds[CollectedItems];
        }
        if (OnCollectItem != null)
        {
            foreach (var item in items)
            {
                if (item.id == id)
                {
                    // If the collected id does not match expected sequence, treat as wrong and reset
                    if (expectedId != -1 && id != expectedId)
                    {
                        // Respect short grace period after a good slice
                        if (Time.time - lastGoodSliceTime <= 0.4f)
                        {
                            return false;
                        }

                        ApplyBadEffects();
                        return false;
                    }

                    if (item.SetCollected(id))
                    {
                        isGoodSlice = true;
                    }
                    break;
                }
            }
        }

        if (isGoodSlice)
        {
            // GOOD SLICE

            // 1. Cancel any pending bad penalty
            if (pendingPenaltyCoroutine != null)
            {
                StopCoroutine(pendingPenaltyCoroutine);
                pendingPenaltyCoroutine = null;
            }

            // 2. Update last good slice time
            lastGoodSliceTime = Time.time;

            // 3. Apply good effects
            ApplyGoodEffects();

            return true;
        }
        else
        {
            // BAD SLICE (Wrong ID or already collected or not in list)

            // 1. Check if we are within the grace period of a good slice
            if (Time.time - lastGoodSliceTime <= 0.2f)
            {
                // Ignore this bad slice
                return false;
            }

            // 2. Schedule penalty
            if (pendingPenaltyCoroutine == null)
            {
                pendingPenaltyCoroutine = StartCoroutine(PendingPenaltyRoutine());
            }

            return false;
        }
    }

    public GameObject GoodJobEffect;
    public Transform content;

    private void ApplyGoodEffects()
    {
        if (videoRoutine != null)
        {
            StopCoroutine(videoRoutine);
            videoRoutine = null;
        }

        videoRoutine = StartCoroutine(ShowVideoObjectTemporarily(2.8f));

        CollectedItems++;
        ScoreManager.Instance.AddScore(10 * CollectedItems);
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play("Cheers");
        }
        if (CollectedItems >= totalItems)
        {
            GameObject GoodJobEffects = Instantiate(GoodJobEffect, content);
            Destroy(GoodJobEffects,2);
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
            if (PhotonLauncher.Instance != null)
            {
                if (PhotonLauncher.Instance.isServer)
                {
                    //ShowControlTrigger.Instance?.SendTrigger("GameTwoCollectallItems");
                }
            }
            
        }
        else
        {
            
            //ShowControlTrigger.Instance?.SendTrigger("GameTwoCollectNewItem");
        }
    }

    private IEnumerator PendingPenaltyRoutine()
    {
        yield return new WaitForSeconds(0.2f);
        ApplyBadEffects();
        pendingPenaltyCoroutine = null;
    }

    private void ApplyBadEffects()
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
