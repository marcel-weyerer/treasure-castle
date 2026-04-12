using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(PlayerInteraction))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField]
    private float moveSpeed = 5f;
    [SerializeField]
    private float dashSpeed = 12f;
    [SerializeField]
    private float dashDuration = 0.5f;
    [SerializeField]
    private float dashCooldown = 3f;
    [SerializeField]
    private float doubleTapTime = 0.2f;

    // Last tap time
    private float _lastLeftTapTime = 0f;
    private float _lastRightTapTime = 0f;

    // Action properties
    private bool _dashAllowed = true;
    public bool IsDashing { get; private set; } = false;
    private bool _isInteracting = false;

    // Coroutines
    private Coroutine _dashRoutine;
    private Coroutine _cooldownRoutine;

    // Player components
    private PlayerInteraction _playerInteraction;
    private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private LookDirection _currentLookDirection;

    // Player turn event
    public event Action<LookDirection> PlayerTurn;

    // Animator parameters
    private static readonly int CharacterState = Animator.StringToHash("CharacterState");
    private CharacterStates _currentState;

    private void Awake()
    {
        _playerInteraction = GetComponent<PlayerInteraction>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        _currentLookDirection = LookDirection.Right;
    }

    private void Update()
    {
        if (_isInteracting)
            return;

        bool moveLeft = Keyboard.current.aKey.isPressed;
        bool moveRight = Keyboard.current.dKey.isPressed;

        float direction = 0f;
        bool dashPressedThisFrame = false;

        if (moveLeft && !moveRight)
        {
            direction = -1;
            dashPressedThisFrame = Keyboard.current.aKey.wasPressedThisFrame;
        }
        else if (moveRight && !moveLeft)
        {
            direction = 1;
            dashPressedThisFrame = Keyboard.current.dKey.wasPressedThisFrame;
        }

        // When no or both move buttons are pressed don't move
        if (direction == 0f)
        {
            if (!IsDashing)
                SetState(CharacterStates.Idle);

            return;
        }

        // Make player face in the current look direction
        LookDirection lookDirection = direction < 0f ? LookDirection.Left : LookDirection.Right;
        FaceInDirection(lookDirection);

        if (ShouldDash(direction, dashPressedThisFrame))
        {
            Dash(direction);
            return;
        }

        if (!IsDashing)
            Move(direction);
    }

    // Movement Methods

    /// <summary>
    /// Makes player move continuously in direction
    /// </summary>
    /// <param name="direction">Move direction</param>
    private void Move(float direction)
    {
        // Set animator state
        SetState(CharacterStates.Running);

        // Move player
        transform.position += new Vector3(direction * moveSpeed * Time.deltaTime, 0f, 0f);
    }

    /// <summary>
    /// Lets player dash in direction
    /// </summary>
    /// <param name="direction">Dash direction</param>
    private void Dash(float direction)
    {
        if (_dashRoutine != null)
            StopCoroutine(_dashRoutine);
        _dashRoutine = StartCoroutine(DashCoroutine(direction));
    }

    // Helper Methods

    /// <summary>
    /// Makes the character look in the current look direction.
    /// </summary>
    /// <param name="direction">Player look direction</param>
    private void FaceInDirection(LookDirection direction)
    {
        if (_currentLookDirection == direction)
            return;

        // Update look direction and flip player sprite if player is facing left
        _currentLookDirection = direction;
        _spriteRenderer.flipX = direction == LookDirection.Left;

        // Send player turn event
        PlayerTurn?.Invoke(direction);
    }

    /// <summary>
    /// Set animation state.
    /// </summary>
    /// <param name="state">New character state</param>
    public void SetState(CharacterStates state)
    {
        if (_currentState == state)
            return;

        _currentState = state;
        _animator.SetInteger(CharacterState, (int)state);
    }

    public void StartInteracting()
    {
        _isInteracting = true;
    }

    public void StopInteracting()
    {
        SetState(CharacterStates.Idle);
        _isInteracting = false;
    }

    /// <summary>
    /// Checks weather the player should dash.
    /// </summary>
    /// <param name="direction">Look direction of player</param>
    /// <param name="dashPressedThisFrame">If dash button has been pressed this frame</param>
    /// <returns>True if a dash has been performed, false otherwise</returns>
    private bool ShouldDash(float direction, bool dashPressedThisFrame)
    {
        if (!dashPressedThisFrame || !CanDash)
            return false;

        if (direction < 0f)
            return CheckDoubleTap(ref _lastLeftTapTime);

        return CheckDoubleTap(ref _lastRightTapTime);
    }

    private bool CanDash => _dashAllowed && !IsDashing;

    /// <summary>
    /// Checks weather player has double tapped for a dash.
    /// </summary>
    /// <param name="lastTapTime">Last time the move button was tapped</param>
    /// <returns>true, if the player has double tapped, false otherwise</returns>
    private bool CheckDoubleTap(ref float lastTapTime)
    {
        if (Time.time - lastTapTime <= doubleTapTime) 
        {
            lastTapTime = 0f;
            return true;
        }

        lastTapTime = Time.time;
        return false;
    }

    // Coroutines

    /// <summary>
    /// Performs a dash in direction.
    /// </summary>
    /// <param name="direction">Look direction of player</param>
    private IEnumerator DashCoroutine(float direction)
    {
        IsDashing = true;
        _dashAllowed = false;

        // Set animator state
        SetState(CharacterStates.Dash);

        float time = 0f;

        while (time < dashDuration)
        {
            transform.position += new Vector3(direction * dashSpeed * Time.deltaTime, 0f, 0f);

            time += Time.deltaTime;
            yield return null;
        }
        
        // Start dash cooldown
        if (_cooldownRoutine != null)
            StopCoroutine(_cooldownRoutine);
        _cooldownRoutine = StartCoroutine(DashCooldown());

        IsDashing = false;
        _dashRoutine = null;
    }

    /// <summary>
    /// Waits for given seconds before another dash can be performed.
    /// </summary>
    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);

        _dashAllowed = true;
        _cooldownRoutine = null;
    }
}

// Look direction of the player
public enum LookDirection {Left, Right};

// Player states used to trigger animations
public enum CharacterStates
{
    Idle = 0,
    Running = 1,
    Dash = 2,
    Interacting = 3
}
