using UnityEngine;
using UnityEngine.UI;

public class CharacterDetailsManager : MonoBehaviour
{
    public GameObject[] detailPanels; 
    private int currentIndex = -1;

    public GameObject P1;
    public GameObject P2;

    public Button[] buttons;          
    public Sprite normalSprite;      
    public Sprite selectedSprite;

    void Start()
    {
        // Hide all panels first
        for (int i = 0; i < detailPanels.Length; i++)
        {
            detailPanels[i].SetActive(false);
        }
        ResetAllButtons();
        // Show the first one by default
        if (detailPanels.Length > 0)
        {
            detailPanels[0].SetActive(true);
            buttons[0].image.sprite = selectedSprite;
            currentIndex = 0;
        }
    }

    public void ShowDetails(int index)
    {
        // Hide currently shown panel
        if (currentIndex >= 0)
            detailPanels[currentIndex].SetActive(false);

        // Show the clicked one
        detailPanels[index].SetActive(true);
        currentIndex = index;

        ResetAllButtons();
        buttons[index].image.sprite = selectedSprite;
    }

    public void NextPanel()
    {
        P1.SetActive(false);
        P2.SetActive(true);
    }

    private void ResetAllButtons()
    {
        foreach (Button btn in buttons)
        {
            btn.image.sprite = normalSprite;
        }
    }
}
