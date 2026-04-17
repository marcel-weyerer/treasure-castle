using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerChunkController : MonoBehaviour
{
    [SerializeField] private WorldGenerator worldGenerator;

    [Header("Post Exposures")]
    [SerializeField] private float forestExposure = -1.5f;
    [SerializeField] private float forestEdgeExposure = -0.5f;

    [Header("Volume")]
    [SerializeField] private Volume volume;

    private BiomeType _currentBiome = BiomeType.Forest;
    private ColorAdjustments _colorAdjustments;

    private void Awake()
    {
        volume.profile.TryGet(out _colorAdjustments);
    }

    private void Update()
    {
        CheckCurrentBiome();
    }

    private void CheckCurrentBiome()
    {
        BiomeType biome = worldGenerator.GetBiomeAt(transform.position.x);

        if (biome != _currentBiome) 
        {
            _currentBiome = biome;
            
            if (biome == BiomeType.Forest)
                _colorAdjustments.postExposure.value = forestExposure;
            else if (biome == BiomeType.ForestEdge)
                _colorAdjustments.postExposure.value = forestEdgeExposure;
            else
                _colorAdjustments.postExposure.value = 0f;
        }
    }
}
