using UnityEngine;

public class SwitchNextScene : MonoBehaviour
{
    public BlackScreen blackScreen;

    public void SwitchActiveScene()
    {
        blackScreen.onFadeFinished = GameManager.instance.LoadNextScene;
        blackScreen.StartFade();
    }
}