using System.Collections;
using DG.Tweening;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using Random = System.Random;

public class ResidentHandler : MonoBehaviour
{

    public ResidentData data;
    public DialogueData dialogue;

    public float speed = 5;
    public float directionChangeInterval = 1;
    public float maxHeadingChange = 30;

    private readonly Random _random = new();
    private CharacterController _controller;
    private float _heading;
    private Vector3 _targetRotation;
    
    private World _currentWorld;
    private Vector3 _teleportPosition;
    private bool _freeze;
    
    public bool isTalking;

    private TMP_Animated _animatedText;
    private Animator _animator;

    public Transform particlesParent;

    private void Awake()
    {
        data = ScriptableObject.CreateInstance<ResidentData>();
        _animator = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();
        _heading = (float) _random.NextDouble() * 360;
        transform.eulerAngles = new Vector3(0, _heading, 0);
        StartCoroutine(NewHeading());
    }

    private void Start()
    {
        _animatedText = ConversationHandler.Instance.animatedText;
        _animatedText.onAction.AddListener(action => SetAction(action));
    }

    private void Update()
    {
        if (_freeze)
            return;
        transform.eulerAngles = Vector3.Slerp(transform.eulerAngles, _targetRotation, Time.deltaTime * directionChangeInterval);
        var forward = transform.TransformDirection(Vector3.forward);
        _controller.SimpleMove(forward * speed);
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
    /// Sets the current action of this resident.
    /// </summary>
    /// <param name="action">The action the resident should do.</param>
    public void SetAction(string action)
    {
        if (this != ConversationHandler.Instance.currentResident)
            return;
        if (action == "shake")
            Camera.main.GetComponent<CinemachineImpulseSource>().GenerateImpulse();
        else
            PlayParticle(action);
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

    /// <summary>
    /// Resets the resident to an idle state.
    /// </summary>
    public void Idle()
    {
        isTalking = false;
        _animator.SetTrigger("idle");
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
    /// Repeatedly calculates a new direction to move towards.
    /// Use this instead of MonoBehaviour.InvokeRepeating so that the interval can be changed at runtime.
    /// </summary>
    private IEnumerator NewHeading()
    {
        while (true) {
            NewHeadingRoutine();
            yield return new WaitForSeconds(directionChangeInterval);
        }
    }

    /// <summary>
    /// Calculates a new direction to move towards.
    /// </summary>
    private void NewHeadingRoutine()
    {
        var floor = transform.eulerAngles.y - maxHeadingChange;
        var ceil  = transform.eulerAngles.y + maxHeadingChange;
        _heading = (float) _random.NextDouble() * (ceil - floor) + floor;
        _targetRotation = new Vector3(0, _heading, 0);
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
    
}