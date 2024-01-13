using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckIfNeedWater : Node
{
    private AgentBehavior _agentBehavior;
    public CheckIfNeedWater(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    // Update is called once per frame
    public override NodeState Evaluate()
    {
        state = _agentBehavior.Thirst == 0 ? NodeState.SUCCESS : NodeState.FAILURE;
        return state;
    }
}
