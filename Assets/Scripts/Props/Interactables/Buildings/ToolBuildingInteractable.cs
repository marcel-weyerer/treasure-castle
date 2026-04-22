using System.Collections.Generic;
using UnityEngine;

public class ToolBuildingInteractable : Interactable
{
    [Header("Tool properties")]
    [SerializeField] private GameObject toolPrefab;
    [SerializeField] private int maxToolAmount = 4;
    [SerializeField] private Transform[] localToolPositions;
    [SerializeField] private Transform toolParent;

    private readonly Stack<GameObject> _availableTools = new();
    private int _currentAmount;

    protected override void Awake()
    {
        base.Awake();

        // Initialize values
        _currentAmount = 0;
        SetIsInteractable(_currentAmount < maxToolAmount);
    }

    public override void Interact()
    {
        if (_currentAmount >= maxToolAmount)
        {
            Debug.LogError("No many tools assigned.");
            return;
        }

        // Spawn a new tool at the next position
        GameObject newTool = Instantiate(toolPrefab, localToolPositions[_currentAmount].position, toolPrefab.transform.rotation);
        newTool.transform.SetParent(toolParent);
        _currentAmount++;
        
        _availableTools.Push(newTool);

        // Check if it can still be interactable
        bool interactable = _currentAmount < maxToolAmount;
        if (IsInteractable != interactable)
        {
            SetIsInteractable(interactable);

            if (interactable)
                OnSelected();
            else
                OnDeselected();
        }
    }

    public void PickUpTool()
    {
        if (_availableTools.Count == 0)
            return;

        GameObject removedTool = _availableTools.Pop();
        Destroy(removedTool);
        _currentAmount--;

        // Check if it can be interactable again
        bool interactable = _currentAmount < maxToolAmount;
        if (IsInteractable != interactable)
        {
            SetIsInteractable(interactable);

            if (interactable)
                OnSelected();
            else
                OnDeselected();
        }
    }
}
