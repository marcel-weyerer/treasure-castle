using System.Collections.Generic;
using Cainos.PixelArtPlatformer_VillageProps;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    [Header("World properties")]
    [SerializeField] private Tilemap groundTilemap;

    [Header("Grassland tiles")]
    [SerializeField] private TileBase grassGroundTile;
    [SerializeField] private TileBase leftEndTile;
    [SerializeField] private TileBase rightEndTile;

    [Header("Forest tiles")]
    [SerializeField] private TileBase forestGroundTile;

    [Header("World size (in chunks)")]
    [SerializeField] private int worldChunks = 200;

    [Header("World Props")]
    [SerializeField] private GameObject starterCoin;
    [SerializeField] private int starterCoinsAmount;
    [SerializeField] private GameObject starterCamp;
    [SerializeField] private GameObject[] chests;

    [Header("Player properties")]
    [SerializeField] private GameObject player;
    [SerializeField] private float playerOffset = -50f;

    // Chunk constant
    public const int ChunkSize = 4;

    private readonly List<Chunk> _chunks = new();
    private int _worldTiles;      // total amount of tiles
    private int _startTilePosX;

    // Spread of spwan positions
    private readonly float starterCoinSpread = 2f;
    private readonly float chestSpread = 10f;

    void Awake()
    {
        // Ensure worldChunks is odd so chunk index 0 is always centred
        if (worldChunks % 2 != 0) 
            worldChunks++;

        _worldTiles = worldChunks * ChunkSize;
        _startTilePosX = -(_worldTiles / 2);
    }

    void Start()
    {
        player.SetActive(false);
        GenerateWorld();
    }

    // World generation

    /// <summary>
    /// Generates the world and fills it with props.
    /// </summary>
    private void GenerateWorld()
    {
        if (!CheckSetup()) 
            return;

        BuildChunkLayout();
        BuildGround();
        BuildWorldProps();
    }

    /// <summary>
    /// Assigns a biome to each chunk in the world.
    /// </summary>
    private void BuildChunkLayout()
    {
        // Clear chunks list
        _chunks.Clear();

        int halfChunks = worldChunks / 2;       // amount of chunks per half (left/right of the 0)

        // Range of grassland chunks in the centre
        int centreClearingStart = halfChunks - 2;
        int centreClearingEnd = halfChunks + 1;    // inclusive

        // true = grassland
        bool[] isGrass = new bool[worldChunks];

        // First and last chunk are always grassland
        isGrass[0] = true;
        isGrass[^1] = true;

        // The center 4 chunks are always grassland
        for (int i = centreClearingStart; i <= centreClearingEnd; i++)
            isGrass[i] = true;

        // Place clearing chunks randomly in the left half
        FillRandomClearings(isGrass, 1, centreClearingStart - 1);

        // Place clearing chunks randomly in the right half
        FillRandomClearings(isGrass, centreClearingEnd + 1, worldChunks - 2);

        // Convert bool array into Chunk list
        for (int i = 0; i < worldChunks; i++)
        {
            int tilePosX = _startTilePosX + i * ChunkSize;
            _chunks.Add(new Chunk(tilePosX, isGrass[i] ? BiomeType.Grassland : BiomeType.Forest));
        }
    }

    /// <summary>
    /// Places grassland clearings inside a given range keeping at least 10 and at most 15 forest chunks between any two clearings.
    /// </summary>
    private void FillRandomClearings(bool[] isGrass, int rangeStart, int rangeEnd)
    {
        if (rangeStart >= rangeEnd) 
            return;

        const int minForest = 10;
        const int maxForest = 15;
        const int minGrass  = 1;
        const int maxGrass  = 2;

        int cursor = rangeStart;

        while (true)
        {
            // Choose amount of forest tiles
            int forestChunkAmount = Random.Range(minForest, maxForest + 1);
            cursor += forestChunkAmount;

            if (cursor > rangeEnd) 
                break;      // End of range reached

            // Place a grassland clearing
            int grassChunkAmount = Random.Range(minGrass, maxGrass + 1);
            int clearingEnd = Mathf.Min(cursor + grassChunkAmount - 1, rangeEnd);

            for (int i = cursor; i <= clearingEnd; i++)
                isGrass[i] = true;

            cursor = clearingEnd + 1;

            if (cursor > rangeEnd) 
                break;      // End of range reached
        }
    }

    /// <summary>
    /// Picks and places the ground tiles according to the distribution of biomes in the world
    /// </summary>
    private void BuildGround()
    {
        // Reset tile map
        groundTilemap.ClearAllTiles();

        // Set first and last tile
        groundTilemap.SetTile(new Vector3Int(_startTilePosX, 0, 0), leftEndTile);
        groundTilemap.SetTile(new Vector3Int(_startTilePosX + _worldTiles - 1, 0, 0), rightEndTile);

        for (int tile = 1; tile < _worldTiles - 1; tile++)
        {
            // Calculate position and chunk of tile
            int tilePosX = _startTilePosX + tile;
            Chunk chunk = GetChunkAt(tilePosX);

            // Pick and place the ground tile corresponding with chunk biome
            TileBase groundTile = chunk.biome == BiomeType.Forest ? forestGroundTile : grassGroundTile;
            groundTilemap.SetTile(new Vector3Int(tilePosX, 0, 0), groundTile);
        }
    }

    /// <summary>
    /// Places all props in the world in specific or random positions
    /// </summary>
    private void BuildWorldProps()
    {
        // Spawn player
        player.transform.position = new Vector3(playerOffset, 2f, 0f);
        player.SetActive(true);

        // Place chests randomly in the forests
        PlaceChests();

        // Spawn starter camp
        starterCamp.transform.position  = new Vector3(0f, 1f, 0f);

        // Spawn starter coins
        Vector3 coinsSpawnPos = Vector3.Lerp(player.transform.position, starterCamp.transform.position, 0.5f);;
        for (int i = 0; i < starterCoinsAmount; i++)
        {
            Vector3 coinPos = coinsSpawnPos + new Vector3(Random.Range(-starterCoinSpread, starterCoinSpread), 0f, 0f);

            GameObject newCoin = Instantiate(starterCoin, coinPos, Quaternion.identity);
            newCoin.GetComponent<CoinController>().DropCoinEntity();
        }
    }

    private void PlaceChests()
    {
        int chestAmount = chests.Length;
        int leftCount = chestAmount / 2;      // Amount of chests in the left half

        // Don't spawn chests near the camp in the center
        int clearZoneHalfWidth = (_worldTiles / 4) / 2;

        // Distribute between world start and clear zone (left side)
        int leftStart = _startTilePosX;
        int leftEnd = -clearZoneHalfWidth;

        // Distribute between clear zone and world end (right side)
        int rightStart = clearZoneHalfWidth;
        int rightEnd = _startTilePosX + _worldTiles;

        PlaceChestsInRange(chests[..leftCount],  leftStart,  leftEnd);
        PlaceChestsInRange(chests[leftCount..],  rightStart, rightEnd);
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
            float randomPosX = Random.Range(anchorX - chestSpread, anchorX + chestSpread);

            // Clamp so spread never pushes a chest outside its range or into the clear zone
            randomPosX = Mathf.Clamp(randomPosX, rangeStart + chestSpread, rangeEnd - chestSpread);

            chests[i].transform.position = new Vector3(randomPosX, 1f, 0f);
        }
    }

    /// <summary>
    /// Returns the Chunk that covers tilePosX, or null if out of bounds.
    /// </summary>
    public Chunk GetChunkAt(float tilePosX)
    {
        int chunkIndex = Mathf.FloorToInt((tilePosX - _startTilePosX) / ChunkSize);

        if (chunkIndex < 0 || chunkIndex >= _chunks.Count) 
            return null;

        return _chunks[chunkIndex];
    }

    private bool CheckSetup()
    {
        if (groundTilemap == null) 
        { 
            Debug.LogError("No ground tilemap found.");         
            return false; 
        }
        if (grassGroundTile == null) 
        { 
            Debug.LogError("No grass ground tile set.");        
            return false; 
        }
        if (forestGroundTile == null) 
        { 
            Debug.LogError("No forest ground tile set.");       
            return false; 
        }
        if (leftEndTile == null) 
        { 
            Debug.LogError("No left end tile set.");            
            return false; 
        }
        if (rightEndTile == null) 
        { 
            Debug.LogError("No right end tile set.");           
            return false; 
        }
        if (worldChunks <= 0)   
        { 
            Debug.LogError("World chunks must be positive.");   
            return false; 
        }
        if (chests == null) 
        { 
            Debug.LogError("Starter chest not found.");         
            return false; 
        }

        return true;
    }
}

public enum BiomeType
{
    Forest,
    Grassland
}

[System.Serializable]
public class Chunk
{
    public int startX;        // x position of the first tile in this chunk
    public BiomeType biome;

    public Chunk(int startX, BiomeType biome)
    {
        this.startX = startX;
        this.biome = biome;
    }
}