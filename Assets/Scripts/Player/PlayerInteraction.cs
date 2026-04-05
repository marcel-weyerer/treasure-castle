using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCoinsController))]
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    public bool IsInteracting { get; set; } = false;

    private readonly List<Interactable> _interactablesInRange = new();
    private Interactable _currentInteractable;

    private PlayerMovement _playerMovement;
    private PlayerCoinsController _coinController;

    void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _coinController = GetComponent<PlayerCoinsController>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Interactable"))
            return;

        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable != null && interactable.IsInteractable && !_interactablesInRange.Contains(interactable))
            _interactablesInRange.Add(interactable);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Interactable"))
            return;

        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable != null)
        {
            _interactablesInRange.Remove(interactable);

            if (_currentInteractable == interactable)
            {
                interactable.OnExitRange();
                _currentInteractable = null;
            }
        }
    }

    void Update()
    {
        UpdateCurrentInteractable();

        if (_currentInteractable != null && !_playerMovement.IsDashing && Keyboard.current.sKey.wasPressedThisFrame && _coinController.PlayerCoins >= _currentInteractable.InteractionCost)
        {
            _coinController.SpendCoins(_currentInteractable.InteractionCost);
            _currentInteractable.Interact();
            StartInteraction();

            if (!_currentInteractable.IsInteractable)
            {
                _interactablesInRange.Remove(_currentInteractable);
                _currentInteractable.OnExitRange();
                _currentInteractable = null;
            }
        }
    }

    private void UpdateCurrentInteractable()
    {
        if (_interactablesInRange.Count == 0)
        {
            _currentInteractable = null;
            return;
        }

        float closestDistance = float.MaxValue;
        Interactable closest = null;

        foreach (Interactable interactable in _interactablesInRange)
        {
            if (interactable == null)
                continue;

            float distance = Vector2.Distance(transform.position, interactable.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        if (_currentInteractable == closest)
            return;

        _currentInteractable?.OnExitRange();
        _currentInteractable = closest;
        _currentInteractable.OnEnterRange();
    }

    private void StartInteraction()
    {
        IsInteracting = true;
        animator.SetInteger("CharacterState", (int)CharacterStates.Interacting);
    }

    private void StopInteraction()
    {
        IsInteracting = false;
    }
}
