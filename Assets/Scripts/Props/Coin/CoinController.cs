using System.Collections;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    // State parameters
    public bool IsPickable { get; private set; } = false;
    public bool IsPlayerDrop { get; private set; } = false;

    // Coroutine parameters
    private float _pickUpBlockDuration = 1f;

    /// <summary>
    /// Sets up a shorter pick up block when coin is dropped by an entity.
    /// </summary>
    public void DropCoinEntity()
    {
        IsPlayerDrop = false;
        _pickUpBlockDuration = 1f;

        StartCoroutine(WaitTillPickable());
    }

    /// <summary>
    /// Sets up a longer pick up block when coin is dropped by a player.
    /// </summary>
    public void DropCoinPlayer()
    {
        IsPlayerDrop = true;
        _pickUpBlockDuration = 3f;

        StartCoroutine(WaitTillPickable());
    }

    private IEnumerator WaitTillPickable()
    {
        yield return new WaitForSeconds(_pickUpBlockDuration);

        IsPickable = true;
    }
}
