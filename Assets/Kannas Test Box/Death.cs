using UnityEngine;
using System.Collections;

public class Death : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Application.LoadLevel(Application.loadedLevel);
    }
}