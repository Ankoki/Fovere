using System.Collections.Generic;
using UnityEngine;

namespace Fovere
{
    public class DDOLTransmitter : MonoBehaviour
    {

        public static DDOLTransmitter Instance;
        private Dictionary<string, object> _playerTransfer;

        private Dictionary<string, object> _worldsTransfer;

        // Used as a backup while database connection isn't guaranteed. TODO fallback save to system storage.
        private string _titleIdTransfer;
        private string _usernameTransfer;

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
        /// Transfers a player's title id to be used between scenes.
        /// </summary>
        /// <param name="titleId">The title id to tranfer.</param>
        public void TransferTitleId(string titleId)
        {
            _titleIdTransfer = titleId;
        }

        /// <summary>
        /// Retrieves the transferred title id.
        /// </summary>
        /// <returns>The title id to transfer.</returns>
        public string RetrieveTitleId()
        {
            return _titleIdTransfer;
        }

        /// <summary>
        /// Transfers a username to be used between scenes.
        /// </summary>
        /// <param name="username">The username to transfer.</param>
        public void TransferUsername(string username)
        {
            _usernameTransfer = username;
        }

        /// <summary>
        /// Retrieves a transferred username.
        /// </summary>
        /// <returns>The transferred username.</returns>
        public string RetrieveUsername()
        {
            return _usernameTransfer;
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
}