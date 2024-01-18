using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Slider deerPopulationSlider;
    public Slider bearPopulationSlider;
    public Button confirmButton;

    private int deerPopulation = 0;
    private int bearPopulation = 0;

    // Start is called before the first frame update
    void Start()
    {
        deerPopulationSlider.onValueChanged.AddListener(delegate { UpdateDeerPopulation(); });
        bearPopulationSlider.onValueChanged.AddListener(delegate { UpdateBearPopulation(); });
        confirmButton.onClick.AddListener(delegate { Submit(); });
    }

    void UpdateDeerPopulation()
    {
        deerPopulation = (int)deerPopulationSlider.value;
    }

    void UpdateBearPopulation()
    {
        bearPopulation = (int)bearPopulationSlider.value;
    }

    void Submit()
    {
        SimulationSettingsController.settings["Deer"] = deerPopulation;
        SimulationSettingsController.settings["Bear"] = bearPopulation;

        Debug.Log(string.Format("Deer: {0}, Bear: {1}", deerPopulation, bearPopulation));
    }
}
