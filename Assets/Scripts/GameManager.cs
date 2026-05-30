using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    void Start()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        GetComponent<SwitchScene>().SwitchActiveScene();
    }
}
