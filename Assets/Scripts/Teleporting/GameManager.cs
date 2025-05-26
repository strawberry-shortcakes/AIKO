using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public List<int> availableScenes = new List<int>() { 2, 3, 4, 5 };
    public List<int> playedScenes = new List<int>();

    //void Awake()
    //{
    //    if (Instance != null && Instance != this)
    //    {
    //        Destroy(gameObject); // Destroy if a duplicate is found
    //        return;
    //    }
    //    Instance = this;
    //    DontDestroyOnLoad(this.gameObject);
    //}

    void Awake()///DESTROY DUPLICATE
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            //SceneManager.sceneLoaded += LoadedScene;
        }
        else
        {
            Destroy(gameObject);
        }


    }
}
