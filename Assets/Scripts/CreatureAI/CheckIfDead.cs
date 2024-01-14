using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckIfDead : Node
{
    private AgentBehavior _agentBehavior;
    public CheckIfDead(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    // Update is called once per frame
    public override NodeState Evaluate()
    {
        state = _agentBehavior.CurrentAgentState == AgentBehavior.AgentState.DEAD ? NodeState.SUCCESS : NodeState.FAILURE;
        return state;
    }
}
