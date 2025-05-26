using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Teleport01 : MonoBehaviour
{ 

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("Switching Scene to 1"  );
            SceneManager.LoadScene(2);

        }
    }



}
