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
        state = _agentBehavior.IsThirsty() ? NodeState.SUCCESS : NodeState.FAILURE;
        return state;
    }
}
