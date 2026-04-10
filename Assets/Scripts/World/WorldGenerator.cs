using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject player;

    [Header("World properties")]
    [SerializeField]
    private Tilemap groundTilemap;
    [SerializeField]
    private TileBase groundTile;
    [SerializeField]
    private TileBase leftEndTile;
    [SerializeField]
    private TileBase rightEndTile;
    [SerializeField]
    private int worldLength = 100;

    [Header("World Props")]
    [SerializeField]
    private GameObject starterCoin;
    [SerializeField]
    private int starterCoinsAmount;
    [SerializeField]
    private GameObject starterChest;
    
    private Vector3Int _startCell;

    void Awake()
    {
        _startCell = new (-(worldLength / 2), 0, 0);
    }

    void Start()
    {
        player.SetActive(false);

        GenerateWorld();
    }

    private void GenerateWorld()
    {
        if (!CheckSetup())
            return;

        // Build ground
        BuildGround();

        // Setup world props
        BuildWorldProps();

        // Spawn player
        player.transform.position = new (-20f, 2f, 0f);
        player.SetActive(true);
    }

    // Build methods

    private void BuildGround()
    {
        groundTilemap.ClearAllTiles();

        int tileOffset;

        // Set left end tile
        groundTilemap.SetTile(_startCell, leftEndTile);

        // Fill world with tiles
        for (tileOffset = 1; tileOffset < worldLength - 1; tileOffset++)
        {
            Vector3Int cellPos = new (_startCell.x + tileOffset, _startCell.y, _startCell.z);

            groundTilemap.SetTile(cellPos, groundTile);
        }

        // Set right end tile
        Vector3Int endCell = new (_startCell.x + tileOffset, _startCell.y, _startCell.z);

        groundTilemap.SetTile(endCell, rightEndTile);
    }

    private void BuildWorldProps()
    {
        // Spawn player
        player.transform.position = new (-20f, 2f, 0f);
        player.SetActive(true);
        
        // Spawn starter chest
        starterChest.transform.position = new (0f, 1f, 0f);

        // Spawn starter coins
        Vector3 coinsSpawnPos = Vector3.Lerp(player.transform.position, starterChest.transform.position, 0.5f);
        for (int i = 0; i < starterCoinsAmount; i++)
        {
            GameObject newCoin = Instantiate(starterCoin, coinsSpawnPos, Quaternion.identity);
            CoinController coinController = newCoin.GetComponent<CoinController>();
            coinController.DropCoinEntity();
        }
    }

    // Helper functions

    private bool CheckSetup()
    {
        if (groundTilemap == null)
        {
            Debug.LogError("No ground tilemap found.");
            return false;
        }
        if (groundTile == null)
        {
            Debug.LogError("No ground tile set.");
            return false;
        }
        if (leftEndTile == null)
        {
            Debug.LogError("No end tile set.");
            return false;
        }
        if (rightEndTile == null)
        {
            Debug.LogError("No end tiles set.");
            return false;
        }
        if (worldLength <= 0)
        {
            Debug.LogError("World length can not be zero or negative.");
            return false;
        }
        if (starterChest == null)
        {
            Debug.LogError("Starter chest not found.");
            return false;
        }

        return true;
    }
}
