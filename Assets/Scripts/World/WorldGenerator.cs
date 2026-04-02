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
        if (groundTilemap == null)
        {
            Debug.LogError("No ground tilemap found.");
            return;
        }
        if (groundTiles == null || groundTiles.Length == 0)
        {
            Debug.LogError("No end tiles set.");
            return;
        }
        if (leftEndTiles == null || leftEndTiles.Length == 0)
        {
            Debug.LogError("No end tiles set.");
            return;
        }
        if (rightEndTiles == null || rightEndTiles.Length == 0)
        {
            Debug.LogError("No end tiles set.");
            return;
        }
        if (worldLength <= 0)
        {
            Debug.LogError("World length can not be zero or negative.");
            return;
        }

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

        player.SetActive(true);
    }
}
