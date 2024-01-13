using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandleDisplayStats : MonoBehaviour
{
    private AgentBehavior agentBehavior;

    [SerializeField] private TMP_Text genderText;
    [SerializeField] private Slider reproductiveSatisfactionSlider;
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider thirstSlider;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private TMP_Text stateText;

    void Start()
    {
        agentBehavior = GetComponent<AgentBehavior>();

        genderText.text = agentBehavior.stats.gender == Gender.MALE ? "Male" : "Female";

        reproductiveSatisfactionSlider.maxValue = agentBehavior.stats.maxReproductiveSatisfaction;
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
        reproductiveSatisfactionSlider.value = agentBehavior.GetReproductiveSatisfaction();
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
        return agentState switch
        {
            AgentBehavior.AgentState.GOING_TO_FOOD => "To Food",
            AgentBehavior.AgentState.EATING => "Eating",
            AgentBehavior.AgentState.DONE_EATING => "Done Eating",
            AgentBehavior.AgentState.GOING_TO_WATER => "To Water",
            AgentBehavior.AgentState.DRINKING => "Drinking",
            AgentBehavior.AgentState.DONE_DRINKING => "Done Drinking",
            AgentBehavior.AgentState.GOING_TO_MATE => "To Mate",
            AgentBehavior.AgentState.MATING => "Mating",
            AgentBehavior.AgentState.DONE_MATING => "Done Mating",
            AgentBehavior.AgentState.ATTACKING => "Attacking",
            AgentBehavior.AgentState.DONE_ATTACKING => "Done attacking",
            AgentBehavior.AgentState.WANDERING => "Wandering",
            AgentBehavior.AgentState.RUNNING => "Running",
            AgentBehavior.AgentState.DEAD => "Dead",
            _ => "Default",
        };
    }
}
