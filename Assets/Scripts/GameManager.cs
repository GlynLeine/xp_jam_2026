using cherrydev;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    // List of Banks to load
    [FMODUnity.BankRef]
    public List<string> banks = new List<string>();
    public GameObject eventEmittersPrefab;
    [NonSerialized]
    public FMODUnity.StudioEventEmitter[] fmodEventEmitters;
    
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
    
    [NonSerialized]
    public int lastSeason;
    
    [NonSerialized]
    public float fogResolution = 1f;
    
    [NonSerialized]
    public int fogStepCount = 100;
    
    [NonSerialized]
    public float combatMusicScalar = 0f;
    
    [NonSerialized]
    public bool isPaused = false;
    
    void Start()
    {
        instance = this;
        nextScene = 1;
        DontDestroyOnLoad(gameObject);
        
        StartCoroutine(LoadGameAsync());
    }

    private IEnumerator LoadGameAsync()
    {
        // Iterate all the Studio Banks and start them loading in the background
        // including the audio sample data
        foreach (var bank in banks)
        {
            FMODUnity.RuntimeManager.LoadBank(bank, true);
        }

        // Keep yielding the co-routine until all the bank loading is done
        // (for platforms with asynchronous bank loading)
        while (!FMODUnity.RuntimeManager.HaveAllBanksLoaded)
        {
            yield return null;
        }

        // Keep yielding the co-routine until all the sample data loading is done
        while (FMODUnity.RuntimeManager.AnySampleDataLoading())
        {
            yield return null;
        }
        
        Instantiate(eventEmittersPrefab, transform);

        fmodEventEmitters = GetComponentsInChildren<FMODUnity.StudioEventEmitter>();

        for (int i = 0; i < fmodEventEmitters.Length; i++)
        {
            fmodEventEmitters[i].Play();
        }

        // Start an asynchronous operation to load the scene
        AsyncOperation async = SceneManager.LoadSceneAsync(nextScene);

        // Keep yielding the co-routine until scene loading and activation is done.
        while (!async.isDone)
        {
            yield return null;
        }
        
        async.allowSceneActivation = true;
        isPaused = false;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            return;
        }
        
        for (int i = 0; i < fmodEventEmitters.Length; i++)
        {
            fmodEventEmitters[i].SetParameter("Action", combatMusicScalar);
        }
    }

    public bool startDialogue(DialogBehaviour dialogBehaviour)
    {
        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (activeSceneIndex == 2)
        {
            if (!succeededSeason)
            {
                return false;
            }
            else
            {
                dialogIndex = (((nextScene - 1) - 3) * 4) + 3;
            }
        }
        else
        {
            if (dialogIndex >= ((activeSceneIndex - 3) * 4) + 3)
            {
                return false;
            }
        }

        if(dialogIndex <= 3)
        {
            dialogBehaviour.StartDialog(AntonyNodes[dialogIndex]);
            dialogIndex++;
            return true;
        }
        if(dialogIndex <= 7)
        {
            dialogBehaviour.StartDialog(SamanthaNodes[(dialogIndex-4)]);
            dialogIndex++;
            return true;
        }
        if(dialogIndex <= 11)
        {
            dialogBehaviour.StartDialog(JamesNodes[(dialogIndex-8)]);
            dialogIndex++;
            return true;
        }
        if (dialogIndex <= 15)
        {
            dialogBehaviour.StartDialog(PhoebeNodes[(dialogIndex-12)]);
            dialogIndex++;
        }
        return true;
    }

    private int m_targetScene = 0;
    private bool m_isLoadingScene = false;

    public void StartLoadingScene(int targetScene)
    {
        if (m_isLoadingScene)
        {
            return;
        }
        m_targetScene = targetScene;
        StartCoroutine(LoadTargetSceneAsync());
    }
    
    public void StartLoadingNextScene()
    {
        if (m_isLoadingScene)
        {
            return;
        }
        m_targetScene = nextScene;
        StartCoroutine(LoadTargetSceneAsync());
    }
    
    private IEnumerator LoadTargetSceneAsync()
    {
        m_isLoadingScene = true;
        isPaused = true;
        
        // Start an asynchronous operation to load the scene
        AsyncOperation async = SceneManager.LoadSceneAsync(m_targetScene);

        // Keep yielding the co-routine until scene loading and activation is done.
        while (!async.isDone)
        {
            yield return null;
        }
        
        async.allowSceneActivation = true;
        isPaused = false;
        m_isLoadingScene = false;
    }
}
