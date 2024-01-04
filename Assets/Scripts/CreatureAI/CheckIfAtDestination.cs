using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using UnityEngine.AI;

public class CheckIfAtDestination : Node
{
    private AgentBehavior _agentBehavior;
    public CheckIfAtDestination(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    // Update is called once per frame
    public override NodeState Evaluate()
    {
        state = _agentBehavior.IsAtDestination() ? NodeState.SUCCESS : NodeState.FAILURE;
        return state;
    }
}
