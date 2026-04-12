using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerCoinsController))]
public class PlayerInteraction : MonoBehaviour
{
    public bool IsInteracting { get; private set; } = false;

    // Interactables near player
    private readonly List<Interactable> _interactablesInRange = new();
    private Interactable _currentInteractable;

    // Player components
    private PlayerMovement _playerMovement;
    private PlayerCoinsController _playerCoinsController;
    private Animator _animator;

    // Interactable layer
    private int _interactableLayer;

    // State parameters
    private readonly float _longPressDuration = 0.2f;
    private bool _sKeyHeld;
    private float _sKeyDownTime;
    private bool _interactionStartedThisPress;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _playerCoinsController = GetComponent<PlayerCoinsController>();
        _animator = GetComponent<Animator>();
        _interactableLayer = LayerMask.NameToLayer("Interactable");
    }

    private void OnDisable()
    {
        StopCurrentInteraction();
        SetCurrentInteractable(null);
        _interactablesInRange.Clear();
    }

    /// <summary>
    /// Adds a new interactable close to player to the list of interactables
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != _interactableLayer)
            return;

        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable == null)
            return;

        if (!_interactablesInRange.Contains(interactable))
            _interactablesInRange.Add(interactable);
    }

    /// <summary>
    /// Removes an interactable from the list when it is too far away from the player
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer != _interactableLayer)
            return;

        Interactable interactable = other.GetComponent<Interactable>();

        if (interactable == null)
            return;

        // Remove interactable when it is too far away
        _interactablesInRange.Remove(interactable);

        if (_currentInteractable == interactable)
        {
            if (IsInteracting)
                interactable.StopInteraction();

            IsInteracting = false;
            SetCurrentInteractable(null);
        }
    }

    private void Update()
    {
        bool sPressed = Keyboard.current.sKey.isPressed;
        bool sDown = Keyboard.current.sKey.wasPressedThisFrame;
        bool sUp = Keyboard.current.sKey.wasReleasedThisFrame;

        // Key was just pressed
        if (sDown)
        {
            _sKeyHeld = true;
            _sKeyDownTime = Time.time;
            _interactionStartedThisPress = false;
        }

        // Start long-press interaction only once per press
        if (_sKeyHeld
            && !_interactionStartedThisPress
            && CanStartInteraction()
            && Time.time - _sKeyDownTime >= _longPressDuration)
        {
            _currentInteractable.StartInteraction();
            IsInteracting = true;
            _interactionStartedThisPress = true;
        }

        // Stop current interaction when releasing S
        if (IsInteracting && sUp)
        {
            StopCurrentInteraction();
        }

        // If s key is released check for short press
        if (sUp)
        {
            float pressDuration = Time.time - _sKeyDownTime;

            // If it is a short press drop a coin
            if (!_interactionStartedThisPress && pressDuration < _longPressDuration)
                _playerCoinsController.DropCoin();

            _sKeyHeld = false;
        }

        // Only update interactable while idle and no S key is being pressed
        if (!IsInteracting && !sPressed)
        {
            UpdateCurrentInteractable();
        }
    }

    /// <summary>
    /// If player has paid the full price for an interaction play interaction animation
    /// </summary>
    private void OnInteraction(Interactable interactable)
    {
        if (interactable == null)
            return;

        // Stop player movement and start interacting animation
        _playerMovement.StartInteracting();
        _playerMovement.SetState(CharacterStates.Interacting);
        
        if (_currentInteractable == interactable)
            IsInteracting = false;

        if (interactable.IsOneTimeInteractable)
        {
            _interactablesInRange.Remove(interactable);

            if (_currentInteractable == interactable)
                SetCurrentInteractable(null);
        }
    }

    /// <summary>
    /// Check which interactable is the closest to the player.
    /// </summary>
    private void UpdateCurrentInteractable()
    {
        _interactablesInRange.RemoveAll(interactable => interactable == null);

        // Find closest interactable
        float closestDistanceSqr = float.MaxValue;
        Interactable closest = null;
        Vector3 playerPosition = transform.position;

        foreach (Interactable interactable in _interactablesInRange)
        {
            if (!interactable.IsInteractable)
                continue;

            float distanceSqr = (interactable.transform.position - playerPosition).sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closest = interactable;
            }
        }

        if (_currentInteractable == closest)
            return;

        SetCurrentInteractable(closest);
    }

    // Helper methods

    private void SetCurrentInteractable(Interactable newInteractable)
    {
        if (_currentInteractable == newInteractable)
            return;

        if (_currentInteractable != null)
        {
            _currentInteractable.Interaction -= OnInteraction;
            _currentInteractable.OnDeselected();
        }

        _currentInteractable = newInteractable;

        if (_currentInteractable != null)
        {
            _currentInteractable.Interaction += OnInteraction;
            _currentInteractable.OnSelected();
        }
    }

    private void StopCurrentInteraction()
    {
        if (IsInteracting && _currentInteractable != null)
            _currentInteractable.StopInteraction();

        IsInteracting = false;
    }

    private bool CanStartInteraction()
    {
        return _currentInteractable != null
            && _currentInteractable.IsInteractable
            && !IsInteracting 
            && !_playerMovement.IsDashing;
    }
}
