using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    protected bool isInteractable = true;

    public bool IsInteractable => isInteractable;

    // Called by the player
    public abstract void Interact();

    public virtual void OnEnterRange() {}
    public virtual void OnExitRange() {}
}
