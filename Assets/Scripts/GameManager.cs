using cherrydev;
using NUnit.Framework;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public DialogNodeGraph[] AntonyNodes;
    public DialogNodeGraph[] SamanthaNodes;
    public DialogNodeGraph[] JamesNodes;
    public DialogNodeGraph[] PhoebeNodes;
    [SerializeField] DialogBehaviour dialogBehaviour;
    public static GameManager instance;
    int i = 0;
    
    [NonSerialized]
    public int nextScene = 1;
    
    [NonSerialized]
    public bool succeededSeason;
    
    void Start()
    {
        instance = this;
        nextScene = 1;
        DontDestroyOnLoad(gameObject);
        LoadNextScene();
    }


    public void startDialogue()
    {
        if(i <= 3)
        {
            dialogBehaviour.StartDialog(AntonyNodes[i]);
            i++;
        }
        if(i <= 7)
        {
            dialogBehaviour.StartDialog(SamanthaNodes[(i-4)]);
            i++;
        }
        if(i <= 11)
        {
            dialogBehaviour.StartDialog(JamesNodes[(i-8)]);
            i++;
        }
        if (i <= 15)
        {
            dialogBehaviour.StartDialog(PhoebeNodes[(i-12)]);
            i++;
        }
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextScene);
    }
}
