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
    private TileBase[] groundTiles;
    [SerializeField]
    private TileBase[] leftEndTiles;
    [SerializeField]
    private TileBase[] rightEndTiles;
    [SerializeField]
    private int worldLength = 100;

    [Header("World Props")]
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
        groundTilemap.SetTile(_startCell, leftEndTiles[0]);

        // Fill world with tiles
        for (tileOffset = 1; tileOffset < worldLength - 1; tileOffset++)
        {
            Vector3Int cellPos = new (_startCell.x + tileOffset, _startCell.y, _startCell.z);

            // Choose random groundTile
            int index = Random.Range(0, groundTiles.Length);

            groundTilemap.SetTile(cellPos, groundTiles[index]);
        }

        // Set right end tile
        Vector3Int endCell = new (_startCell.x + tileOffset, _startCell.y, _startCell.z);

        groundTilemap.SetTile(endCell, rightEndTiles[0]);
    }

    private void BuildWorldProps()
    {
        starterChest.transform.position = new (0f, 1f, 0f);
    }

    // Helper functions

    private bool CheckSetup()
    {
        if (groundTilemap == null)
        {
            Debug.LogError("No ground tilemap found.");
            return false;
        }
        if (groundTiles == null || groundTiles.Length == 0)
        {
            Debug.LogError("No end tiles set.");
            return false;
        }
        if (leftEndTiles == null || leftEndTiles.Length == 0)
        {
            Debug.LogError("No end tiles set.");
            return false;
        }
        if (rightEndTiles == null || rightEndTiles.Length == 0)
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
