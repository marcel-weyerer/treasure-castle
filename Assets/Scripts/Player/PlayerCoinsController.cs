using UnityEngine;

public class PlayerCoinsController : MonoBehaviour
{
    [SerializeField]
    private int playerCoins;

    public int PlayerCoins => playerCoins;

    public void SpendCoins(int amount)
    {
        if (amount <= 0)
            return;
        if (amount > playerCoins)
        {
            Debug.LogError("Too much spent.");
            return;
        }

        playerCoins -= amount;
    }
}
