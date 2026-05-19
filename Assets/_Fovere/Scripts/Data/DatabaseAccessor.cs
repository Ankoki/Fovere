using System.Collections;
using System.Collections.Generic;
using PlayFab.Json;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Fovere
{
    /// <summary>
    /// Class used to access the database. This is used so we can use a coroutine whenever needed.
    /// </summary>
    public static class DatabaseAccessor
    {

        private const string RestHost = "https://10.8.2.61:3000/";
        private const string UpdateUserEndpoint = "update-user";
        private const string GetUserEndpoint = "get-user";
        private const string UpdateWorldEndpoint = "update-world";
        private const string GetWorldEndpoint = "get-world";
        private const string GetWorldsEndpoint = "get-worlds";
        private const string DatabaseToken = "JG#6eP4pfHwfkdTF59wcNsazipzi4cThX^/cHx2kn#c="; // Regenerated on API end. Not in use for prototype.

        private static readonly Dictionary<long, string> FetchUserErrors = new()
        {
            { 401, "No user header or no authentication key header received." },
            { 403, "Authentication key provided was incorrect." },
            { 404, "No user found with the given title id." },
            { 422, "An exception occured during selection from database." }
        };

        private static bool dirty;

        /// <summary>
        /// Monitors the execution of these requests and allows for easier error tracking.
        /// </summary>
        public static void Monitor()
        {
            dirty = false;
        }

        /// <summary>
        /// Checks if any errors have occured since last marked as monitored.
        /// </summary>
        /// <returns>True if any errors, else false.</returns>
        public static bool IsDirty()
        {
            return dirty;
        }

        /// <summary>
        /// Fetches the data for the user with the given title id.
        /// </summary>
        /// <param name="titleId">The title ID of the user.</param>
        /// <param name="loginStatusText">Text field for authentication. This is used to display errors if necessary.</param>
        /// <returns></returns>
        public static IEnumerator FetchUserData(string titleId, TMP_Text loginStatusText)
        {
            using var request = UnityWebRequest.Get(RestHost + GetUserEndpoint);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("User", titleId);
            request.SetRequestHeader("Authorization", DatabaseToken);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                var json =
                    (Dictionary<string, object>)PlayFabSimpleJson.DeserializeObject(request.downloadHandler.text);
                DDOLTransmitter.Instance.TransferPlayer(json);
                SceneManager.LoadScene("Scenes/GameScene",
                    LoadSceneMode
                        .Single); // Might be additive in the future to further simplify layers. TODO maybe change placement, doesn't belong in this class.
            }
            else
            {
                loginStatusText.text = FetchUserErrors[request.responseCode];
                dirty = true;
            }
        }

        public static IEnumerator UpdateUserData(Dictionary<string, object> userData)
        {
            using var request = UnityWebRequest.Post(RestHost + UpdateUserEndpoint,
                PlayFabSimpleJson.SerializeObject(userData), "application/json");
            request.SetRequestHeader("Authorization", DatabaseToken);
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                // TODO error handling, this can be called anywhere, so perhaps a toast alerting the user, with a local backup made?
                dirty = true;
            }
        }

        public static IEnumerator FetchWorlds(string titleId, TMP_Text loginStatusText)
        {
            using var request = UnityWebRequest.Get(RestHost + GetWorldsEndpoint);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("User", titleId);
            request.SetRequestHeader("Authorization", DatabaseToken);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                var json =
                    (Dictionary<string, object>)PlayFabSimpleJson.DeserializeObject(request.downloadHandler.text);
                DDOLTransmitter.Instance.TransferPlayer(json);
                SceneManager.LoadScene("Scenes/GameScene",
                    LoadSceneMode.Single); // Might be additive in the future to further simplify layers.
            }
            else
            {
                loginStatusText.text = FetchUserErrors[request.responseCode];
                dirty = true;
            }
        }

    }
}