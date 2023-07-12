using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class TerrainObjectGenerator : MonoBehaviour
{
    public event System.Action OnValuesUpdated;
    public abstract void SpawnObjects(Bounds bounds);
    public abstract void ClearObjects();

#if UNITY_EDITOR

    protected virtual void OnValidate()
    {

        UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;
    }

    public void NotifyOfUpdatedValues()
    {
        UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;
        if (OnValuesUpdated != null)
        {
            OnValuesUpdated();
        }
    }

#endif
}
