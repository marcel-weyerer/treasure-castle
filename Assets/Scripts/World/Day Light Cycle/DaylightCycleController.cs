using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SunRotationController : MonoBehaviour
{
    [Header("Sun")]
    [SerializeField] private GameObject sun;
    [SerializeField] private GameObject sunPivot;
    [SerializeField] private float dayMinutes = 5f;

    [Header("Volume")]
    [SerializeField] private Volume volume;

    [Header("Day Phases")]
    [SerializeField] private DayPhase[] phases;

    private float _rotationSpeed;
    private float _currentZ;

    private Bloom _bloom;
    private ColorAdjustments _colorAdjustments;
    private Vignette _vignette;

    void Awake()
    {
        _rotationSpeed = 360f / (dayMinutes * 60f);

        volume.profile = Instantiate(volume.profile);
        volume.profile.TryGet(out _bloom);
        volume.profile.TryGet(out _colorAdjustments);
        volume.profile.TryGet(out _vignette);
    }

    void OnEnable()
    {
        _currentZ = 180f;
        sunPivot.transform.eulerAngles = new Vector3(0f, 180f, _currentZ);
    }

    void Update()
    {
        _currentZ += _rotationSpeed * Time.deltaTime;
        sunPivot.transform.rotation = Quaternion.Euler(0f, 180f, _currentZ);
        sun.transform.rotation = Quaternion.identity;

        UpdateVolume();
    }

    private void UpdateVolume()
    {
        if (phases == null || phases.Length == 0)
            return;

        float angle = Mathf.Repeat(_currentZ, 360f);

        // Shift all angles so they are strictly ascending from the first phase
        float origin = phases[0].sunAngle;
        float shiftedAngle = Mathf.Repeat(angle - origin, 360f);

        DayPhase from = phases[^1];
        DayPhase to   = phases[0];

        for (int i = 1; i < phases.Length; i++)
        {
            float shiftedPhaseAngle = Mathf.Repeat(phases[i].sunAngle - origin, 360f);

            if (shiftedAngle < shiftedPhaseAngle)
            {
                from = phases[i - 1];
                to   = phases[i];

                float shiftedFromAngle = Mathf.Repeat(phases[i - 1].sunAngle - origin, 360f);
                float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(shiftedFromAngle, shiftedPhaseAngle, shiftedAngle));
                ApplyPhase(from, to, t);
                return;
            }
        }

        // Past the last phase — interpolate between last and first
        float lastShifted = Mathf.Repeat(phases[^1].sunAngle - origin, 360f);
        float tWrap = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(lastShifted, 360f, shiftedAngle));
        ApplyPhase(phases[^1], phases[0], tWrap);
    }

    private void ApplyPhase(DayPhase from, DayPhase to, float t)
    {
        if (_colorAdjustments != null)
        {
            _colorAdjustments.postExposure.value = Mathf.Lerp(from.postExposure, to.postExposure, t);
            _colorAdjustments.colorFilter.value  = Color.Lerp(from.colorFilter,  to.colorFilter,  t);
            _colorAdjustments.saturation.value   = Mathf.Lerp(from.saturation,   to.saturation,   t);
        }

        if (_bloom != null)
            _bloom.intensity.value = Mathf.Lerp(from.bloomIntensity, to.bloomIntensity, t);

        if (_vignette != null)
            _vignette.intensity.value = Mathf.Lerp(from.vignetteIntensity, to.vignetteIntensity, t);
    }
}