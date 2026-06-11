using UnityEngine;
using UnityEngine.SceneManagement;

public class Destination : MonoBehaviour
{
    public bool isStart;
    public Transform spawnLocation;
    public BlackScreen blackScreen;
    
    public void EndScene()
    {
        if (GameManager.instance.isPaused)
        {
            return;
        }
        
        GameManager.instance.nextScene = SceneManager.GetActiveScene().buildIndex + 1;
        GameManager.instance.succeededSeason = true;
        blackScreen.FadeIn(() => GameManager.instance.StartLoadingScene(2));
    }
}
