using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    public BiomeType biome;
    public GameObject tree;
    public List<GameObject> tileObjects = new();

    public bool IsFirstForestEdge { get; set; }

    public Tile(BiomeType biome)
    {
        this.biome = biome;
        tree = null;
        IsFirstForestEdge = false;
    }

    public void AddTree(GameObject tree)
    {
        if (this.tree != null)
            return;

        this.tree = tree;
        tree.GetComponent<TreeInteractable>().SetInteractable(biome == BiomeType.ForestEdge);
    }

    public void RemoveTree()
    {
        if (tree != null)
            tree.SetActive(false);  // Hide it visually

        Object.Destroy(tree, 0.5f); // Destroy after coroutine is done
        tree = null;
    }

    public void AddTileObject(GameObject obj)
    {
        tileObjects.Add(obj);
    }

    public void ChangeTileBiome(BiomeType biome)
    {
        if (this.biome == BiomeType.Forest && biome == BiomeType.ForestEdge)
        {
            tree?.GetComponent<TreeInteractable>().SetInteractable(true);

            // Destroy each tile object with random delay
            foreach (GameObject obj in tileObjects)
                Object.Destroy(obj, Random.Range(0f, 5f));

            tileObjects.Clear();
        }
        else if (this.biome == BiomeType.Clearing && biome == BiomeType.Grassland)
        {
            // Destroy each tile object with random delay
            foreach (GameObject obj in tileObjects)
                Object.Destroy(obj, Random.Range(0f, 5f));

            tileObjects.Clear();
        }

        this.biome = biome;        
    }

    public bool HasTree() => tree != null;
}

public enum BiomeType
{
    Forest,
    ForestEdge,
    Clearing,
    Grassland
}