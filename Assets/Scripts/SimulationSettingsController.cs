using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationSettingsController : MonoBehaviour
{
    public static Dictionary<string, EditableSimulationSettings> settings = new Dictionary<string, EditableSimulationSettings>(){
        { "Deer", new EditableSimulationSettings() },
        { "Bear", new EditableSimulationSettings() }
    };
}

public class EditableSimulationSettings
{
    public int population = 0;
    public float speed = 1.0f;
    public float size = 1.0f;
}
