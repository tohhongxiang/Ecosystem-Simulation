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
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private TMP_Text stateText;

    void Start()
    {
        agentBehavior = GetComponent<AgentBehavior>();

        genderText.text = agentBehavior.stats.gender == Gender.MALE ? "Male" : "Female";

        healthSlider.maxValue = agentBehavior.stats.maxHealth;
        hungerSlider.maxValue = agentBehavior.stats.maxHunger;
        thirstSlider.maxValue = agentBehavior.stats.maxThirst;
        staminaSlider.maxValue = agentBehavior.stats.maxStamina;

        if (stateText != null)
        {
            stateText.text = GetStateText(agentBehavior.GetAgentState());
        }
    }

    void Update()
    {
        matedText.text = agentBehavior.IsJustMatedRecently() ? "Mated" : "Not Mated";

        healthSlider.value = agentBehavior.GetHealth();
        hungerSlider.value = agentBehavior.GetHunger();
        thirstSlider.value = agentBehavior.GetThirst();
        staminaSlider.value = agentBehavior.GetStamina();

        if (stateText != null)
        {
            stateText.text = GetStateText(agentBehavior.GetAgentState());
        }
    }

    private string GetStateText(AgentBehavior.AgentState agentState)
    {
        switch (agentState)
        {
            case AgentBehavior.AgentState.EATING:
                return "Eating";
            case AgentBehavior.AgentState.DONE_EATING:
                return "Done Eating";
            case AgentBehavior.AgentState.DRINKING:
                return "Drinking";
            case AgentBehavior.AgentState.DONE_DRINKING:
                return "Done Drinking";
            case AgentBehavior.AgentState.MATING:
                return "Mating";
            case AgentBehavior.AgentState.DONE_MATING:
                return "Done Mating";
            case AgentBehavior.AgentState.ATTACKING:
                return "Attacking";
            case AgentBehavior.AgentState.DONE_ATTACKING:
                return "Done attacking";
            case AgentBehavior.AgentState.WANDERING:
                return "Wandering";
            case AgentBehavior.AgentState.GOING_TO_FOOD:
                return "To Food";
            case AgentBehavior.AgentState.GOING_TO_WATER:
                return "To Water";
            case AgentBehavior.AgentState.RUNNING:
                return "Running";
            case AgentBehavior.AgentState.DEAD:
                return "Dead";
            default:
                return "Default";
        }
    }
}
