using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;

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
            return;

        if (moveLeft)
        {
            if (_currentLookDirection != LookDirection.Left)
            {
                _currentLookDirection = LookDirection.Left;
                _spriteRenderer.flipX = true;

                // Send player turn event
                PlayerTurn?.Invoke(LookDirection.Left);
            }

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

            Move(1f);
        }
    }

    private void Move(float direction)
    {
        transform.position += new Vector3(direction * moveSpeed * Time.deltaTime, 0f, 0f);
    }
}

public enum LookDirection {Left, Right};
