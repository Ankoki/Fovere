using System.IO;
using UnityEditor;
using UnityEngine;

namespace ByeolTools // Hidden for now as it's causing issues with build.
{
    public static class Setup
    {
        /*
        [MenuItem("Tools/Setup/Refresh")]
        public static void CreateDefaultFolders()
        {
            Folders.CreateDefault("_Project", "Prefabs", "Art", "Animation", "ScriptableObjects", "Scripts", "Materials",
                "Settings");
            AssetDatabase.Refresh();
        }

        private static class Folders
        {
            public static void CreateDefault(string root, params string[] folders)
            {
                var fullPath = Path.Combine(Application.dataPath, root);
                foreach (var folder in folders)
                {
                    var path = Path.Combine(fullPath, folder);
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                }
            }
        }*/ 
    }
}