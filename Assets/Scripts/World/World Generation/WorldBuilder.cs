using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldBuilder : MonoBehaviour
{
    [Header("World Generation")]
    [SerializeField] private WorldGenerator worldGenerator;

    [Header("World properties")]
    [SerializeField] private Transform forestObjectParent;
    [SerializeField] private Transform grassObjectParent;

    [Header("Grassland tiles")]
    [SerializeField] private TileBase grassGroundTile;
    [SerializeField] private TileBase leftEndTile;
    [SerializeField] private TileBase rightEndTile;
    [SerializeField] private GameObject[] grass;

    [Header("Forest tiles")]
    [SerializeField] private TileBase forestGroundTile;
    [SerializeField] private TileBase[] forestCover;
    [SerializeField] private GameObject[] forestBushes;
    [SerializeField] private GameObject[] forestTrees;
    [SerializeField] private GameObject[] forestBackgroundTrees;
    [SerializeField] private GameObject[] forestBackgroundBushes;  

    [Header("Spawn percentages")]
    [SerializeField] private float grassPercentage = 0.8f;
    [SerializeField] private float forestCoverPercentage = 0.5f;
    [SerializeField] private float bushPercentage = 0.4f;
    [SerializeField] private float treePercentage = 0.6f;
    [SerializeField] private float forestBackTreePercentage = 0.7f;
    [SerializeField] private float forestBackBushPercentage = 0.7f;
    [SerializeField] private float clearBackTreePercentage = 0.3f;
    [SerializeField] private float clearBackBushPercentage = 0.3f;

    /// <summary>
    /// Picks and places the ground tiles according to the distribution of biomes in the world
    /// </summary>
    public void BuildGround(ref Tilemap groundTilemap, int worldTiles, int startTilePosX)
    {
        // Reset tile map
        groundTilemap.ClearAllTiles();

        // Set first and last tile
        groundTilemap.SetTile(new Vector3Int(startTilePosX, 0, 0), leftEndTile);
        groundTilemap.SetTile(new Vector3Int(startTilePosX + worldTiles - 1, 0, 0), rightEndTile);

        for (int i = 1; i < worldTiles - 2; i++)
        {
            // Calculate position of tile
            int tilePosX = startTilePosX + i;
            Tile tile = worldGenerator.GetTileAt(tilePosX);

            // Pick and place the ground tile corresponding with tile biome
            TileBase groundTile;
            TileBase[] groundCover = null;

            // Choose the correct ground tile and ground cover for the biome
            if (tile.biome == BiomeType.Forest || tile.biome == BiomeType.ForestEdge)
            {
                groundTile = forestGroundTile;
                groundCover = forestCover;
            }
            else
            {
                groundTile = grassGroundTile;
            }

            // Place bione specific tile
            groundTilemap.SetTile(new Vector3Int(tilePosX, 0, 0), groundTile);

            // Populate scene with objects corresponding to the given biome
            if (tile.biome == BiomeType.Forest)
            {
                FillForestObjects(ref groundTilemap, groundCover, tilePosX, ref tile);
                FillForestBackground(tilePosX, forestBackBushPercentage, forestBackTreePercentage, ref tile);
                FillWithTrees(tilePosX, false, ref tile);
            }
            else if (tile.biome == BiomeType.ForestEdge)
            {
                FillWithTrees(tilePosX, tile.IsFirstForestEdge, ref tile);
            }
            else if (tile.biome == BiomeType.Clearing) 
            {
                FillForestBackground(tilePosX, clearBackBushPercentage, clearBackTreePercentage, ref tile);
                FillGrasslandObjects(tilePosX, ref tile);
            }
            else
            {
                FillGrasslandObjects(tilePosX, ref tile);
            }
        }
    }

    private void FillForestObjects(ref Tilemap groundTilemap, TileBase[] groundCover, int tilePosX, ref Tile tile)
    {
        // Each tile has a chance of spawning ground cover
        if (Random.value < forestCoverPercentage)
        {
            int index = Random.Range(0, groundCover.Length);
            groundTilemap.SetTile(new Vector3Int(tilePosX, 1, 0), groundCover[index]);
        }

        int sortingOrder = Random.Range(-1, 11);

        // Each tile has a chance of spawning a bush
        if (Random.value < bushPercentage)
        {
            int index = Random.Range(0, forestBushes.Length);
            GameObject bush = Instantiate(forestBushes[index], new Vector3(tilePosX, 1, 0), Quaternion.identity);
            bush.transform.SetParent(forestObjectParent);

            bush.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;

            // Add object to tile
            tile.AddTileObject(bush);
        }
    }

    private void FillForestBackground(int tilePosX, float bushPercentage, float treePercentage, ref Tile tile)
    {
        // Fill with background elements
        int sortingOrder = Random.Range(-17, -2);

        // Each tile has a chance of spawning a background bush
        if (Random.value < bushPercentage)
        {
            int index = Random.Range(0, forestBackgroundBushes.Length);

            // Spawn bush
            Vector3 spawnPos = new(tilePosX, forestBackgroundBushes[index].transform.position.y, -sortingOrder * 2f);
            GameObject bush = Instantiate(forestBackgroundBushes[index], spawnPos, Quaternion.identity);
            bush.transform.SetParent(forestObjectParent);

            // Variante sorting order of trees to prevent flickering
            bush.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;

            // Add object to tile
            tile.AddTileObject(bush);
        }

        // Each tile has a chance of spawning a background tree
        if (Random.value < treePercentage)
        {
            int index = Random.Range(0, forestBackgroundTrees.Length);

            // Spawn tree
            Vector3 spawnPos = new(tilePosX, forestBackgroundTrees[index].transform.position.y, -sortingOrder * 2f);
            GameObject tree = Instantiate(forestBackgroundTrees[index], spawnPos, Quaternion.identity);
            tree.transform.SetParent(forestObjectParent);

            // Variante sorting order of trees to prevent flickering
            tree.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder - 1;

            // Add object to tile
            tile.AddTileObject(tree);
        }
    }

    private void FillWithTrees(int tilePosX, bool guaranteedTree, ref Tile tile)
    {
        int sortingOrder = Random.Range(1, 11);

        // Each tile has a chance of spawning a tree
        if (guaranteedTree || Random.value < treePercentage)
        {
            int treeIndex = Random.Range(0, forestTrees.Length);

            GameObject tree = Instantiate(forestTrees[treeIndex], new Vector3(tilePosX, 1, 0), Quaternion.identity);
            tree.transform.SetParent(forestObjectParent);

            // Scale tree randomly for variation
            float scaleFactor = Random.Range(1.2f, 1.7f);
            tree.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

            // Variante sorting order of trees to prevent flickering
            tree.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;

            // Add tree to tile
            worldGenerator.GetTileAt(tilePosX).AddTree(tree);
        }
    }

    private void FillGrasslandObjects(int tilePosX, ref Tile tile)
    {
        // Each tile has a chance of spawning a grass patch
        if (Random.value < grassPercentage)
        {
            int index = Random.Range(0, grass.Length);

            GameObject grassPatch = Instantiate(grass[index], new Vector3(tilePosX, 1, 0), Quaternion.identity);
            grassPatch.transform.SetParent(grassObjectParent);

            // Add object to tile
            tile.AddTileObject(grassPatch);
        }
    }
}
