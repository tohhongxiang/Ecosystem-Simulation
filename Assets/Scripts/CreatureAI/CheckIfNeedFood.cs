using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckIfNeedFood : Node
{
    private AgentBehavior _agentBehavior;
    public CheckIfNeedFood(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    // Update is called once per frame
    public override NodeState Evaluate()
    {
        state = _agentBehavior.GetHunger() == 0 ? NodeState.SUCCESS : NodeState.FAILURE;
        return state;
    }
}
