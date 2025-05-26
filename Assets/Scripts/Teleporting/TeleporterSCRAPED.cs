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
        gm = GameObject.Find("TELEPORTER MANAGER").GetComponent<GameManager>();
        //gm.availableScenes.RemoveAt(index);
    }

    void OnTriggerEnter(Collider other)
    {
         Debug.Log("Pre Tag");
        if (other.CompareTag("Player")) 
        {
             Debug.Log("Post taag");

            int index = Random.Range(0, gm.availableScenes.Count);
             //index = 0;//for debug
            int theSceneIndex = gm.availableScenes[index];
            gm.availableScenes.RemoveAt(index);

            

            gm.playedScenes.Add(theSceneIndex);
            SceneManager.LoadScene(theSceneIndex, LoadSceneMode.Single);
             Debug.Log(index); //debug
             Debug.Log(theSceneIndex); //debug
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
