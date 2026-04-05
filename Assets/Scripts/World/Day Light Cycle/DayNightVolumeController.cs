using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DayNightVolumeController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform sunPivot;
    [SerializeField] private Volume globalVolume;
    [SerializeField] private VolumeProfile defaultProfile;

    [Header("Day Phases")]
    [SerializeField] private DayPhase[] phases;

    private VolumeProfile _runtimeProfile;
    private ColorAdjustments _colorAdjustments;
    private Bloom _bloom;
    private Vignette _vignette;

    private void Awake()
    {
        if (sunPivot == null || globalVolume == null || defaultProfile == null)
        {
            Debug.LogError("Missing sunPivot, globalVolume, or defaultProfile reference.");
            enabled = false;
            return;
        }

        _runtimeProfile = Instantiate(defaultProfile);
        globalVolume.profile = _runtimeProfile;
        globalVolume.weight = 1f;

        if (!_runtimeProfile.TryGet(out _colorAdjustments))
            Debug.LogWarning("ColorAdjustments missing in Volume Profile.");

        if (!_runtimeProfile.TryGet(out _bloom))
            Debug.LogWarning("Bloom missing in Volume Profile.");

        if (!_runtimeProfile.TryGet(out _vignette))
            Debug.LogWarning("Vignette missing in Volume Profile.");

        ForceOverridesOn();
    }

    private void Update()
    {
        if (phases == null || phases.Length < 2)
            return;

        float currentAngle = NormalizeSignedAngle(sunPivot.eulerAngles.z);

        GetSurroundingPhases(currentAngle, out DayPhase fromPhase, out DayPhase toPhase, out float t);
        ApplyInterpolatedValues(fromPhase, toPhase, t);
    }

    private void ForceOverridesOn()
    {
        if (_colorAdjustments != null)
        {
            _colorAdjustments.active = true;
            _colorAdjustments.postExposure.overrideState = true;
            _colorAdjustments.colorFilter.overrideState = true;
            _colorAdjustments.saturation.overrideState = true;
        }

        if (_bloom != null)
        {
            _bloom.active = true;
            _bloom.intensity.overrideState = true;
        }

        if (_vignette != null)
        {
            _vignette.active = true;
            _vignette.intensity.overrideState = true;
        }
    }

    private void ApplyInterpolatedValues(DayPhase a, DayPhase b, float t)
    {
        if (_colorAdjustments != null)
        {
            _colorAdjustments.postExposure.value = Mathf.Lerp(a.postExposure, b.postExposure, t);
            _colorAdjustments.colorFilter.value = Color.Lerp(a.colorFilter, b.colorFilter, t);
            _colorAdjustments.saturation.value = Mathf.Lerp(a.saturation, b.saturation, t);
        }

        if (_bloom != null)
        {
            _bloom.intensity.value = Mathf.Lerp(a.bloomIntensity, b.bloomIntensity, t);
        }

        if (_vignette != null)
        {
            _vignette.intensity.value = Mathf.Lerp(a.vignetteIntensity, b.vignetteIntensity, t);
        }
    }

    private void GetSurroundingPhases(float currentAngle, out DayPhase fromPhase, out DayPhase toPhase, out float t)
    {
        for (int i = 0; i < phases.Length; i++)
        {
            DayPhase current = phases[i];
            DayPhase next = phases[(i + 1) % phases.Length];

            float start = current.sunAngle;
            float end = next.sunAngle;

            if (end < start)
                end += 360f;

            float angle = currentAngle;
            if (angle < start)
                angle += 360f;

            if (angle >= start && angle <= end)
            {
                fromPhase = current;
                toPhase = next;
                t = Mathf.InverseLerp(start, end, angle);
                return;
            }
        }

        fromPhase = phases[0];
        toPhase = phases[1];
        t = 0f;
    }

    private float NormalizeSignedAngle(float angle)
    {
        angle %= 360f;

        if (angle > 180f)
            angle -= 360f;

        if (angle < -180f)
            angle += 360f;

        return angle;
    }
}