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
        if (!_agentBehavior.IsHungry()) {
            ClearData("target");
        }
        
        state = _agentBehavior.IsHungry() ? NodeState.SUCCESS : NodeState.FAILURE;
        return state;
    }
}
