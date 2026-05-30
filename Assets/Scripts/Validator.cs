using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Validator : MonoBehaviour
{
    private void Awake()
    {
        if (GameManager.instance is null)
        {
            SceneManager.LoadScene(0);
        }
    }
}
