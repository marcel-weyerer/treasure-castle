using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private Animator animator;

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
            animator.SetInteger("CharacterState", (int)CharacterStates.Idle);
            return;
        }

        if (moveLeft)
        {
            if (_currentLookDirection != LookDirection.Left)
            {
                _currentLookDirection = LookDirection.Left;
                _spriteRenderer.flipX = true;

                // Send player turn event
                PlayerTurn?.Invoke(LookDirection.Left);
            }

            animator.SetInteger("CharacterState", (int)CharacterStates.Running);

            Move(-1f);
        } 
        else if (moveRight)
        {
            if (_currentLookDirection != LookDirection.Right)
            {
                _currentLookDirection = LookDirection.Right;
                _spriteRenderer.flipX = false;

                // Send player turn event
                PlayerTurn?.Invoke(LookDirection.Right);
            }

            animator.SetInteger("CharacterState", (int)CharacterStates.Running);

            Move(1f);
        }
        else
        {
            animator.SetInteger("CharacterState", (int)CharacterStates.Idle);
        }
    }

    private void Move(float direction)
    {
        transform.position += new Vector3(direction * moveSpeed * Time.deltaTime, 0f, 0f);
    }
}

public enum LookDirection {Left, Right};

public enum CharacterStates
{
    Idle = 0,
    Running = 1
}
