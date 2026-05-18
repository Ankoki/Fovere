using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Fovere
{
    /// <summary>
    /// Handles player information and interactions with its data.
    /// </summary>
    public class PlayerHandler : MonoBehaviour
    {

        [Header("Render Settings")] public int renderDistance = 8;

        [Header("Movement Settings")] public float speed = 5F;
        public float sensitivity = 0.5f;
        public float gravity = -9.81f;
        public float jumpHeight = 1.5f;

        [Header("Inventory Settings")] public GameObject inventoryMenu;

        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _lookAction;

        private Vector3 _velocity;

        private CharacterController _controller;
        private Animator _animator;
        private Camera _mainCamera;

        private PlayerData _playerData;

        private PlayerInventory _playerInventory;
        private bool _inventoryOpen;

        private World _currentWorld;
        private bool _isSpawned;
        private Vector3 _teleportPosition = Vector3.zero;

        private bool _freeze;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
            _mainCamera = GameObject.Find("Camera").GetComponent<Camera>();
            _moveAction = InputSystem.actions.FindAction("Move");
            _jumpAction = InputSystem.actions.FindAction("Jump");
            _lookAction = InputSystem.actions.FindAction("Look");
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private readonly List<Chunk> _renderedChunks = new();

        private void Update()
        {
            Debug.Log($"Spawned[{_isSpawned}],ChunkPos[{string.Join(",", _renderedChunks)}]");
            if (!IsSpawned())
                return;
            // Rendering
            /*var old = _renderedChunks;
            var chunk = GetChunk();
            if (!chunk.IsLoaded())
                chunk.Load();
            var originPos = chunk.chunkCoord;
            var originX = originPos.x;
            var originZ = originPos.y;
            var chunkPosition = chunk.chunkCoord;
            var xMax = chunkPosition.x + renderDistance;
            var zMax = chunkPosition.y + renderDistance;
            _renderedChunks.Clear();
            for (var x = xMax; x > (originX - renderDistance); x--)
            {
                for (var z = zMax; z > (originZ - renderDistance); z--)
                {
                    var found = _currentWorld.GetChunk(x, z);
                    found.Load();
                    _renderedChunks.Add(found);
                }
            }
            foreach (var c in old)
            {
                if (!_renderedChunks.Contains(c))
                    c.Unload();
            }*/
            // Gravity
            if (_controller.isGrounded && _velocity.y < 0)
                _velocity.y = -2f;
            _velocity.y += gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
            // Camera Rotation for Mouse Look
            var inputL = _lookAction.ReadValue<Vector2>();
            var mouseX = inputL.x * sensitivity;
            var mouseY = inputL.y * sensitivity; // TODO y axis wont budge, check constraints.
            var currentRotation = transform.localEulerAngles;
            currentRotation.y += mouseX;
            transform.localRotation = Quaternion.AngleAxis(currentRotation.y, Vector3.up);
            var currentCameraRotation = _mainCamera.gameObject.transform.eulerAngles;
            currentCameraRotation.x -= mouseY;
            _mainCamera.gameObject.transform.localRotation =
                Quaternion.AngleAxis(currentCameraRotation.x, Vector3.right);
            // Process gravity before freezing mechanics so players aren't stuck midair.
            if (_freeze)
                return;
            // Movement
            var input = _moveAction.ReadValue<Vector2>();
            var move = new Vector3(input.x, 0, input.y);
            move = _mainCamera.transform.TransformDirection(move);
            _controller.Move(move * speed * Time.deltaTime);
            // Jumping
            if (_jumpAction.triggered && _controller.isGrounded)
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            // Animation
            // anim.SetFloat("run", direction.magnitude); // TODO setup player animations.
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
        /// Checks if the player has been spawned.
        /// </summary>
        /// <returns>True if spawned, else false.</returns>
        public bool IsSpawned()
        {
            return _isSpawned;
        }

        /// <summary>
        /// Sets the player data of this player. Will only be set once and should be done by the game manager.
        /// </summary>
        /// <param name="playerData">The player data of this player.</param>
        public void SetPlayerData(PlayerData playerData)
        {
            _playerData ??= playerData;
        }

        public void SpawnPlayer()
        {
            _isSpawned = true;
        }

        /// <summary>
        /// Teleports the player to a new location.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="x">The x-coordinates.</param>
        /// <param name="z">The z-coordinates.</param>
        public void SafeTeleport(World world, float x, float z)
        {
            Teleport(world, x, world.GetHighestAt(Mathf.FloorToInt(x), Mathf.FloorToInt(z)).Position.y + 2, z);
        }

        /// <summary>
        /// Teleports the player to a new location.
        /// </summary>
        /// <param name="x">The x-coord.</param>
        /// <param name="y">The y-coord.</param>
        /// <param name="z">The z-coord.</param>
        public void Teleport(float x, float y, float z)
        {
            Teleport(_currentWorld, x, y, z);
        }

        /// <summary>
        /// Teleports the player to a new location.
        /// </summary>
        /// <param name="vector">The vector.</param>
        public void Teleport(Vector3 vector)
        {
            Teleport(_currentWorld, vector);
        }

        /// <summary>
        /// Teleports the player to a new location.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="x">The x-coord.</param>
        /// <param name="y">The y-coord.</param>
        /// <param name="z">The z-coord.</param>
        public void Teleport(World world, float x, float y, float z)
        {
            _currentWorld = world;
            _teleportPosition = new Vector3(x, y, z);
        }

        /// <summary>
        /// Teleports the player to a new location.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="vector">The vector position.</param>
        public void Teleport(World world, Vector3 vector)
        {
            _currentWorld = world;
            _teleportPosition = vector;
        }

        /// <summary>
        /// Opens the players current inventory.
        /// </summary>
        public void OpenInventory()
        {
            var items = _playerInventory.GetItems();
            var grid = GameObject.Find("PI3Grid");
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (item == null)
                    continue;
                var slot = grid.GetComponentAtIndex<Image>(i);
                slot.sprite = item.GetItemType().Icon;
            }

            inventoryMenu.SetActive(true);
            _inventoryOpen = true;
        }

        /// <summary>
        /// Closes the player's current inventory.
        /// </summary>
        public void CloseInventory()
        {
            inventoryMenu.SetActive(false);
            _inventoryOpen = false;
        }

        /// <summary>
        /// Checks if a player's inventory is open.
        /// </summary>
        /// <returns>True if a player's inventory is open.</returns>
        public bool IsInventoryOpen()
        {
            return _inventoryOpen;
        }

        /// <summary>
        /// Freezes the players movement.
        /// </summary>
        public void Freeze()
        {
            _freeze = true;
        }

        /// <summary>
        /// Unfreezes the players' movement.
        /// </summary>
        public void Unfreeze()
        {
            _freeze = false;
        }

        /// <summary>
        /// Checks if the player is frozen.
        /// </summary>
        /// <returns>True if frozen, else false.</returns>
        public bool IsFrozen()
        {
            return _freeze;
        }

        /// <summary>
        /// Makes the resident turn to look at a location.
        /// </summary>
        /// <param name="pos">The location to look at.</param>
        public void TurnTo(Vector3 pos)
        {
            transform.DOLookAt(pos, Vector3.Distance(transform.position, pos) / 5);
            var turn = "turn_" + (IsRightSide(pos) ? "right" : "left");
            _animator.SetTrigger(turn);
        }

        /// <summary>
        /// Gets the chunk the player is currently in.
        /// </summary>
        /// <returns>The chunk the player is in.</returns>
        public Chunk GetChunk()
        {
            return _currentWorld.GetChunkAt((int)transform.position.x, (int)transform.position.z);
        }

        /// <summary>
        /// Gets the direction the player is currently facing.
        /// </summary>
        /// <returns>The direction of the player.</returns>
        public Direction GetDirection()
        {
            var forward = transform.forward.normalized;
            var maxDot = -Mathf.Infinity;
            var bestDirection = Direction.Forward;
            Vector3[] directions =
            {
                Vector3.forward,
                Vector3.back,
                Vector3.right,
                Vector3.left,
                Vector3.up,
                Vector3.down
            };
            Direction[] enums =
            {
                Direction.Forward,
                Direction.Backward,
                Direction.Right,
                Direction.Left,
                Direction.Up,
                Direction.Down
            };
            for (var i = 0; i < directions.Length; i++)
            {
                var dot = Vector3.Dot(forward, directions[i]);
                if (!(dot > maxDot))
                    continue;
                maxDot = dot;
                bestDirection = enums[i];
            }

            return bestDirection;
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

    }
}