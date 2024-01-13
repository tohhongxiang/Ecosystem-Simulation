using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandleDisplayStats : MonoBehaviour
{
    private AgentBehavior agentBehavior;

    [SerializeField] private TMP_Text genderText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private Slider thirstSlider;
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private TMP_Text pregnancyText;

    void Start()
    {
        agentBehavior = GetComponent<AgentBehavior>();

        genderText.text = agentBehavior.stats.gender == Gender.MALE ? "Male" : "Female";

        healthSlider.maxValue = agentBehavior.MaxHealth;
        hungerSlider.maxValue = agentBehavior.MaxHunger;
        thirstSlider.maxValue = agentBehavior.MaxThirst;
        staminaSlider.maxValue = agentBehavior.MaxStamina;

        if (stateText != null)
        {
            stateText.text = GetStateText(agentBehavior.CurrentAgentState);
        }

        if (pregnancyText != null) {
            pregnancyText.text = GetPregnancyText(agentBehavior.IsPregnant, agentBehavior.IsRecoveringFromBirth);
        }
    }

    void Update()
    {
        healthSlider.value = agentBehavior.Health;
        hungerSlider.value = agentBehavior.Hunger;
        thirstSlider.value = agentBehavior.Thirst;
        staminaSlider.value = agentBehavior.Stamina;

        if (stateText != null)
        {
            stateText.text = GetStateText(agentBehavior.CurrentAgentState);
        }

        if (pregnancyText != null) {
            pregnancyText.text = GetPregnancyText(agentBehavior.IsPregnant, agentBehavior.IsRecoveringFromBirth);
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
            AgentBehavior.AgentState.DEAD => "Dead",
            AgentBehavior.AgentState.RUNNING_FROM_PREDATOR => "Run away",
            AgentBehavior.AgentState.CHASING_PREY => "Chasing prey",
            _ => "Default",
        };
    }

    private string GetPregnancyText(bool isPregnant, bool isRecoveringFromBirth) {
        if (isPregnant) {
            return "Pregnant";
        }

        if (isRecoveringFromBirth) {
            return "Recovering";
        }

        return "";
    }
}
