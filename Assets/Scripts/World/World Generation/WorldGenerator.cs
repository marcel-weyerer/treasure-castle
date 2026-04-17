using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    [Header("Generator Components")]
    [SerializeField] private BiomeBuilder biomeBuilder;
    [SerializeField] private WorldBuilder worldBuilder;
    [SerializeField] private WorldPropGenerator worldPropGenerator;

    [Header("World properties")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private int worldTiles = 640;

    [Header("River")]
    [SerializeField] private WaterFlowController waterController;

    [Header("Player properties")]
    [SerializeField] private GameObject player;

    private List<Tile> _tiles = new();
    private int _startTilePosX;

    void Awake()
    {
        // Ensure worldChunks is even so center tile is always at world x-position 0
        if (worldTiles % 2 != 0)
            worldTiles++;

        _startTilePosX = -(worldTiles / 2);
    }

    void Start()
    {
        player.GetComponent<Rigidbody2D>().simulated = false;
        GenerateWorld();
        player.GetComponent<Rigidbody2D>().simulated = true;
    }

    // World generation

    /// <summary>
    /// Generates the world and fills it with props.
    /// </summary>
    private void GenerateWorld()
    {
        BuildTileBiomes();
        BuildGround();
        BuildWorldProps();
        SetRiverScale();
    }

    private void BuildTileBiomes()
    {
        biomeBuilder.BuildTileBiomes(ref _tiles, worldTiles);
    }

    private void BuildGround()
    {
        worldBuilder.BuildGround(ref groundTilemap, worldTiles, _startTilePosX);
    }

    private void BuildWorldProps()
    {
        worldPropGenerator.BuildWorldProps(worldTiles, _startTilePosX);
    }

    private void SetRiverScale()
    {
        float riverScale = (worldTiles / 10f) + 4f;        // World scale in plane scale plus 2 on either end
        waterController.ScaleRiver(riverScale);
    }

    // Helper methods

    public Tile GetTileAt(int tilePosX)
    {
        int tileIndex = Mathf.FloorToInt(tilePosX - _startTilePosX);

        if (tileIndex < 0 || tileIndex >= _tiles.Count)
            return null;

        return _tiles[tileIndex];
    }

    public BiomeType GetBiomeAt(float posX)
    {
        Tile tile = GetTileAt(Mathf.RoundToInt(posX));
        return tile?.biome ?? BiomeType.Forest;
    }

    public void SetTileAt(int posX, TileBase tileBase)
    {
        groundTilemap.SetTile(new Vector3Int(posX, 0, 0), tileBase);
    }

    // Getters

    public int GetWorldTiles() => worldTiles;
}