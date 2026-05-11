using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueTrigger : MonoBehaviour
{
    
    [SerializeField] public GameObject uiContainer;
    [SerializeField] public TMP_Text buttonText; // Used to assign correct keybind parameters.
    private ConversationHandler _handler;
    private ResidentHandler _resident;
    private PlayerHandler _player;
    private CameraController _cameraController;
    public CinemachineTargetGroup target;
    private InputAction _interactAction;

    private void Awake()
    {
        _handler = ConversationHandler.Instance;
        _player = GetComponent<PlayerHandler>();
        _interactAction = InputSystem.actions.FindAction("Interact");
        // TODO in future, scale box collidor with size of mesh. 
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Resident"))
            return;
        _resident = other.GetComponent<ResidentHandler>();
        _handler.currentResident = _resident;
        if (!_resident.isTalking)
        {
            buttonText.text = _interactAction.bindings[Gamepad.current == null ? 0 : 1].effectivePath;
            uiContainer.SetActive(true);
        } else if (_interactAction.triggered && !_handler.inDialogue && _resident != null)
        {
            uiContainer.SetActive(false);
            target.Targets[1].Object = _resident.transform;
            _player.Freeze();
            _handler.SetupBubble();
            _handler.inDialogue = true;
            _resident.isTalking = true;
            _player.TurnTo(_resident.transform.position);
            _resident.TurnTo(transform.position);
            if (_cameraController == null)
                _cameraController = Camera.main.GetComponent<CameraController>();
            _cameraController.ShowZoomBetween(_player.transform.position, _resident.transform.position);
            _handler.ClearText();
            _handler.FadeUI(true, 0.2f, 0.65f);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Resident"))
            return;
        _resident = null;
        _handler.currentResident = null;
        uiContainer.SetActive(false);
    }
    
}