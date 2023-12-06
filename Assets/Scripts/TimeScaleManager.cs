using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleManager : MonoBehaviour
{
    public float timeScale = 1;
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = timeScale;
    }
}
