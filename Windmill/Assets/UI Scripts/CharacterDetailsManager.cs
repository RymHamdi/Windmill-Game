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
    public GameObject SelectCharacterText;

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
        ShowDetails(0);
        SelectCharacterText.SetActive(true);
    }

    public void ShowDetails(int index)
    {
        // Hide currently shown panel
        if (currentIndex >= 0)
            detailPanels[currentIndex].SetActive(false);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.clickSound);
        // Show the clicked one
        detailPanels[index].SetActive(true);
        currentIndex = index;
        ResetAllButtons();
        buttons[index].image.sprite = selectedSprite;
        LobbyManager.Instance.localPlayerindex = index;
        LobbyManager.Instance.UpdateLocalPlayerCharacter();
        ShowControlTrigger.Instance?.SendTrigger("UpdateCharacterSelection");
       
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
