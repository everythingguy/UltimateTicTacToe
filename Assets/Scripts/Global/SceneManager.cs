using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    // Declare any public variables that you want to be able 
    // to access throughout your scene
    public Player player;
    public Turn turn;
    public NetworkManager networkManager;
    public static SceneManager Instance { get; private set; } // static singleton
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else { Destroy(gameObject); }
    }
}
