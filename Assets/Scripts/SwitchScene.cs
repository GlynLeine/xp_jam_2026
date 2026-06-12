using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    public int targetSceneBuildIndex; 
    public BlackScreen blackScreen;
    
    public void SwitchActiveScene()
    {
        GameManager.instance.isPaused = false;
        blackScreen.FadeIn(() => GameManager.instance.StartLoadingScene(targetSceneBuildIndex));
    }
}
