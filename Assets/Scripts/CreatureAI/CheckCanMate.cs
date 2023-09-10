using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckCanMate : Node
{
    private AgentBehavior _agentBehavior;
    public CheckCanMate(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    public override NodeState Evaluate()
    {
        if (!_agentBehavior.CanMate()) {
            state = NodeState.FAILURE;
            return state;
        } else {
            state = NodeState.SUCCESS;
            return state;
        }
    }
}
