using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{
    public int targetSceneBuildIndex; 
    
    public void SwitchActiveScene()
    {
        SceneManager.LoadScene(targetSceneBuildIndex);
    }
}
