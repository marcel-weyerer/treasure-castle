using System;
using UnityEngine;

public class TreeInteractable : Interactable
{
    public static event Action<int> FellTree;

    public override void Interact()
    {
        if (!isInteractable)
            return;

        isInteractable = false;

        FellTree?.Invoke((int)transform.position.x);
    }

    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
    }
}
