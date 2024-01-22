using UnityEngine;

public class TerrainObject : MonoBehaviour
{
    public delegate void OnDestroyedHandler();
    public event OnDestroyedHandler OnDestroyed;

    void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
}
