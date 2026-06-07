using UnityEngine;

public class Quit : MonoBehaviour
{
    private void Start()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer) 
        {
            gameObject.SetActive(false);
        }
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