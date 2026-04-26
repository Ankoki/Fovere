using System.Collections.Generic;
using UnityEngine;

public class DDOLTransmitter : MonoBehaviour
{

    public static DDOLTransmitter Instance;
    private Dictionary<string, object> _playerTransfer;
    private Dictionary<string, object> _worldsTransfer;

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

    /// <summary>
    /// Transfers a players serialized data to the transmitter for later use.
    /// </summary>
    /// <param name="data">The serialized player data.</param>
    public void TransferPlayer(Dictionary<string, object> data)
    {
        _playerTransfer = data;
    }

    /// <summary>
    /// Whether the transmitter contains player data or not.
    /// </summary>
    /// <returns>True if present and not empty, else false.</returns>
    public bool HasPlayerTransferData()
    {
        return _playerTransfer != null && _playerTransfer.Count > 0;
    }

    /// <summary>
    /// Retrieve the serialized player passed into this transmitter.
    /// </summary>
    /// <returns>The serialized player data. Data may not have been passed, this will be null if so.</returns>
    public Dictionary<string, object> RetrieveTransferredPlayer()
    {
        return _playerTransfer;
    }
    
    /// <summary>
    /// Transfers worlds serialized data to the transmitter for later use.
    /// </summary>
    /// <param name="data">The serialized world data.</param>
    public void TransferWorlds(Dictionary<string, object> data)
    {
        _worldsTransfer = data;
    }

    /// <summary>
    /// Whether the transmitter contains world transfer data or not.
    /// </summary>
    /// <returns>True if present and not empty, else false.</returns>
    public bool HasWorldTransferData()
    {
        return _worldsTransfer != null && _worldsTransfer.Count > 0;
    }

    /// <summary>
    /// Retrieve the serialized worlds passed into this transmitter.
    /// </summary>
    /// <returns>The serialized world data. Data may not have been passed, this will be null if so.</returns>
    public Dictionary<string, object> RetrieveTransferredWorlds()
    {
        return _worldsTransfer;
    }


    private void OnApplicationQuit()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
    }
    
}
