using UnityEngine;

public class GameOneController : MonoBehaviour
{
    public GameObject timeLine;
   public GameObject Spawner;


   private void ActivateTimeLine()
    {
        timeLine.SetActive(true);
    }

    private void ActivateOtherObject()
    {
        timeLine.SetActive(false);
        Spawner.SetActive(true);
       
        
    }

    void OnEnable()
    {
        Invoke("ActivateTimeLine", 0.5f);
        Invoke("ActivateOtherObject", 10);
    }
}
