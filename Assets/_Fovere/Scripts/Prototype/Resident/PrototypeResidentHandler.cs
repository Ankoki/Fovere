using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Fovere
{
    public class PrototypeResidentHandler : MonoBehaviour
    {

        [Header("Prompt Objects")] 
        [SerializeField] private GameObject canvas;
        [SerializeField] private GameObject interact;
        [SerializeField] private GameObject prompt;
        [Header("Speech Settings")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] talkingSounds;
        [SerializeField] private AudioClip punctuationSound;
        [SerializeField] private int talkingInterval = 4; // Stops rapid talking if too fast.
        
        [Header("Resident Data")]
        public ResidentData data; // Contains dialogue currently but will be removed. Set separately for now to show logic.
        public DialogueData dialogue;
        
        private CharacterController _controller;
        private PrototypeResidentNavigation _navigator;
        private Vector3 _teleportPosition;
        private bool _freeze;

        public bool isTalking;
        private int _charCount;

        private TMP_Animated _animatedText;
        private Animator _animator;

        public Transform particlesParent;

        private void Awake()
        {
            data = ScriptableObject.CreateInstance<ResidentData>();
            _animator = GetComponent<Animator>();
            _controller = GetComponent<CharacterController>();
            _navigator = GetComponent<PrototypeResidentNavigation>();
        }

        private void Start()
        {
            interact.SetActive(false);
            prompt.SetActive(false);
            canvas.SetActive(false);
            _animatedText = ConversationHandler.Instance.animatedText;
            if (_animatedText == null)
                return;
            _animatedText.OnReveal += HandleSpeech;
            _animatedText.OnAction += SetAction;
        }

        private void OnDisable()
        {
            _animatedText.OnReveal -= HandleSpeech;
            _animatedText.OnAction -= SetAction;
        }

        private void Update()
        {
            // Make the interactions face the main camera.
            var rot = Camera.main.transform.rotation;
            interact.transform.rotation = rot;
            prompt.transform.rotation = rot;
        }

        private void LateUpdate()
        {
            // Safe Teleporting for CharacterController
            if (_teleportPosition == Vector3.zero)
                return;
            _controller.enabled = false;
            transform.position = _teleportPosition;
            _teleportPosition = Vector3.zero;
            _controller.enabled = true;
        }

        /// <summary>
        /// Handles our on text reveal event to make the resident talk along with text revealed.
        /// </summary>
        /// <param name="c">The revealed character.</param>
        private void HandleSpeech(char c)
        {
            if (char.IsWhiteSpace(c)) 
                return;
            if (char.IsPunctuation(c))
            {
                audioSource.pitch = Random.Range(1.1f, 1.3f);
                audioSource.PlayOneShot(punctuationSound);
                return;
            }
            _charCount++;
            if (_charCount % talkingInterval != 0) 
                return;
            var clip = talkingSounds[Random.Range(0, talkingSounds.Length)];
            audioSource.pitch = Random.Range(1.1f, 1.3f); // 0.8 - 1.0 for deeper. Add pitch variation to resident data.
            audioSource.PlayOneShot(clip);
        }
        
        public void ResetSpeech()
        {
            _charCount = 0;
            audioSource.pitch = 1f;
        }

        /// <summary>
        /// Sets the current action of this resident.
        /// </summary>
        /// <param name="action">The action the resident should do.</param>
        public void SetAction(string action)
        {
            if (this != ConversationHandler.Instance.currentPrototypeResident)
                return;
            /*if (action == "shake")
                Camera.main.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
            else
                PlayParticle(action);*/ // TODO setup actions.
        }

        /// <summary>
        /// Plays a particle at the resident.
        /// </summary>
        /// <param name="particle">The particle to play.</param>
        public void PlayParticle(string particle)
        {
            var p = particlesParent.Find(particle + "_particle");
            if (p == null)
                return;
            p.GetComponent<ParticleSystem>().Play();
        }

        public void ShowInteractPrompt()
        {
            if (isTalking)
                return;
            canvas.SetActive(true);
            interact.SetActive(true);
            prompt.SetActive(true);
        }

        public void HideInteractPrompt()
        {
            interact.SetActive(false);
            prompt.SetActive(false);
            canvas.SetActive(false);
        }

        /// <summary>
        /// Resets the resident to an idle state.
        /// </summary>
        public void Idle()
        {
            isTalking = false;
            _navigator.SetState(PrototypeResidentNavigation.ResidentState.Idle);
            // _animator.SetTrigger("idle");
        }

        /// <summary>
        /// Makes the resident turn to look at a location.
        /// </summary>
        /// <param name="pos">The location to look at.</param>
        public void TurnTo(Vector3 pos)
        {
            transform.DOLookAt(pos, Vector3.Distance(transform.position, pos) / 5);
            // var turn = "turn_" + (IsRightSide(pos) ? "right" : "left");
            // _animator.SetTrigger(turn);
        }

        /// <summary>
        /// Checks if the direction between the forward and target is left or right.
        /// </summary>
        /// <param name="target">The target location.</param>
        /// <returns>True if on the right, else false.</returns>
        private bool IsRightSide(Vector3 target)
        {
            var right = Vector3.Cross(Vector3.up.normalized, transform.forward.normalized);
            var dir = Vector3.Dot(right, target.normalized);
            return dir > 0f;
        }
        
        /// <summary>
        /// Teleports the player to a new location.
        /// </summary>
        /// <param name="x">The x-coord.</param>
        /// <param name="y">The y-coord.</param>
        /// <param name="z">The z-coord.</param>
        public void Teleport(float x, float y, float z)
        {
            _teleportPosition = new Vector3(x, y, z);
        }

        /// <summary>
        /// Freezes the residents movement.
        /// </summary>
        public void Freeze()
        {
            _navigator.SetState(PrototypeResidentNavigation.ResidentState.Frozen);
        }

        /// <summary>
        /// Unfreezes the residents' movement.
        /// </summary>
        public void Unfreeze()
        {
            _navigator.SetState(PrototypeResidentNavigation.ResidentState.Idle);
        }

        /// <summary>
        /// Checks if the resident is frozen.
        /// </summary>
        /// <returns>True if frozen, else false.</returns>
        public bool IsFrozen()
        {
            return _navigator.GetState() == PrototypeResidentNavigation.ResidentState.Frozen;
        }

    }
}