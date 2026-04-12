using UnityEngine;

public class PlayerCoinsController : MonoBehaviour
{
    [SerializeField] private int playerCoins;
    [SerializeField] private CoinPurseUI coinPurseUI;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private float dropForce = 2f;

    private Transform _coinParent;

    public int PlayerCoins => playerCoins;

    private void Start()
    {
        _coinParent = GameObject.FindGameObjectWithTag("CoinParent").transform;
    }

    public void AddCoin()
    {
        playerCoins++;
        coinPurseUI.AddCoin();
    }

    /// <summary>
    /// Decreases the player coins.
    /// </summary>
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
    /// Spends a coin on an interactable and removes it from the player purse.
    /// </summary>
    public void SpendCoin()
    {
        RemoveCoin();
        coinPurseUI.RemoveCoin();
    }

    /// <summary>
    /// Drops a coin as a player drop and removes it from the player purse.
    /// </summary>
    public void DropCoin(bool removeFromPurse = true)
    {
        if (playerCoins <= 0)
            return;

        // Spawn new coin as player drop
        GameObject newCoin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
        CoinController coinController = newCoin.GetComponent<CoinController>();
        coinController.DropCoinPlayer();
        newCoin.transform.SetParent(_coinParent);

        Rigidbody2D coinRB = newCoin.GetComponent<Rigidbody2D>();
        coinRB.linearVelocity = new Vector2(0f, 1f).normalized * dropForce;

        RemoveCoin();

        if (removeFromPurse)
            coinPurseUI.RemoveCoin();
    }
}
