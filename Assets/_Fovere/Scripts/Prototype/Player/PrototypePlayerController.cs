using System;
using DG.Tweening;
using UnityEngine;

namespace Fovere
{
    public class PrototypePlayerController : MonoBehaviour
    {
        [SerializeField] private DialogueData startingDialogue;
        [SerializeField] private PrototypeColliderDetector detector;
        private PrototypeResidentHandler _nearestResident;
        public bool IsFrozen { get; set; } // Update this to use enum states for consistency.

        private void Start()
        {
            ConversationHandler.Instance.ShowDialogue(startingDialogue, this);
        }

        private void OnEnable()
        {
            detector.OnDetect += HandleDetect;
            detector.OnLeave += HandleLeave;
        }

        private void OnDisable()
        {
            detector.OnDetect -= HandleDetect;
            detector.OnLeave -= HandleLeave;
        }

        public void HandleJump()
        {
            if (!TryGetComponent(out PrototypeMovementController controller))
                return;
            controller.TriggerJump();
        }

        public void HandleInteract()
        {
            var handler = ConversationHandler.Instance;
            if (handler.inDialogue)
                handler.Advance();
            else if (_nearestResident != null && !_nearestResident.isTalking)
            {
                _nearestResident.HideInteractPrompt();
                handler.StartConversation(_nearestResident, this);
            }
        }

        private void HandleDetect(GameObject obj)
        {
            if (ConversationHandler.Instance.inDialogue)
                return;
            if (!obj.TryGetComponent(out PrototypeResidentHandler resident))
                return;
            Debug.Log($"Resident[{resident.name}] detected.");
            _nearestResident = resident;
            _nearestResident.ShowInteractPrompt();
        }

        private void HandleLeave(GameObject obj)
        {
            if (!obj.TryGetComponent(out PrototypeResidentHandler resident))
                return;
            Debug.Log($"Resident[{resident.name}] left range.");
            resident.HideInteractPrompt();
            if (_nearestResident == resident)
                _nearestResident = null;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw sphere around the player's interact radius.
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.3f);
            Gizmos.DrawSphere(transform.position, 4);
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, 4);
        }
#endif
        
    }
}