using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckIfHungry : Node
{
    private AgentBehavior _agentBehavior;
    public CheckIfHungry(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    // Update is called once per frame
    public override NodeState Evaluate()
    {
        float currentHunger = _agentBehavior.GetHunger();
        float maxHunger = _agentBehavior.stats.maxHunger;

        state = currentHunger <= 0.5f * maxHunger ? NodeState.SUCCESS : NodeState.FAILURE;
        return state;
    }
}
