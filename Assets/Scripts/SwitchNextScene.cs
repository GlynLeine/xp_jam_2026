using UnityEngine;

public class SwitchNextScene : MonoBehaviour
{
    public void SwitchActiveScene()
    {
        GameManager.instance.LoadNextScene();
    }
}
