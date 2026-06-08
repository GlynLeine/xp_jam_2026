using UnityEngine;

public class Quit : MonoBehaviour
{
    private void Start()
    {
#if UNITY_WEBGL
            gameObject.SetActive(false);
#endif
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}