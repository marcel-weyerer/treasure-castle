using System.Collections;
using UnityEditor;
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
    private int lootAmount = 5;
    [SerializeField]
    private float spawnForce = 5f;
    [SerializeField]
    private readonly float spread = 0.5f;

    private readonly float _spawnDelay = 0.1f;
    private Vector3 _spawnPos;

    public override void Interact()
    {
        animator.SetBool("IsOpened", true);

        SpawnCoins();

        isInteractable = false;
    }

    private void SpawnCoins()
    {
        Vector3 chestPos = transform.position;
        _spawnPos = new (chestPos.x, chestPos.y + 1f, chestPos.z);

        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        for (int i = 0; i < lootAmount; i++)
        {
            SpawnOne(i);

            yield return new WaitForSeconds(_spawnDelay);
        }
    }

    private void SpawnOne(int index)
    {        
        GameObject newCoin = Instantiate(loot, _spawnPos, Quaternion.identity);
        newCoin.transform.SetParent(GameObject.FindGameObjectWithTag("CoinParent").transform);

        Rigidbody2D rb = newCoin.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float offset = GetOffset(index);

            Vector2 direction = new Vector2(offset, 1f).normalized;
            rb.linearVelocity = direction * spawnForce;
        }
    }

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
