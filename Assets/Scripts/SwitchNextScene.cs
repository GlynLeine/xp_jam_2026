using UnityEngine;

public class SwitchNextScene : MonoBehaviour
{
    public BlackScreen blackScreen;

    public void SwitchActiveScene()
    {
        blackScreen.FadeIn(GameManager.instance.StartLoadingNextScene);
    }
}