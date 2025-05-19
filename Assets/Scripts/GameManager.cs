using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public List<int> availableScenes = new List<int>() { 2, 3, 4 };
    public List<int> playedScenes = new List<int>();

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
