using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

namespace Fovere
{
    public class PrototypeMovementController : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private CinemachineCamera cineCamera;
        [SerializeField] private PrototypePlayerController playerController;
        [SerializeField] private PrototypeInputReader inputReader;

        [Header("Settings")] 
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotationSpeed = 12f;
        [SerializeField] private float smoothTime = 0.2f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private bool hasGravity = true; // By default, the controller should support gravity.
        [SerializeField] private float gravity = -9.81f;
        
        private CharacterController _controller;
        private Animator _animator;

        private Transform _mainCamera;
        private float _currentSpeed;
        private float _velocity;
        private Vector3 _gravity;
        
        private void OnEnable()
        {
            inputReader.Interact += playerController.HandleInteract;
            inputReader.Jump += playerController.HandleJump; // The method dose only call the TriggerJump, however for now keep movement logic and input reading separate.
        }

        private void OnDisable()
        {
            inputReader.Interact -= playerController.HandleInteract;
            inputReader.Jump -= playerController.HandleJump;
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
            _mainCamera = Camera.main.transform;
            SetupFreelook();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void TriggerJump()
        {
            // Sometimes does not trigger. Investigate.
            if (_controller.isGrounded)
                _gravity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        private void SetupFreelook()
        {
            cineCamera.Follow = transform;
            cineCamera.LookAt = transform;
            cineCamera.OnTargetObjectWarped(
                transform,
                transform.position - cineCamera.transform.position - Vector3.forward);
        }

        private void Update()
        {
            if (hasGravity)
                HandleGravity();
            if (!playerController.IsFrozen)
                HandleMovement();
        }
        

        private void HandleGravity()
        {
            if (_controller.isGrounded && _gravity.y < 0)
                _gravity.y = -2f;
            _gravity.y += gravity * Time.deltaTime;
            _controller.Move(_gravity * Time.deltaTime);
        }
        
        private void HandleMovement()
        {
            var movementDir = new Vector3(inputReader.Direction.x, 0f, inputReader.Direction.y).normalized;
            // Rotate movement direction to account for camera rotation.
            var adjustedDir = Quaternion.AngleAxis(_mainCamera.eulerAngles.y, Vector3.up) * movementDir;
            if (adjustedDir.magnitude > 0f)
            {
                HandleRotation(adjustedDir);
                HandleCharacterController(adjustedDir);
                SmoothSpeed(adjustedDir.magnitude);
            }
            else
                SmoothSpeed(0f);
        }

        private void HandleCharacterController(Vector3 adjustedDir)
        {
            // Move player
            var adjustedMovement = adjustedDir * (moveSpeed * Time.deltaTime);
            _controller.Move(adjustedMovement);
        }

        private void HandleRotation(Vector3 adjustedDir)
        {
            // Rotate to match movement direction
            var targetRotation = Quaternion.LookRotation(adjustedDir);
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            transform.LookAt(transform.position + adjustedDir);
        }

        private void SmoothSpeed(float target)
        {
            _currentSpeed = Mathf.SmoothDamp(_currentSpeed, target, ref _velocity, smoothTime);
        }
        
        /// <summary>
        /// Makes the player turn to look at a location.
        /// Taken directly from resident. TODO change animation and maybe import other method too.
        /// </summary>
        /// <param name="pos">The location to look at.</param>
        public void TurnTo(Vector3 pos)
        {
            transform.DOLookAt(pos, Vector3.Distance(transform.position, pos) / 5);
            // var turn = "turn_" + (IsRightSide(pos) ? "right" : "left");
            // _animator.SetTrigger(turn); 
        }
        
    }
}