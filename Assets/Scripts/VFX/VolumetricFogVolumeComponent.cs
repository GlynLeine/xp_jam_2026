using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable, VolumeComponentMenu("Volumetric Fog")]
public class VolumetricFogVolumeComponent : VolumeComponent
{
    public ClampedFloatParameter resolutionScale = new ClampedFloatParameter(0.25f, 0f, 1f);
    public MinIntParameter stepCount = new MinIntParameter(100, 0);
    public MinFloatParameter fogDensity = new MinFloatParameter(0.1f, 0f);
    public ColorParameter scatteringCoefficients = new ColorParameter(Color.white);
    public ColorParameter absorptionCoefficients = new ColorParameter(Color.clear);
    public MinFloatParameter lightScale = new MinFloatParameter(1f, 0f);
    public FloatParameter fadeHeight = new FloatParameter(5f);
    public MinFloatParameter fadeDistance = new MinFloatParameter(4f, 0f);
    public FloatParameter minFogHeight = new FloatParameter(0f);
    public MinFloatParameter maxMarchDistance = new MinFloatParameter(100f, 0f);
    public MinFloatParameter jitterDistance = new MinFloatParameter(0.1f, 0f);
    public ClampedFloatParameter nearFieldAggressiveness = new ClampedFloatParameter(0.8f, 0f, 1f);
    public ClampedFloatParameter fogHistoryContribution = new ClampedFloatParameter(0.2f, 0f, 1f);
}
