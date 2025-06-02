using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<int> availableScenes = new List<int>() { 2, 3, 4, 5 };
    public List<int> playedScenes = new List<int>();

    public PlayerMovement pm;

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
        pm = GameObject.Find("Player").GetComponent<PlayerMovement>();

        if (Instance == null)
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

    void Start ()
    {
        
    }

    void Update ()
    {
        if (pm.isDead)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
            }

            
        }
    }
}
