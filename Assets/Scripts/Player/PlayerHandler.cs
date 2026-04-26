using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Handles player information and interactions with its data.
/// </summary>
public class PlayerHandler : MonoBehaviour
{
    [Header("Movement Settings")] 
    public float speed = 5F;
    public float sensitivity = 0.5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Inventory Settings")] 
    public GameObject inventoryMenu;

    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _lookAction;

    private Vector3 _velocity;

    private CharacterController _characterController;
    private Animator _animator;
    private Camera _mainCamera;

    private PlayerData _playerData;

    private PlayerInventory _playerInventory;
    private bool _inventoryOpen;

    private World _currentWorld;
    private bool _isSpawned;
    private Vector3 _teleportPosition = Vector3.zero;

    private void Awake()
    {
        // TODO check for transferred presence.
        _playerData = PlayerData.Deserialize(DDOLTransmitter.Instance.RetrieveTransferredPlayer());
        _playerInventory = new PlayerInventory();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _mainCamera = GameObject.Find("Camera").GetComponent<Camera>();
        _moveAction = InputSystem.actions.FindAction("Move");
        _jumpAction = InputSystem.actions.FindAction("Jump");
        _lookAction = InputSystem.actions.FindAction("Look");
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

    public void Teleport(float x, float y, float z)
    {
        Teleport(_currentWorld, x, y, z);
    }

    public void Teleport(World world, float x, float y, float z)
    {
        _currentWorld = world;
        _teleportPosition = new Vector3(x, y, z);
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
            slot.sprite = item.GetItemType().icon;
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

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Movement
        var input = _moveAction.ReadValue<Vector2>();
        var move = new Vector3(input.x, 0, input.y);
        move = _mainCamera.transform.TransformDirection(move);
        _characterController.Move(move * speed * Time.deltaTime);
        // Gravity
        if (_characterController.isGrounded && _velocity.y < 0)
            _velocity.y = -2f;
        _velocity.y += gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
        // Jumping
        if (_jumpAction.triggered && _characterController.isGrounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        // Animation
        // anim.SetFloat("run", direction.magnitude); // TODO setup player animations.
        
        // Camera Rotation for Mouse Look
        var inputL = _lookAction.ReadValue<Vector2>();
        var mouseX = inputL.x * sensitivity;
        var mouseY = inputL.y * sensitivity; // TODO y axis wont budge, check constraints.
        var currentRotation = transform.localEulerAngles;
        currentRotation.y += mouseX;
        transform.localRotation = Quaternion.AngleAxis(currentRotation.y, Vector3.up);
        var currentCameraRotation = _mainCamera.gameObject.transform.eulerAngles;
        currentCameraRotation.x -= mouseY;
        _mainCamera.gameObject.transform.localRotation = Quaternion.AngleAxis(currentCameraRotation.x, Vector3.right);
    }

    private void LateUpdate()
    {
        // Safe Teleporting for CharacterController
        if (_teleportPosition == Vector3.zero)
            return;
        _characterController.enabled = false;
        transform.position = _teleportPosition;
        _teleportPosition = Vector3.zero;
        _characterController.enabled = true;
    }
    
}