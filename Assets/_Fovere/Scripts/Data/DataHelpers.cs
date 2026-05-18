using System.Collections.Generic;
using UnityEngine;

namespace Fovere
{
    /// <summary>
    /// Utility class to help with serializing and deserializing data.
    /// </summary>
    public static class DataHelpers
    {

        /// <summary>
        /// Converts a string to a Vector3Int.
        /// </summary>
        /// <param name="raw">The raw string.</param>
        /// <returns>The Vector3Int.</returns>
        public static Vector3Int ToVector3Int(string raw)
        {
            var split = raw.Split(",");
            return new Vector3Int(int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2]));
        }

        /// <summary>
        /// Converts a string to a Vector3.
        /// </summary>
        /// <param name="raw">The raw string.</param>
        /// <returns>The Vector3.</returns>
        public static Vector3 ToVector3(string raw)
        {
            var split = raw.Split(",");
            return new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
        }

        /// <summary>
        /// Converts a string to a Vector2Int.
        /// </summary>
        /// <param name="raw">The raw string.</param>
        /// <returns>The Vector2Int.</returns>
        public static Vector2Int ToVector2Int(string raw)
        {
            var split = raw.Split(",");
            return new Vector2Int(int.Parse(split[0]), int.Parse(split[1]));
        }

        /// <summary>
        /// Converts a string to a Vector2.
        /// </summary>
        /// <param name="raw">The raw string.</param>
        /// <returns>The Vector2.</returns>
        public static Vector2 ToVector2(string raw)
        {
            var split = raw.Split(",");
            return new Vector2(float.Parse(split[0]), float.Parse(split[1]));
        }

        /// <summary>
        /// Converts a Vector3 to a string.
        /// </summary>
        /// <param name="vector">The vector to convert.</param>
        /// <returns>The converted string.</returns>
        public static string FromVector3(Vector3 vector)
        {
            return $"{vector.x},{vector.y},{vector.z}";
        }

        /// <summary>
        /// Converts a Vector2 to a string.
        /// </summary>
        /// <param name="vector">The vector to convert.</param>
        /// <returns>The converted string.</returns>
        public static string FromVector2(Vector2 vector)
        {
            return $"{vector.x},{vector.y}";
        }

        /// <summary>
        /// Validates a dictionary contains the given keys.
        /// </summary>
        /// <param name="data">The data to validate.</param>
        /// <param name="keys">The keys to compare against.</param>
        /// <returns>True if all data is valid and present, else false.</returns>
        public static bool ValidateKeys(Dictionary<string, object> data, string[] keys)
        {
            var valid = true;
            var missing = new List<string>();
            foreach (var key in keys)
            {
                if (data.ContainsKey(key))
                    continue;
                missing.Add(key);
                valid = false;
            }

            if (valid)
                return true;
            Debug.Log($"Invalid data, missing keys: {string.Join(", ", missing)}.");
            return false;
        }

    }
}