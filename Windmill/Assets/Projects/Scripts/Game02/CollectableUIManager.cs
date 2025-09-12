using System;
using System.Collections.Generic;
using UnityEngine;

public class CollectableUIManager : MonoBehaviour
{
    public static CollectableUIManager Instance;
    public Func<int, bool> OnCollectItem;
    public Action OnResetItems;

    public int totalItems = 5;
    private int collectedItems = 0;

    private bool isFinished = false;

    public List<CollectableUIItem> items;

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
                collectedItems++;

                if (collectedItems >= totalItems)
                {
                    Debug.Log("All items collected! You win!");
                    if (!isFinished) isFinished = true;
                    // Trigger win condition here
                }
                return true;
            }
        }
        return true;
    }
}
