using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerBiomeController : MonoBehaviour
{
    [SerializeField] private WorldGenerator worldGenerator;

    [Header("Post Exposures")]
    [SerializeField] private float forestExposure = -1.5f;
    [SerializeField] private float forestEdgeExposure = -0.5f;

    [Header("Volume")]
    [SerializeField] private Volume volume;

    private BiomeType _currentBiome = BiomeType.Grassland;
    private ColorAdjustments _colorAdjustments;

    // Coroutine parameters
    private Coroutine _changePostProcessing;
    private readonly float _changeDuration = 3f;
    private float _currentExposure;

    private void Awake()
    {
        volume.profile.TryGet(out _colorAdjustments);

        _currentExposure = 0f;
    }

    private void Start()
    {
        CheckCurrentBiome();
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

            float targetExposure = biome switch
            {
                BiomeType.Forest => forestExposure,
                BiomeType.ForestEdge => forestEdgeExposure,
                _ => 0f,
            };

            if (_changePostProcessing != null)
                StopCoroutine(_changePostProcessing);

            // Get current post exposure value
            _currentExposure = _colorAdjustments.postExposure.value;
            _changePostProcessing = StartCoroutine(ChangePostProcessing(targetExposure));
        }
    }

    private IEnumerator ChangePostProcessing(float targetExposure)
    {
        float time = 0f;
        while (time < _changeDuration)
        {
            float t = time / _changeDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            _colorAdjustments.postExposure.value = Mathf.Lerp(_currentExposure, targetExposure, smoothT);

            time += Time.deltaTime;
            yield return null;
        }

        _colorAdjustments.postExposure.value = targetExposure;
        _changePostProcessing = null;
    }
}
