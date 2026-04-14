using System;
using UnityEngine;

public class DDOLTransmitter : MonoBehaviour
{

    public static DDOLTransmitter Instance;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    private void OnApplicationQuit()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
    }
    
}
