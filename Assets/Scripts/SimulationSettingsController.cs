using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationSettingsController : MonoBehaviour
{
    public static Dictionary<string, int> settings = new Dictionary<string, int>(){
        { "Deer", 0 },
        { "Bear", 0 }
    };
}
