using cherrydev;
using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public DialogNodeGraph[] AntonyNodes;
    public DialogNodeGraph[] SamanthaNodes;
    public DialogNodeGraph[] JamesNodes;
    public DialogNodeGraph[] PhoebeNodes;
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
