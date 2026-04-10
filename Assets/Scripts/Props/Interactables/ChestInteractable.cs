using System.Collections;
using UnityEngine;

public class ChestInteractable : Interactable
{
    [Header("Interactor Animator")]
    [SerializeField]
    private Animator animator;

    [Header("Loot Properties")]
    [SerializeField]
    private GameObject loot;
    [SerializeField]
    private Transform lootParent;
    [SerializeField]
    private int lootAmount = 5;
    [SerializeField]
    private float spawnForce = 5f;
    [SerializeField]
    private float spread = 0.5f;

    // Spawn properties
    private readonly float _spawnDelay = 0.1f;
    private Vector3 _spawnPos;
    private WaitForSeconds _spawnWait;

    private void Awake()
    {
        _spawnPos = transform.position + new Vector3(transform.position.x, 2f, transform.position.z);
        _spawnWait = new WaitForSeconds(_spawnDelay);
    }

    /// <summary>
    /// Spawns loot and mark as not interactable
    /// </summary>
    public override void Interact()
    {
        if (!isInteractable)
            return;

        isInteractable = false;

        animator.SetBool("IsOpened", true);

        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        for (int i = 0; i < lootAmount; i++)
        {
            SpawnOne(i);

            yield return _spawnWait;
        }
    }

    /// <summary>
    /// Spawns one loot object and calculates its spawn direction.
    /// </summary>
    /// <param name="index"></param>
    private void SpawnOne(int index)
    {        
        GameObject newLoot = Instantiate(loot, _spawnPos, Quaternion.identity);
        CoinController coinController = newLoot.GetComponent<CoinController>();
        coinController.DropCoinEntity();
        newLoot.transform.SetParent(lootParent);

        if (newLoot.TryGetComponent<Rigidbody2D>(out var rb))
        {
            float offset = GetOffset(index);

            Vector2 direction = new Vector2(offset, 1f).normalized;
            rb.linearVelocity = direction * spawnForce;
        }
    }


    // Helper methods

    /// <summary>
    /// Calculates the spawn direction of a loot object
    /// </summary>
    /// <param name="index">Index of loot object</param>
    /// <returns>Spawn direction of loot object</returns>
    private float GetOffset(int index)
    {
        if (lootAmount <= 1 || index == 0)
            return 0f;

        float step = spread / Mathf.CeilToInt((lootAmount -1) / 2f);

        int pair = (index + 1) / 2;
        int sign = (index % 2) == 1 ? -1 : 1;

        float offset = pair * sign * step;

        return Mathf.Clamp(offset, -spread, spread);
    }
}
