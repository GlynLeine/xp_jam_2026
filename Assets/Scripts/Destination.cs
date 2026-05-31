using UnityEngine;
using UnityEngine.SceneManagement;

public class Destination : MonoBehaviour
{
    public bool isStart;
    public Transform spawnLocation;
    public BlackScreen blackScreen;
    
    public void EndScene()
    {
        GameManager.instance.nextScene = SceneManager.GetActiveScene().buildIndex + 1;
        GameManager.instance.succeededSeason = true;
        blackScreen.onFadeFinished = () => SceneManager.LoadScene(2);
        blackScreen.StartFade();
    }
}
