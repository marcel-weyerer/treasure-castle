using System;
using UnityEngine;

[Serializable]
public class DayPhase
{
    public string phaseName;
    public float sunAngle;

    [Header("Color Adjustments")]
    public float postExposure;
    public Color colorFilter = Color.white;
    public float saturation = 0f;

    [Header("Bloom")]
    public float bloomIntensity = 0f;

    [Header("Vignette")]
    public float vignetteIntensity = 0f;
}