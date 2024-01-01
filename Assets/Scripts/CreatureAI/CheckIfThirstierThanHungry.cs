using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckIfThirstierThanHungry : Node
{
    private AgentBehavior _agentBehavior;
    public CheckIfThirstierThanHungry(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    // Update is called once per frame
    public override NodeState Evaluate()
    {
        state = _agentBehavior.GetThirst() < _agentBehavior.GetHunger() ? NodeState.SUCCESS : NodeState.FAILURE;
        return state;
    }
}
