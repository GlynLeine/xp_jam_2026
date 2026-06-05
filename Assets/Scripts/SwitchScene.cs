using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    public int targetSceneBuildIndex; 
    public BlackScreen blackScreen;
    
    public void SwitchActiveScene()
    {
        GameManager.instance.isPaused = false;
        blackScreen.onFadeFinished = ()=> SceneManager.LoadScene(targetSceneBuildIndex);
        blackScreen.StartFade();
    }
}
