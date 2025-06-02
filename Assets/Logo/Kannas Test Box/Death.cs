using UnityEngine;
using System.Collections;

public class Death : MonoBehaviour
{
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
            Application.LoadLevel(Application.loadedLevel);
    }
}