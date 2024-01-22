using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class TerrainObjectGenerator : MonoBehaviour
{
    public string generatorName;
    public event System.Action OnValuesUpdated;
    public abstract void SpawnObjects(Bounds bounds);
    public abstract void ClearObjects();
    public int seed = 0;

    void Start()
    {
        Random.InitState(seed);
    }

#if UNITY_EDITOR

    protected virtual void OnValidate()
    {
        Random.InitState(seed);
        UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
    }

    public void NotifyOfUpdatedValues()
    {
        Random.InitState(seed);
        UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
    }

#endif
}
