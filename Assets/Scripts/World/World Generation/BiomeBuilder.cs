using System.Collections.Generic;
using UnityEngine;

public class BiomeBuilder : MonoBehaviour
{
    // Biome rules
    [SerializeField] private int centerClearingSize = 32;
    [SerializeField] private int endClearingSize = 8;
    [SerializeField] private int minForest = 40;
    [SerializeField] private int maxForest = 60;
    [SerializeField] private int minClearing = 8;
    [SerializeField] private int maxClearing = 12;
    [SerializeField] private int forestEdgeTiles = 6;

    /// <summary>
    /// Assigns a biome to each tile in the world.
    /// </summary>
    public void BuildTileBiomes(ref List<Tile> tiles, int worldTiles)
    {
        // Clear tile list
        tiles.Clear();

        BiomeType[] tileBiomes = new BiomeType[worldTiles];

        // First and last tiles are always grassland
        for (int i = 0; i < endClearingSize; i++)
        {
            tileBiomes[i] = BiomeType.Grassland;
            tileBiomes[worldTiles - 1 - i] = BiomeType.Grassland;
        }

        // The center tiles are always grassland
        int halfTiles = worldTiles / 2;       // amount of tiles per half (left/right of the 0)
        int centreClearingStart = halfTiles - (centerClearingSize / 2);
        int centreClearingEnd = halfTiles + (centerClearingSize / 2);

        for (int i = centreClearingStart; i < centreClearingEnd; i++)
            tileBiomes[i] = BiomeType.Grassland;

        // Place clearing tiles randomly in the left and right half
        FillBiomesInRange(tileBiomes, endClearingSize, centreClearingStart - 1);
        FillBiomesInRange(tileBiomes, centreClearingEnd, worldTiles - endClearingSize - 1);

        // Convert biomes array into tile list
        for (int i = 0; i < worldTiles; i++)
            tiles.Add(new Tile(tileBiomes[i]));

        // Mark first forest edge tiles after the list is built
        for (int i = 1; i < worldTiles - 1; i++)
        {
            if (tiles[i].biome == BiomeType.ForestEdge && (tiles[i - 1].biome == BiomeType.Grassland || tiles[i + 1].biome == BiomeType.Grassland))
                tiles[i].IsFirstForestEdge = true;
        }
    }

    /// <summary>
    /// Choose biome of each tile inside a given range.
    /// </summary>
    private void FillBiomesInRange(BiomeType[] tileBiomes, int rangeStart, int rangeEnd)
    {
        if (rangeStart >= rangeEnd)
            return;

        int cursor = rangeStart;

        while (true)
        {
            // Choose amount of forest tiles
            int forestTileAmount = Random.Range(minForest, maxForest + 1);
            int forestEnd = Mathf.Min(cursor + forestTileAmount - 1, rangeEnd);
            int forestLength = forestEnd - cursor + 1;

            // Edge count is capped to half the forest length so edges don't overlap
            int edgeCount = Mathf.Min(forestEdgeTiles, forestLength / 2);

            // Begin and finish a forest section with forest edge tiles            
            for (int i = 0; i < edgeCount; i++)
            {
                tileBiomes[i + cursor] = BiomeType.ForestEdge;
                tileBiomes[forestEnd - i] = BiomeType.ForestEdge;
            }

            // Fill interior as Forest
            for (int i = cursor + edgeCount; i < forestEnd - edgeCount; i++)
                tileBiomes[i] = BiomeType.Forest;

            cursor = forestEnd + 1;

            if (cursor > rangeEnd)
                break;      // End of range reached

            // Place a grassland clearing
            int clearingTileAmount = Random.Range(minClearing, maxClearing + 1);
            int clearingEnd = Mathf.Min(cursor + clearingTileAmount - 1, rangeEnd);

            for (int i = cursor; i <= clearingEnd; i++)
                tileBiomes[i] = BiomeType.Clearing;

            cursor = clearingEnd + 1;

            if (cursor > rangeEnd)
                break;      // End of range reached
        }
    }
}
