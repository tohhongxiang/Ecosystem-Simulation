using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckHungerLowerThanThirst : Node
{
    private AgentBehavior _agentBehavior;
    public CheckHungerLowerThanThirst(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    // Update is called once per frame
    public override NodeState Evaluate()
    {
        float currentHunger = _agentBehavior.GetHunger();
        float currentThirst = _agentBehavior.GetThirst();

        state = currentHunger <= currentThirst ? NodeState.SUCCESS : NodeState.FAILURE;
        return state;
    }
}
