using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandleDisplayStats : MonoBehaviour
{
    private AgentBehavior agentBehavior;

    [SerializeField] private TMP_Text genderText;
    [SerializeField] private TMP_Text matedText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider thirstSlider;
    
    void Start()
    {
        agentBehavior = GetComponent<AgentBehavior>();

        genderText.text = agentBehavior.stats.gender == Gender.MALE ? "Male" : "Female";

        healthSlider.maxValue = agentBehavior.stats.maxHealth;
        hungerSlider.maxValue = agentBehavior.stats.maxHunger;
        thirstSlider.maxValue = agentBehavior.stats.maxThirst;
    }

    void Update()
    {
        matedText.text = agentBehavior.IsJustMatedRecently() ? "Mated" : "Not Mated";

        healthSlider.value = agentBehavior.GetHealth();
        hungerSlider.value = agentBehavior.GetHunger();
        thirstSlider.value = agentBehavior.GetThirst();
    }
}
