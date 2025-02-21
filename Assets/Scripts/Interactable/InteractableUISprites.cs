using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableUISprites : MonoBehaviour
{
    public static InteractableUISprites Instance { get; private set; }
    public Dictionary<string, Sprite> _keyDictionary;
    
    /// <summary>
    /// Singleton for loading interactable sprite assets
    /// </summary>
    void Start()
    {
        if (Instance == null){
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(gameObject);
        }

        _keyDictionary = new Dictionary<string, Sprite>();
        for (char c = 'A'; c <= 'Z'; c++)
        {
            _keyDictionary[c.ToString()] = Resources.Load<Sprite>($"Sprites/Keys/Letters/keyboard_{c.ToString().ToLower()}");
        }
    }
}
