using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Teleport_SCRAPPED: MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("Switching Scene to 2");
            SceneManager.LoadScene(3);

        }
    }



}
