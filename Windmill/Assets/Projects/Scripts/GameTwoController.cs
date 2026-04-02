using UnityEngine;

public class GameTwoController : MonoBehaviour
{
   public GameObject timeLine;
   public GameObject Spawner;
   public GameObject TouchBlade;
   public GameObject Timer;
   public GameObject GlobalEffect;

   private void ActivateTimeLine()
    {
        timeLine.SetActive(true);
    }

    private void ActivateOtherObject()
    {
        timeLine.SetActive(false);
        Spawner.SetActive(true);
        TouchBlade.SetActive(true);
        Timer.SetActive(true);
        GlobalEffect.SetActive(true);
        
    }

    void OnEnable()
    {
        Invoke("ActivateTimeLine", 0.5f);
        Invoke("ActivateOtherObject", 18);
    }
}
