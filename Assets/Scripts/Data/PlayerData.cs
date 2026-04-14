using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// TODO link to authentication, somehow retrieve current auth info.
public class PlayerData
{

    private static UnityWebRequest setHeaders(UnityWebRequest request)
    {
        request.SetRequestHeader("Content-Type", "application/json");
        
    }
    
    private const string DataServerUrl = "http://localhost:8080"; // it's not, change this after the backend is set up.
    
    private readonly Dictionary<string, bool> _expansions = new();
    public string World = "Village";

    public PlayerData(string identifier)
    {
        Debug.Log($"identifier={identifier}");
        var request = SignData(identifier);
        
    }

    IEnumerator SignData(string identifier)
    {
        using var www = UnityWebRequest.Get($"{DataServerUrl}/request-game-connection?{identifier}");
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            switch (www.responseCode)
            {
                 
            }
        }
    }

    public bool HasExpansion(string key)
    {
        return _expansions.GetValueOrDefault(key, false);
    }

    public void UnlockExpansion(string key)
    {
        _expansions.Add(key, true);
    }

    public void RevokeExpansion(string key)
    {
        _expansions.Remove(key);
    }
    
    public static class Expansion
    {
        public const string InventoryRowTwo = "EXP_INV_ROW_TWO";
        public const string InventoryRowThree = "EXP_INV_ROW_THREE";
        public const string InventoryRowFour = "EXP_INV_ROW_FOUR";
    }
    
}