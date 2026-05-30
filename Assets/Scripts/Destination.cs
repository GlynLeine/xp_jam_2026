using UnityEngine;
using UnityEngine.SceneManagement;

public class Destination : MonoBehaviour
{
    public bool isStart;
    public Transform spawnLocation;

    public void EndScene()
    {
        GameManager.instance.nextScene = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(2);
    }
}
