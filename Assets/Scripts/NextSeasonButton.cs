using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSeasonButton : MonoBehaviour
{
    private void Awake()
    {
        if (GameManager.instance == null)
        {
            return;
        }
        
        if (GameManager.instance.nextScene >= SceneManager.sceneCountInBuildSettings)
        {
            gameObject.SetActive(false);
        }

        if (!GameManager.instance.succeededSeason)
        {
            gameObject.GetComponentInChildren<TMPro.TMP_Text>().text = "Retry Season";
        }
    }
}
