using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckThirstLowerThanHunger : Node
{
    private AgentBehavior _agentBehavior;
    public CheckThirstLowerThanHunger(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    // Update is called once per frame
    public override NodeState Evaluate()
    {
        float currentHunger = _agentBehavior.GetHunger();
        float currentThirst = _agentBehavior.GetThirst();

        state = currentThirst <= currentHunger ? NodeState.SUCCESS : NodeState.FAILURE;
        return state;
    }
}
