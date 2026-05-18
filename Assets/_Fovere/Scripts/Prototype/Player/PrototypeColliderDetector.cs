using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Fovere
{
    /// <summary>
    /// Detects for colliders within the interact range with the given search tag.
    /// Will call a detection event with all colliders found every update.
    /// </summary>
    public class PrototypeColliderDetector : MonoBehaviour
    {
        [Header("Settings")] 
        [SerializeField] private float interactRange = 3f;
        [SerializeField] private int colliderSize = 10;
        [SerializeField] private string searchTag = "Resident";

        public event UnityAction<GameObject> OnDetect = delegate { }; // Maybe pass Collider in the future
        public event UnityAction<GameObject> OnLeave = delegate { };

        private readonly List<Collider> _colliders = new();

        private void Update()
        {
            var hitColliders = new Collider[colliderSize];
            var count = Physics.OverlapSphereNonAlloc(transform.position, interactRange, hitColliders);
            // Sorts closest to furthest
            var sortedColliders = hitColliders
                .Take(count)
                .OrderBy(c => (c.transform.position - transform.position).sqrMagnitude)
                .Where(c => c.CompareTag(searchTag));
            var foundArray = sortedColliders as Collider[] ?? sortedColliders.ToArray();
            // Invokes on detect for colliders that aren't in the global list.
            foreach (var col in foundArray.Except(_colliders).ToList())
            {
                _colliders.Add(col);
                OnDetect?.Invoke(col.gameObject);
            }
            // Invokes on leave for colliders that aren't in the found list.
            foreach (var c in _colliders.Except(foundArray).ToList())
            {
                _colliders.Remove(c);
                OnLeave?.Invoke(c.gameObject);
            }
        }
        
    }
}