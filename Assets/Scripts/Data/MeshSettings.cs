using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : UpdatableData
{
    public int chunkSize;
    public int numberOfChunks;
    public float scale;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        chunkSize = Mathf.Max(chunkSize, 1);
        numberOfChunks = Mathf.Max(numberOfChunks, 1);
        base.OnValidate();
    }
#endif
}
