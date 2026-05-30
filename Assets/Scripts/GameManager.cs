using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [NonSerialized]
    public int nextScene = 1;
    
    void Start()
    {
        instance = this;
        nextScene = 1;
        DontDestroyOnLoad(gameObject);
        LoadNextScene();
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextScene);
    }
}
