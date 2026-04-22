using System.Collections;
using UnityEngine;

public class CampInteractable : Interactable
{
    [SerializeField] private GameObject[] states;
    [SerializeField] private StateBuilding[] stateBuildings;

    // Coroutine parameters
    private float _blockInteractionDuration = 60f;

    // State
    private int _currentState = 0;

    protected override void Awake()
    {
        base.Awake();
        SetState(_currentState);
    }

    public override void Interact()
    {
        Upgrade();
    }

    public void Upgrade()
    {
        if (_currentState >= states.Length - 1)
            return;

        isInteractable = false;

        SetState(++_currentState);
        StartCoroutine(BlockInteraction());
    }

    public void SetState(int index)
    {
        for (int i = 0; i < states.Length; i++)
            states[i].SetActive(i == index);

        _currentState = index;

        // Activate all building belonging to this state
        foreach (StateBuilding sb in stateBuildings)
        {
            if (sb.state <= _currentState && !sb.building.activeSelf)
            {
                sb.building.transform.position = sb.position;
                sb.building.SetActive(true);
            }
        }
    }

    // Block interaction coroutine

    private IEnumerator BlockInteraction()
    {
        yield return new WaitForSeconds(_blockInteractionDuration);

        // Increase block duration for the next state
        _blockInteractionDuration *= 2f;

        isInteractable = true;
    }
}
