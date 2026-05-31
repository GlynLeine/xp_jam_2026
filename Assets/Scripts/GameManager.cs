using cherrydev;
using NUnit.Framework;
using System;
using System.Collections;
using Unity.VisualScripting;
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
    public int dialogIndex = 0;
    
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

    public void startDialogue(DialogBehaviour dialogBehaviour)
    {
        if (!succeededSeason && SceneManager.GetActiveScene().buildIndex == 2)
        {
            return;
        }
        
        if(dialogIndex <= 3)
        {
            dialogBehaviour.StartDialog(AntonyNodes[dialogIndex]);
            dialogIndex++;
            return;
        }
        if(dialogIndex <= 7)
        {
            dialogBehaviour.StartDialog(SamanthaNodes[(dialogIndex-4)]);
            dialogIndex++;
            return;
        }
        if(dialogIndex <= 11)
        {
            dialogBehaviour.StartDialog(JamesNodes[(dialogIndex-8)]);
            dialogIndex++;
            return;
        }
        if (dialogIndex <= 15)
        {
            dialogBehaviour.StartDialog(PhoebeNodes[(dialogIndex-12)]);
            dialogIndex++;
            return;
        }
    }

    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextScene);
    }

}
