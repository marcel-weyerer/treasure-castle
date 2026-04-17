using UnityEngine;

public class WorldPropGenerator : MonoBehaviour
{
    [Header("Player properties")]
    [SerializeField] private GameObject player;
    [SerializeField] private float playerOffset = -50f;

    [Header("World props")]
    [SerializeField] private GameObject starterCoin;
    [SerializeField] private int starterCoinsAmount;
    [SerializeField] private GameObject starterCamp;
    [SerializeField] private GameObject[] chests;

    [Header("Parent objects")]
    [SerializeField] private Transform coinParent;

    // Spawn spread parameters
    private readonly float _starterCoinSpread = 2f;
    private readonly float _chestSpread = 10f;  

    /// <summary>
    /// Places all props in the world in specific or random positions
    /// </summary>
    public void BuildWorldProps(int worldTiles, int startTilePosX)
    {
        // Spawn player
        player.transform.position = new Vector3(playerOffset, 2f, 0f);

        // Place chests randomly in the forests
        PlaceChests(worldTiles, startTilePosX);

        // Spawn starter camp in the center
        starterCamp.transform.position = new Vector3(0f, 1f, 0f);

        // Spawn starter coins
        Vector3 coinsSpawnPos = Vector3.Lerp(player.transform.position, starterCamp.transform.position, 0.5f); ;
        for (int i = 0; i < starterCoinsAmount; i++)
        {
            Vector3 coinPos = coinsSpawnPos + new Vector3(Random.Range(-_starterCoinSpread, _starterCoinSpread), 0f, 0f);

            // Spawn coin
            GameObject newCoin = Instantiate(starterCoin, coinPos, Quaternion.identity);
            newCoin.GetComponent<CoinController>().DropCoinEntity();
            newCoin.transform.SetParent(coinParent);
        }
    }

    private void PlaceChests(int worldTiles, int startTilePosX)
    {
        int chestAmount = chests.Length;
        int leftCount = chestAmount / 2;      // Amount of chests in the left half

        // Don't spawn chests near the camp in the center
        int clearZoneHalfWidth = 50;

        // Distribute between world start and clear zone (left side)
        int leftStart = startTilePosX;
        int leftEnd = -clearZoneHalfWidth;

        // Distribute between clear zone and world end (right side)
        int rightStart = clearZoneHalfWidth;
        int rightEnd = startTilePosX + worldTiles;

        PlaceChestsInRange(chests[..leftCount], leftStart, leftEnd);
        PlaceChestsInRange(chests[leftCount..], rightStart, rightEnd);
    }

    private void PlaceChestsInRange(GameObject[] chests, int rangeStart, int rangeEnd)
    {
        int count = chests.Length;
        if (count == 0)
            return;

        int rangeWidth = rangeEnd - rangeStart;
        int spacing = rangeWidth / (count + 1);

        for (int i = 0; i < count; i++)
        {
            int anchorX = rangeStart + (i + 1) * spacing;
            float randomPosX = Random.Range(anchorX - _chestSpread, anchorX + _chestSpread);

            // Clamp so spread never pushes a chest outside its range or into the clear zone
            randomPosX = Mathf.Clamp(randomPosX, rangeStart + _chestSpread, rangeEnd - _chestSpread);

            chests[i].transform.position = new Vector3(randomPosX, 1f, 0f);
        }
    }
}
