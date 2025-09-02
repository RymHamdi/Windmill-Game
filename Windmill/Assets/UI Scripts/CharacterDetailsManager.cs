using UnityEngine;

public class CharacterDetailsManager : MonoBehaviour
{
    public GameObject[] detailPanels; // Assign your 5 panels here in order
    private int currentIndex = -1;

    public void ShowDetails(int index)
    {
        // Hide currently shown panel
        if (currentIndex >= 0)
            detailPanels[currentIndex].SetActive(false);

        // Show the clicked one
        detailPanels[index].SetActive(true);
        currentIndex = index;
    }
}
