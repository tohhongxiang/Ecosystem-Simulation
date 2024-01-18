using System;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Deer Parameters")]
    public Slider deerPopulationSlider;
    public Slider deerSpeedSlider;
    public Slider deerSizeSlider;

    [Header("Bear Parameters")]
    public Slider bearPopulationSlider;
    public Slider bearSpeedSlider;
    public Slider bearSizeSlider;

    [Header("Confirm")]
    public Button confirmButton;

    private int deerPopulation = 0;
    private float deerSpeed = 1.0f;
    private float deerSize = 1.0f;

    private int bearPopulation = 0;
    private float bearSpeed = 1.0f;
    private float bearSize = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        deerPopulationSlider.value = deerPopulation;
        deerPopulationSlider.onValueChanged.AddListener(delegate { UpdateDeerPopulation(); });

        deerSpeedSlider.value = deerSpeed;
        deerSpeedSlider.minValue = AgentStats.minSpeed;
        deerSpeedSlider.maxValue = AgentStats.maxSpeed;
        deerSpeedSlider.onValueChanged.AddListener(delegate { UpdateDeerSpeed(); });

        deerSizeSlider.value = deerSize;
        deerSizeSlider.minValue = AgentStats.minSize;
        deerSizeSlider.maxValue = AgentStats.maxSize;
        deerSizeSlider.onValueChanged.AddListener(delegate { UpdateDeerSize(); });

        bearPopulationSlider.value = bearPopulation;
        bearPopulationSlider.onValueChanged.AddListener(delegate { UpdateBearPopulation(); });

        bearSpeedSlider.value = bearSpeed;
        bearSpeedSlider.minValue = AgentStats.minSpeed;
        bearSpeedSlider.maxValue = AgentStats.maxSpeed;
        bearSpeedSlider.onValueChanged.AddListener(delegate { UpdateBearSpeed(); });

        bearSizeSlider.value = bearSize;
        bearSizeSlider.minValue = AgentStats.minSize;
        bearSizeSlider.maxValue = AgentStats.maxSize;
        bearSizeSlider.onValueChanged.AddListener(delegate { UpdateBearSize(); });

        confirmButton.onClick.AddListener(delegate { Submit(); });
    }

    void UpdateDeerPopulation()
    {
        deerPopulation = (int)deerPopulationSlider.value;
    }

    void UpdateDeerSpeed()
    {
        deerSpeed = (float)Math.Round(deerSpeedSlider.value, 2);
    }

    void UpdateDeerSize()
    {
        deerSize = (float)Math.Round(deerSizeSlider.value, 2);
    }

    void UpdateBearPopulation()
    {
        bearPopulation = (int)bearPopulationSlider.value;
    }

    void UpdateBearSpeed()
    {
        bearSpeed = (float)Math.Round(bearSpeedSlider.value, 2);
    }

    void UpdateBearSize()
    {
        bearSize = (float)Math.Round(bearSizeSlider.value, 2);
    }

    void Submit()
    {
        SimulationSettingsController.settings["Deer"].population = deerPopulation;
        SimulationSettingsController.settings["Deer"].speed = deerSpeed;
        SimulationSettingsController.settings["Deer"].size = deerSize;

        SimulationSettingsController.settings["Bear"].population = bearPopulation;
        SimulationSettingsController.settings["Bear"].speed = bearSpeed;
        SimulationSettingsController.settings["Bear"].size = bearSize;
    }
}
