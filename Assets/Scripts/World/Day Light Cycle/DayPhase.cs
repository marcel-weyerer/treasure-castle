using System;
using UnityEngine;

[Serializable]
public class DayPhase
{
    public PhaseNames phaseName;
    public float sunAngle;

    // Volume properties

    [Header("Color Adjustments")]
    public Color colorFilter = Color.white;
    public float saturation = 0f;

    [Header("Bloom")]
    public float bloomIntensity = 0f;

    [Header("Vignette")]
    public float vignetteIntensity = 0f;
}

public enum PhaseNames
{
    Dawn,
    Morning,
    Midday,
    Evening,
    Dusk,
    NightStart,
    NightEnd
}