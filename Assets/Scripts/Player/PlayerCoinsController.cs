using UnityEngine;

public class PlayerCoinsController : MonoBehaviour
{
    [SerializeField]
    private int playerCoins;
    [SerializeField]
    private GameObject coinPrefab;
    [SerializeField]
    private float dropForce = 2f;

    public int PlayerCoins => playerCoins;

    public void AddCoin() => playerCoins++;

    public void RemoveCoin() 
    {
        if (playerCoins <= 0)
        {
            Debug.LogError("Not enough coins to spend.");
            return;
        }
        
        playerCoins--;
    }

    /// <summary>
    /// Drops a coin as a player drop and removes it from the player purse
    /// </summary>
    public void DropCoin()
    {
        if (playerCoins <= 0)
            return;

        // Spawn new coin as player drop
        GameObject newCoin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
        CoinController coinController = newCoin.GetComponent<CoinController>();
        coinController.DropCoinPlayer();

        Rigidbody2D coinRB = newCoin.GetComponent<Rigidbody2D>();
        coinRB.linearVelocity = new Vector2(0f, 1f).normalized * dropForce;

        RemoveCoin();
    }
}
