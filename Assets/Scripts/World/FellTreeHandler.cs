using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BiomeConverter : MonoBehaviour
{
    [Header("World Generator")]
    [SerializeField] private WorldGenerator worldGenerator;

    [Header("Ground tiles")]
    [SerializeField] private TileBase grassGroundTile;
    [SerializeField] private TileBase forestGroundTile;

    [Header("Conversion parameters")]
    [SerializeField] private float conversionDelay = 5f;

    [Header("Scanner parameters")]
    [SerializeField] private float scanInterval = 4f;
    [SerializeField] private int tilesPerFrame = 32;
    
    private int _worldTiles;
    private int _startTilePosX;

    // Tracks tiles currently waiting for a delayed conversion,
    // so we don't start duplicate coroutines for the same tile
    private readonly HashSet<int> _pendingConversions = new();

    private void OnEnable()
    {
        TreeInteractable.FellTree += HandleTreeFelling;
    }

    private void OnDisable()
    {
        TreeInteractable.FellTree -= HandleTreeFelling;
    }

    private void Start()
    {
        _worldTiles = worldGenerator.GetWorldTiles();
        _startTilePosX = -(_worldTiles / 2);

        tilesPerFrame = Mathf.RoundToInt(_worldTiles / scanInterval);

        StartCoroutine(ScanWorldCoroutine());
    }

    public void HandleTreeFelling(int treePosX)
    {
        Tile tile = worldGenerator.GetTileAt(treePosX);

        if (tile == null)
            return;

        tile.RemoveTree();
    }

    // World scanner

    /// <summary>
    /// Continuously scans every tile in the world, spread over multiple frames, and schedules a conversion for any tile whose neighbours require it.
    /// </summary>
    private IEnumerator ScanWorldCoroutine()
    {
        while (true)
        {
            for (int i = 0; i < _worldTiles; i++)
            {
                int tilePosX = _startTilePosX + i;
                TryScheduleConversion(tilePosX);

                if (i % tilesPerFrame == 0)
                    yield return null;      // Spread work across frames
            }

            yield return new WaitForSeconds(scanInterval);
        }
    }

    /// <summary>
    /// Checks whether a tile needs converting and schedules the coroutine if so.
    /// Does nothing if a conversion for this tile is already pending.
    /// </summary>
    private void TryScheduleConversion(int tilePosX)
    {
        if (_pendingConversions.Contains(tilePosX))
            return;

        Tile tile = worldGenerator.GetTileAt(tilePosX);
        if (tile == null)
            return;

        BiomeType? targetBiome = GetTargetBiome(tile, tilePosX);
        if (targetBiome == null)
            return;

        _pendingConversions.Add(tilePosX);
        StartCoroutine(ConvertTileCoroutine(tilePosX, tile));
    }

    // Conversion rules

    /// <summary>
    /// Returns the biome a tile should convert to based on its neighbours, or null if no conversion is needed.
    /// </summary>
    private BiomeType? GetTargetBiome(Tile tile, int tilePosX)
    {
        // Rule 1: Grassland spreads into adjacent clearings
        if (tile.biome == BiomeType.Clearing)
        {
            Tile left  = worldGenerator.GetTileAt(tilePosX - 1);
            Tile right = worldGenerator.GetTileAt(tilePosX + 1);

            if (left?.biome == BiomeType.Grassland || right?.biome == BiomeType.Grassland)
                return BiomeType.Grassland;
        }

        // Rule 2: A ForestEdge tile with no tree and IsFirstForestEdge converts to its open neighbour
        if (tile.biome == BiomeType.ForestEdge && !tile.HasTree())
        {
            Tile left  = worldGenerator.GetTileAt(tilePosX - 1);
            Tile right = worldGenerator.GetTileAt(tilePosX + 1);

            bool leftOpen  = left?.biome  == BiomeType.Grassland || left?.biome  == BiomeType.Clearing;
            bool rightOpen = right?.biome == BiomeType.Grassland || right?.biome == BiomeType.Clearing;

            if (leftOpen && rightOpen) 
                return BiomeType.Grassland;
            else if (leftOpen)  
                return left.biome;
            else if (rightOpen) 
                return right.biome;
            else
                return null;
        }

        // Rule 3: A Forest tile promotes to ForestEdge if open land exists within 5 tiles
        if (tile.biome == BiomeType.Forest)
        {
            for (int offset = 1; offset <= 5; offset++)
            {
                Tile leftSearch  = worldGenerator.GetTileAt(tilePosX - offset);
                Tile rightSearch = worldGenerator.GetTileAt(tilePosX + offset);

                bool leftIsOpen  = leftSearch?.biome  == BiomeType.Grassland || leftSearch?.biome  == BiomeType.Clearing;
                bool rightIsOpen = rightSearch?.biome == BiomeType.Grassland || rightSearch?.biome == BiomeType.Clearing;

                if (leftIsOpen || rightIsOpen)
                    return BiomeType.ForestEdge;
            }
        }

        return null;
    }

    // Conversion coroutines

    private IEnumerator ConvertTileCoroutine(int tilePosX, Tile tile)
    {
        yield return new WaitForSeconds(conversionDelay);

        _pendingConversions.Remove(tilePosX);

        // Re-evaluate after the delay if state has changes while waiting
        BiomeType? currentTarget = GetTargetBiome(tile, tilePosX);
        if (currentTarget == null)
            yield break;

        ConvertTile(tile, tilePosX, currentTarget.Value);

        // After converting, update neighbouring forest edge state
        UpdateAdjacentForestEdge(tilePosX - 1);
        UpdateAdjacentForestEdge(tilePosX + 1);
    }

    /// <summary>
    /// Converts a full Forest tile that now borders open land to a ForestEdge tile,
    /// making its tree interactable.
    /// </summary>
    private void PromoteForestTileToEdge(int tilePosX)
    {
        Tile tile = worldGenerator.GetTileAt(tilePosX);
        if (tile == null || tile.biome != BiomeType.Forest)
            return;

        tile.ChangeTileBiome(BiomeType.ForestEdge);
        worldGenerator.SetTileAt(tilePosX, forestGroundTile);
    }

    // Helpers

    public void ConvertTile(Tile tile, int tilePosX, BiomeType biome)
    {
        tile.IsFirstForestEdge = false;
        tile.ChangeTileBiome(biome);
        worldGenerator.SetTileAt(tilePosX, grassGroundTile);
    }

    private void UpdateAdjacentForestEdge(int tilePosX)
    {
        Tile tile = worldGenerator.GetTileAt(tilePosX);
        if (tile == null)
            return;

        if (tile.biome == BiomeType.Forest)
        {
            tile.ChangeTileBiome(BiomeType.ForestEdge);
            tile.IsFirstForestEdge = true;
            worldGenerator.SetTileAt(tilePosX, forestGroundTile);
        }
        else if (tile.biome == BiomeType.ForestEdge)
        {
            tile.IsFirstForestEdge = true;
        }
    }
}