using System.Collections;
using UnityEditor;
using UnityEngine;

public class ChestInteractable : Interactable
{
    [SerializeField]
    private Animator animator;

    [Header("Coin Properties")]
    [SerializeField]
    private GameObject coin;
    [SerializeField]
    private int coinAmount = 5;
    [SerializeField]
    private float spawnForce = 5f;

    private Vector3 _spawnPos;
    private readonly float _spread = 0.5f;
    private readonly float _spawnDelay = 0.1f;

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
        for (int i = 0; i < coinAmount; i++)
        {
            SpawnOne();

            yield return new WaitForSeconds(_spawnDelay);
        }
    }

    private void SpawnOne()
    {        
        GameObject newCoin = Instantiate(coin, _spawnPos, Quaternion.identity);
        newCoin.transform.SetParent(GameObject.FindGameObjectWithTag("CoinParent").transform);

        Rigidbody2D rb = newCoin.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float randomX = Random.Range(-_spread, _spread);
            Vector2 direction = new Vector2(randomX, 1f).normalized;

            rb.linearVelocity = direction * spawnForce;
        }
    }
}
