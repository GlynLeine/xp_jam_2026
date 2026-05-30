using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSeasonButton : MonoBehaviour
{
    private void Awake()
    {
        if (GameManager.instance.nextScene >= SceneManager.sceneCountInBuildSettings)
        {
            gameObject.SetActive(false);
        }
    }
}
