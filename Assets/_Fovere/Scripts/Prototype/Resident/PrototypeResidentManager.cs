using System.Collections.Generic;
using UnityEngine;

namespace Fovere
{
    /// <summary>
    /// Spawns residents at random positions within a radius.
    /// Useful for spawning test residents.
    /// </summary>
    public class PrototypeResidentManager : MonoBehaviour
    {
        [Header("Spawning")] 
        [SerializeField] private GameObject residentPrefab;
        [SerializeField] private List<ResidentData> residentData;
        [SerializeField] private List<DialogueData> dialogueData;
        [SerializeField] private float spawnRadius = 30f;

        private void Start()
        {
            if (residentPrefab == null)
            {
                Debug.LogWarning("[ResidentManager] No resident prefab assigned.");
                return;
            }
            SpawnResidents();
        }

        /// <summary>
        /// Spawns our test residents using the data from the fields above.
        /// </summary>
        private void SpawnResidents()
        {
            var count = Mathf.Min(residentData.Count, dialogueData.Count);
            for (var i = 0; i < count; i++)
            {
                var flatOffset = Random.insideUnitCircle * spawnRadius;
                var spawnPos = transform.position + new Vector3(flatOffset.x, 0f, flatOffset.y);
                if (Physics.Raycast(spawnPos + Vector3.up * 10f, Vector3.down, out var hit, 20f))
                    spawnPos = hit.point;
                var resident = Instantiate(residentPrefab, spawnPos, Quaternion.identity, transform);
                var data = residentData[i];
                var renderer = resident.GetComponent<MeshRenderer>();
                renderer.material = data.residentMaterial;
                var handler = resident.GetComponent<PrototypeResidentHandler>();
                handler.data = data;
                handler.dialogue = dialogueData[i];
                resident.name = $"Resident[{handler.data.name}]";
                resident.transform.parent = transform.parent;
            }
            Debug.Log($"[ResidentManager] Spawned {count} residents.");
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.3f);
            Gizmos.DrawSphere(transform.position, spawnRadius);
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
        }
#endif
    }
}
