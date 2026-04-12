using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPurseUI : MonoBehaviour
{
    [Header("Stacks")]
    [SerializeField] private Transform[] stacks;
    [SerializeField] private float[] stackWeights;
    [SerializeField] private float coinHeight = 15f;

    [Header("Purse Settings")]
    [SerializeField] private int maxCoinsInPurse = 30;
    [SerializeField] private int purseSafeThreshold = 24;   // 0% fail below this

    [Header("Stack Settings")]
    [SerializeField] private int maxCoinsPerStack = 9;
    [SerializeField] private int stackSafeThreshold = 6;    // 0% fail at or below this

    [Header("Coin Prefab")]
    [SerializeField] private GameObject coinPrefab;

    [Header("Coins Controller")]
    [SerializeField] private PlayerCoinsController coinsController;

    // Coroutine parameters
    private readonly float _dropDuration = 0.3f;
    private readonly float _bounceSpinSpeed = 720f;   // degrees/sec while bouncing off
    private readonly float _bounceFallDistance = 200f;
    private readonly float _bounceDuration = 0.4f;

    private readonly List<List<GameObject>> _stackCoins = new();

    // Calculates total coins in all stacks
    private int TotalCoins
    {
        get
        {
            int total = 0;
            foreach (var stack in _stackCoins) total += stack.Count;
            return total;
        }
    }

    private void Awake()
    {
        for (int i = 0; i < stacks.Length; i++)
            _stackCoins.Add(new List<GameObject>());
    }

    /// <summary>
    /// Adds a newly picked up coin to the player purse.
    /// </summary>
    public void AddCoin()
    {
        int stackIndex = PickRandomStack();
        List<GameObject> stack = _stackCoins[stackIndex];

        // Calculate if the coin pickup should fail or not
        float purseFail = GetPurseFailChance();
        float stackFail = GetStackFailChance(stack.Count);
        float failChance = Mathf.Max(purseFail, stackFail);     // Fail percentage of the current coin

        bool success = Random.value >= failChance;

        // Calculate where and in which orientation the coin should spawn and where it needs to land
        float landingY = stack.Count * coinHeight;
        float spawnY = coinHeight * 10f;
        float randomRot = Random.Range(60f, 360f);

        // Spawn new purse coin
        GameObject coin = Instantiate(coinPrefab, stacks[stackIndex]);
        RectTransform rt = coin.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0f, spawnY);
        rt.localEulerAngles = new Vector3(0f, 0f, randomRot);

        if (success)
            stack.Add(coin);

        StartCoroutine(AnimateCoin(rt, spawnY, landingY, success));
    }

    public void RemoveCoin()
    {
        // Pick a non-empty stack according to weights
        float totalWeight = 0f;
        for (int i = 0; i < stacks.Length; i++)
            if (_stackCoins[i].Count > 0)
                totalWeight += stackWeights[i];

        if (totalWeight <= 0f) return;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        int stackIndex = 0;

        for (int i = 0; i < stacks.Length; i++)
        {
            if (_stackCoins[i].Count == 0) continue;
            cumulative += stackWeights[i];
            if (roll < cumulative) { stackIndex = i; break; }
        }

        List<GameObject> stack = _stackCoins[stackIndex];
        GameObject top = stack[^1];
        stack.RemoveAt(stack.Count - 1);
        Destroy(top);
    }

    // Fail chance helpers

    /// <summary>
    /// 0 at or below purseSafeThreshold, 1 at maxCoinsInPurse.
    /// </summary>
    private float GetPurseFailChance()
    {
        int total = TotalCoins;

        if (total >= maxCoinsInPurse) 
            return 1f;
        if (total <= purseSafeThreshold) 
            return 0f;

        return (float)(total - purseSafeThreshold) / (maxCoinsInPurse - purseSafeThreshold);
    }

    /// <summary>
    /// 0 at or below stackSafeThreshold, 1 at maxCoinsPerStack.
    /// </summary>
    private float GetStackFailChance(int currentCount)
    {
        if (currentCount >= maxCoinsPerStack) 
            return 1f;
        if (currentCount <= stackSafeThreshold) 
            return 0f;

        return (float)(currentCount - stackSafeThreshold) / (maxCoinsPerStack - stackSafeThreshold);
    }

    // Stack picking

    /// <summary>
    /// Picks a random coin stack according to given weights
    /// </summary>
    /// <returns></returns>
    private int PickRandomStack()
    {
        float totalWeight = 0f;
        foreach (float w in stackWeights) totalWeight += w;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < stacks.Length; i++)
        {
            cumulative += stackWeights[i];
            if (roll < cumulative) 
                return i;
        }

        return stacks.Length - 1; // Fallback
    }

    // Animation

    private IEnumerator AnimateCoin(RectTransform rt, float fromY, float toY, bool success)
    {
        float startRot = rt.localEulerAngles.z;

        float time = 0f;
        while (time < _dropDuration)
        {
            if (rt == null)
                yield break;

            float t = Mathf.Clamp01(time / _dropDuration);

            rt.anchoredPosition = new Vector2(0f, Mathf.Lerp(fromY, toY, t * t));
            rt.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(startRot, 0f, t));

            time += Time.deltaTime;
            yield return null;
        }

        rt.anchoredPosition = new Vector2(0f, toY);
        rt.localEulerAngles = Vector3.zero;

        if (success)
            yield break;

        // Drop was not successful

        // Random horizontal direction, spin follows the direction
        float horizontalJitter = Random.Range(-0.5f, 0.5f);
        float spinDir = horizontalJitter < 0 ? 1f : -1f;

        Vector2 startPos = rt.anchoredPosition;
        float bounceRot = 0f;

        time = 0f;
        while (time < _bounceDuration)
        {
            if (rt == null)
                yield break;

            float t = Mathf.Clamp01(time / _bounceDuration);

            float x = horizontalJitter * _bounceFallDistance * t;
            float y = -_bounceFallDistance * t * t;
            rt.anchoredPosition = startPos + new Vector2(x, y);

            bounceRot += spinDir * _bounceSpinSpeed * Time.deltaTime;
            rt.localEulerAngles = new Vector3(0f, 0f, bounceRot);

            time += Time.deltaTime;
            yield return null;
        }

        coinsController.DropCoin(removeFromPurse: false);
        Destroy(rt.gameObject);
    }
}