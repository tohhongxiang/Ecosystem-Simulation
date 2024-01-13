using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckIfRecovering : Node
{
    private AgentBehavior _agentBehavior;
    public CheckIfRecovering(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    // Update is called once per frame
    public override NodeState Evaluate()
    {
        state = _agentBehavior.IsRecovering ? NodeState.SUCCESS : NodeState.FAILURE;
        return state;
    }
}
