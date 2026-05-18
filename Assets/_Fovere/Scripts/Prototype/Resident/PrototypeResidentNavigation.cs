using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace Fovere
{
    /// <summary>
    /// Class to use a Navigation Agent to control random movement
    /// of residents around our terrain.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(PrototypeResidentHandler))]
    public class PrototypeResidentNavigation : MonoBehaviour
    {
        [Header("Wandering Settings")] public float wanderRadius = 15f;
        public float minIdleTime = 2f;
        public float maxIdleTime = 6f;
        public int maxNavMeshSampleAttempts = 5;

        [Header("Movement Settings")] public float walkSpeed = 1.5f;
        public float stoppingThreshold = 0.5f;

        [Header("Animation")] public Animator animator;
        public string walkingParam = "IsWalking";

        private bool _hasAnimator;
        private NavMeshAgent _agent;
        private PrototypeResidentHandler _handler;
        private Vector3 _homePosition;
        private ResidentState _state = ResidentState.Idle;
        private Coroutine _idleCoroutine;

        public enum ResidentState
        {
            Idle,
            Walking,
            Frozen
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _handler = GetComponent<PrototypeResidentHandler>();
            _homePosition = transform.position;
            _agent.speed = walkSpeed;
            _agent.stoppingDistance = stoppingThreshold;
            _agent.autoBraking = true;
            _hasAnimator = animator != null;
        }

        private void OnEnable()
        {
            _idleCoroutine = StartCoroutine(IdleRoutine());
        }

        private void OnDisable()
        {
            if (_idleCoroutine != null)
                StopCoroutine(_idleCoroutine);
        }

        private void Update()
        {
            // Detect frozen state and clear any paths
            if (_state == ResidentState.Frozen && !HasReachedDestination())
                _agent.ResetPath();
            // Detect arrival and transition back to idle
            if (_state == ResidentState.Walking && HasReachedDestination())
                SetState(ResidentState.Idle);
            // Sync animation
            if (_hasAnimator)
                animator.SetBool(walkingParam, _state == ResidentState.Walking);
        }

        private IEnumerator IdleRoutine()
        {
            while (true)
            {
                // Wait while not idle. Stops frozen and walking residents generating new paths.
                yield return new WaitUntil(() => _state == ResidentState.Idle);

                // Pause for a random idle duration
                var waitTime = Random.Range(minIdleTime, maxIdleTime);
                yield return new WaitForSeconds(waitTime);

                if (_state == ResidentState.Frozen) // Check for frozen state before getting path for any edits in state between idle time.
                    continue;
                // Pick a destination and walk to it
                if (!TryGetRandomNavMeshPoint(out var destination))
                    continue;
                _agent.SetDestination(destination);
                _state = ResidentState.Walking;
            }
        }

        /// <summary>
        /// Tries to find a valid point on the NavMesh within wanderRadius of home.
        /// </summary>
        private bool TryGetRandomNavMeshPoint(out Vector3 result)
        {
            for (var i = 0; i < maxNavMeshSampleAttempts; i++)
            {
                // Pick a random direction from home position
                var randomOffset = Random.insideUnitSphere * wanderRadius;
                randomOffset.y = 0f; // keep horizontal
                var candidate = _homePosition + randomOffset;
                if (!NavMesh.SamplePosition(candidate, out var hit, wanderRadius, NavMesh.AllAreas))
                    continue;
                result = hit.position;
                return true;
            }

            result = transform.position;
            return false;
        }

        /// <summary>
        /// Returns true when the agent has stopped and is close enough to its destination.
        /// </summary>
        private bool HasReachedDestination()
        {
            if (_agent.pathPending)
                return false;
            if (_agent.remainingDistance > _agent.stoppingDistance)
                return false;
            return !_agent.hasPath || !(_agent.velocity.sqrMagnitude > 0.01f);
        }

        /// <summary>
        /// Gets the state of this resident.
        /// </summary>
        /// <returns>The state of a resident.</returns>
        public ResidentState GetState()
        {
            return _state;
        }

        /// <summary>
        /// Updates the state of this resident.
        /// </summary>
        /// <param name="state"></param>
        public void SetState(ResidentState state)
        {
            _state = state;
            if (state == ResidentState.Idle)
                _agent.ResetPath();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw the wander radius around home position
            var origin = Application.isPlaying ? _homePosition : transform.position;
            UnityEditor.Handles.color = new Color(0.2f, 0.8f, 0.2f, 0.15f);
            UnityEditor.Handles.DrawSolidDisc(origin, Vector3.up, wanderRadius);
            UnityEditor.Handles.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
            UnityEditor.Handles.DrawWireDisc(origin, Vector3.up, wanderRadius);

            // Draw current destination
            if (!Application.isPlaying || _agent == null || !_agent.hasPath)
                return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_agent.destination, 0.2f);
            Gizmos.DrawLine(transform.position, _agent.destination);
        }
#endif
    }
}