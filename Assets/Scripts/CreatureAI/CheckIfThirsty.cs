using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckIfThirsty : Node
{
    private AgentBehavior _agentBehavior;
    public CheckIfThirsty(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    // Update is called once per frame
    public override NodeState Evaluate()
    {
        float currentThirst = _agentBehavior.GetThirst();
        float maxThirst = _agentBehavior.stats.maxThirst;

        state = currentThirst <= 0.5f * maxThirst ? NodeState.SUCCESS : NodeState.FAILURE;
        return state;
    }
}
