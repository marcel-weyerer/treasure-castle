using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Cost Properties")]
    [SerializeField] private int interactionCost;
    [SerializeField] private GameObject costPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private float costYOffset = 0f;

    [Header("Interactable")]
    [SerializeField] private bool isOneTimeInteractable = true;
    [SerializeField] protected bool isInteractable = true;

    // Coroutine parameters
    private Coroutine _spawnCoroutine;
    private Coroutine _spendCoroutine;
    private const float ShowDuration = 0.1f;
    private const float SpawnDelay   = 0.01f;
    private const float SpendDelay   = 0.2f;

    private GameObject _player;
    private PlayerCoinsController _coinsController;
    private Transform _coinParent;
    private Transform _costParent;

    private readonly List<GameObject> _costPrefabs = new();
    private readonly List<GameObject> _coinPrefabs  = new();

    // Events
    public event Action<Interactable> Interaction;

    public int InteractionCost => interactionCost;
    public bool IsInteractable => isInteractable;
    public bool IsOneTimeInteractable => isOneTimeInteractable;

    protected virtual void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player");

        _coinsController = _player.GetComponent<PlayerCoinsController>();
        _coinParent = GameObject.FindGameObjectWithTag("CoinParent").transform;
        _costParent = GameObject.FindGameObjectWithTag("CostParent").transform;
    }

    /// <summary>
    /// Called by the player to trigger this interactable's effect.
    /// </summary>
    public abstract void Interact();

    public virtual void StartInteraction()
    {
        if (_spendCoroutine != null)
            return;

        _spendCoroutine = StartCoroutine(SpendCoins(GetCostPositions()));
    }

    public virtual void StopInteraction()
    {
        if (_spendCoroutine != null)
        {
            StopCoroutine(_spendCoroutine);
            _spendCoroutine = null;
        }


        ReleaseCoins();
    }

    public virtual void OnSelected()
    {
        _spawnCoroutine = StartCoroutine(ShowCostSequence(GetCostPositions()));
    }

    public virtual void OnDeselected()
    {
        if (_spawnCoroutine != null)
            StopCoroutine(_spawnCoroutine);

        DestroyList(_costPrefabs);
    }

    // Helper functions

    /// <summary>
    /// Selects the correct CostPositions array based on the interaction cost.
    /// Cost <= 10: use the Under-Ten layouts (uneven or even).
    /// Cost > 10: use the Over-Ten  layouts (uneven or even).
    /// </summary>
    private Vector3[] GetCostPositions()
    {
        bool isOver10 = interactionCost > 10;
        bool isOdd    = (interactionCost % 2) != 0;

        return (isOver10, isOdd) switch
        {
            (false, true)  => CostPositions.UnevenUnderTen,
            (false, false) => CostPositions.EvenUnderTen,
            (true,  true)  => CostPositions.UnevenOverTen,
            (true,  false) => CostPositions.EvenOverTen,
        };
    }

    // Coin Spending Coroutines

    private IEnumerator SpendCoins(Vector3[] positions)
    {
        int spentCoins = 0;
        bool outOfCoins = false;

        foreach ((int start, int end) in GetActiveRanges(positions))
        {
            for (int i = start; i <= end; i++)
            {
                if (_coinsController.PlayerCoins <= 0)
                {
                    outOfCoins = true;
                    break;
                }

                StartCoroutine(SpendOneCoin(GetWorldTarget(positions[i])));
                spentCoins++;

                if (spentCoins < interactionCost)
                    yield return new WaitForSeconds(SpendDelay);
            }

            if (outOfCoins) break;
        }

        if (spentCoins == interactionCost)
        {
            // Wait one frame per coin animation duration
            yield return new WaitForSeconds(ShowDuration);

            if (this == null) yield break;

            DestroyList(_coinPrefabs);
            Interact();
            Interaction?.Invoke(this);
        }
        else
        {
            StopInteraction();
        }

        _spendCoroutine = null;
    }

    private IEnumerator SpendOneCoin(Vector3 targetPos)
    {
        // Capture world position before reparenting to avoid local-space mismatch
        Vector3 startPos = _player.transform.position;

        GameObject coin = Instantiate(coinPrefab, startPos, Quaternion.identity);
        coin.transform.SetParent(_coinParent);
        coin.GetComponent<CoinController>().DropCoinEntity();
        coin.GetComponent<Rigidbody2D>().simulated = false;

        _coinPrefabs.Add(coin);
        _coinsController.SpendCoin();

        yield return LerpPosition(coin, startPos, targetPos, ShowDuration);

        if (coin != null)
            coin.transform.position = targetPos;
    }

    // Cost Display Coroutines

    private IEnumerator ShowCostSequence(Vector3[] positions)
    {
        foreach ((int start, int end) in GetActiveRanges(positions))
        {
            for (int i = start; i <= end; i++)
            {
                StartCoroutine(ShowCostPrefab(GetWorldTarget(positions[i])));
                yield return new WaitForSeconds(SpawnDelay);
            }
        }
    }

    private IEnumerator ShowCostPrefab(Vector3 endPos)
    {
        Vector3 spawnPos = endPos - new Vector3(0f, 0.5f, 0f);

        GameObject prefab = Instantiate(costPrefab, spawnPos, Quaternion.identity);
        prefab.transform.SetParent(_costParent);
        _costPrefabs.Add(prefab);

        yield return LerpPosition(prefab, spawnPos, endPos, ShowDuration);

        if (prefab != null)
            prefab.transform.position = endPos;
    }

    // Shared Lerp Helper

    /// <summary>
    /// Smoothly moves <paramref name="obj"/> in world space from
    /// <paramref name="from"/> to <paramref name="to"/> over
    /// <paramref name="duration"/> seconds.
    /// </summary>
    private static IEnumerator LerpPosition(
        GameObject obj, Vector3 from, Vector3 to, float duration)
    {
        float elapsed = Time.deltaTime;

        while (elapsed < duration)
        {
            if (obj == null)
                yield break;

            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            obj.transform.position = Vector3.Lerp(from, to, t);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    // Range Calculation

    /// <summary>
    /// Returns the (startIndex, endIndex) ranges inside <paramref name="positions"/>
    /// that correspond to the active cost slots for the current
    /// <see cref="interactionCost"/>. Yields one range for costs <= 10,
    /// two ranges (bottom row then top row) for costs > 10.
    /// </summary>
    private IEnumerable<(int start, int end)> GetActiveRanges(Vector3[] positions)
    {
        if (interactionCost > 10)
        {
            int bottomCount = 2 * ((interactionCost + 1) / 4);
            int topCount    = interactionCost - bottomCount;

            int bottomStart = (10 - bottomCount) / 2;
            yield return (bottomStart, bottomStart + bottomCount - 1);

            int topStart = (positions.Length - 10 - topCount) / 2 + 10;
            yield return (topStart, topStart + topCount - 1);
        }
        else
        {
            int start = (positions.Length - interactionCost) / 2;
            yield return (start, start + interactionCost - 1);
        }
    }

    // Utility

    /// <summary>
    /// Converts a cost-grid offset into a world position.
    /// </summary>
    private Vector3 GetWorldTarget(Vector3 offset) =>
        transform.position + offset + new Vector3(0f, costYOffset, 0f);

    /// <summary>Re-enables physics on in-flight coins and clears the list.</summary>
    private void ReleaseCoins()
    {
        foreach (GameObject coin in _coinPrefabs)
        {
            if (coin == null) continue;
            Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
            if (rb != null) rb.simulated = true;
        }

        _coinPrefabs.Clear();
    }

    /// <summary>
    /// Destroys every object in <paramref name="list"/> and clears it.
    /// </summary>
    private static void DestroyList(List<GameObject> list)
    {
        foreach (GameObject go in list)
            Destroy(go);

        list.Clear();
    }
}
