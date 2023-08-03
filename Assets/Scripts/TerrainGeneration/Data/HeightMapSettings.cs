using UnityEngine;

[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData
{
    public NoiseParameters noiseParameters;
    public bool useFalloff = false;
    public FalloffParameters falloffParameters;

    public float heightMultiplier = 15;
    public AnimationCurve heightCurve;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        noiseParameters.ValidateValues();
        base.OnValidate();
    }
#endif
}

