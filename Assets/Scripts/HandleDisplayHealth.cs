using UnityEngine;
using UnityEngine.UI;

public class HandleDisplayHealth : MonoBehaviour
{
    public Slider slider;
    private AgentBehavior agentBehavior;
    // Start is called before the first frame update
    void Start()
    {
        agentBehavior = GetComponent<AgentBehavior>();
        slider.maxValue = agentBehavior.stats.maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = agentBehavior.GetHealth();
    }
}
