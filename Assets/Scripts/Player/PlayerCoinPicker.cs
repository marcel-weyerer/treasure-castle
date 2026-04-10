using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCoinPicker : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private PlayerCoinsController coinsController;

    private readonly float _collectDuration = 0.15f;

    private readonly List<GameObject> _unpickableCoins = new();
    private readonly HashSet<GameObject> _coinsBeingCollected = new();

    /// <summary>
    /// Starts collecting if it is a coin.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Coin"))
            return;

        GameObject coin = other.gameObject;

        if (_coinsBeingCollected.Contains(coin))
            return;

        if (!coin.TryGetComponent(out CoinController coinController))
            return;

        if (coinController.IsPickable)
            StartCoroutine(CollectCoin(other.gameObject));
        else
            _unpickableCoins.Add(other.gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Coin"))
            return;

        _unpickableCoins.Remove(other.gameObject);
    }

    private void Update()
    {
        if (_unpickableCoins.Count == 0)
            return;

        for (int i = _unpickableCoins.Count - 1; i >= 0; i--)
        {
            GameObject coin = _unpickableCoins[i];

            if (coin == null)
            {
                _unpickableCoins.RemoveAt(i);
                continue;
            }

            if (_coinsBeingCollected.Contains(coin))
            {
                _unpickableCoins.RemoveAt(i);
                continue;
            }

            if (!coin.TryGetComponent(out CoinController coinController))
            {
                _unpickableCoins.RemoveAt(i);
                continue;
            }

            if (coinController.IsPickable)
            {
                _unpickableCoins.RemoveAt(i);
                StartCollectCoin(coin);
            }
        }
    }

    private void StartCollectCoin(GameObject coin)
    {
        if (coin == null || _coinsBeingCollected.Contains(coin))
            return;

        _coinsBeingCollected.Add(coin);
        StartCoroutine(CollectCoin(coin));
    }

    /// <summary>
    /// Smoothly moves the coin object to the player position before collecting it.
    /// </summary>
    /// <param name="coin">Coin GameObject to collect</param>
    private IEnumerator CollectCoin(GameObject coin)
    {
        Vector3 startPos = coin.transform.position;
        Vector3 targetPos = player.transform.position;

        // Stop physics for coin while it is collected
        coin.GetComponent<Rigidbody2D>().simulated = false;

        float time = Time.deltaTime;

        while (time < _collectDuration)
        {
            float t = time / _collectDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            coin.transform.position = Vector3.Lerp(startPos, targetPos, smoothT);

            time += Time.deltaTime;
            yield return null;
        }

        // Add coin to player purse and destroy the coin object
        if (coin != null)
        {
            coinsController.AddCoin();
            _coinsBeingCollected.Remove(coin);
            Destroy(coin);
        }
    }
}
