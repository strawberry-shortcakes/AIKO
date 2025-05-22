using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Teleporter : MonoBehaviour
{
    public GameManager gm;

    //public List<int> availableScenes = new List<int>() {2, 3, 4};

    //public List<int> playedScenes = new List<int>();

    //public int theSceneIndex;

    //Single level level

    void Awake()
    {
        gm = GameObject.Find("GAME MANAGER").GetComponent<GameManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) { 
        
        int index = Random.Range(0, gm.availableScenes.Count);
        int theSceneIndex = gm.availableScenes[index];
        gm.availableScenes.RemoveAt(index);

        gm.playedScenes.Add(theSceneIndex);
        SceneManager.LoadScene(theSceneIndex, LoadSceneMode.Single);
        Debug.Log(" SEX "); 
            if (gm.availableScenes.Count < 1)
            {
                gm.availableScenes = gm.playedScenes;
                gm.playedScenes = new List<int>();
            }
        }
    }
   // void OnTriggerEnter(Collider other)
    //{
      //  if (other.CompareTag("Player"))
       // {
       //     print("Switching Scene to " + scenebuildIndex);
       //     SceneManager.LoadScene(scenebuildIndex, LoadSceneMode.Single);

       // }
    //}

}
