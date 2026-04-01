using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
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

    [Header("Components")]
    [SerializeField]
    private Animator animator;

    private float _lastLeftTapTime = 0f;
    private float _lastRightTapTime = 0f;

    // Dash properties
    private bool _dashAllowed = true;
    private bool _isDashing = false;
    private Coroutine _dashRoutine;
    private Coroutine _cooldownRoutine;

    private SpriteRenderer _spriteRenderer;
    private LookDirection _currentLookDirection;

    // Player turn event
    public event Action<LookDirection> PlayerTurn;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentLookDirection = LookDirection.Right;
    }

    void Start()
    {
        PlayerTurn?.Invoke(LookDirection.Right);
    }

    void Update()
    {
        bool moveLeft = Keyboard.current.aKey.isPressed;
        bool moveRight = Keyboard.current.dKey.isPressed;

        if (moveLeft && moveRight) 
        {
            if (!_isDashing)
                animator.SetInteger("CharacterState", (int)CharacterStates.Idle);
            return;
        }

        if (moveLeft)
        {
            FaceLeft();

            if (_dashAllowed && !_isDashing && Keyboard.current.aKey.wasPressedThisFrame && CheckDoubleTap(ref _lastLeftTapTime)) 
            {
                Dash(-1f);
                return;
            }
            
            if (!_isDashing)
                Move(-1f);
        } 
        else if (moveRight)
        {
            FaceRight();

            if (_dashAllowed && !_isDashing && Keyboard.current.dKey.wasPressedThisFrame && CheckDoubleTap(ref _lastRightTapTime)) 
            {
                Dash(1f);
                return;
            }
            
            if (!_isDashing)
                Move(1f);
        }
        else
        {
            if (!_isDashing)
                animator.SetInteger("CharacterState", (int)CharacterStates.Idle);
        }
    }

    // Movement Methods

    private void Move(float direction)
    {
        // Set animator state
        animator.SetInteger("CharacterState", (int)CharacterStates.Running);

        // Move player
        transform.position += new Vector3(direction * moveSpeed * Time.deltaTime, 0f, 0f);
    }

    private void Dash(float direction)
    {
        if (_dashRoutine != null)
            StopCoroutine(_dashRoutine);

        _dashRoutine = StartCoroutine(DashCoroutine(direction));
    }

    // Helper Methods

    private void FaceLeft()
    {
        if (_currentLookDirection == LookDirection.Left)
            return;

        _currentLookDirection = LookDirection.Left;
        _spriteRenderer.flipX = true;

        // Send player turn event
        PlayerTurn?.Invoke(LookDirection.Left);
    }

    private void FaceRight()
    {
        if (_currentLookDirection == LookDirection.Right)
            return;

        _currentLookDirection = LookDirection.Right;
        _spriteRenderer.flipX = false;

        // Send player turn event
        PlayerTurn?.Invoke(LookDirection.Right);
    }

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

    private IEnumerator DashCoroutine(float direction)
    {
        _isDashing = true;
        _dashAllowed = false;

        // Set animator state
        animator.SetInteger("CharacterState", (int)CharacterStates.Dash);

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

        StartCoroutine(DashCooldown());

        _isDashing = false;
        _dashRoutine = null;
    }

    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);

        _dashAllowed = true;
    }
}

public enum LookDirection {Left, Right};

public enum CharacterStates
{
    Idle = 0,
    Running = 1,
    Dash = 2
}
