using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskGoToWater : Node
{
    private AgentBehavior _agentBehavior;

    // Start is called before the first frame update
    public TaskGoToWater(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    public override NodeState Evaluate()
    {
        if (GetData("water") is null) {
            state = NodeState.FAILURE;
            return state;
        }

        Vector3 g = (Vector3)GetData("water");
        
        _agentBehavior.GoToWater(g);

        state = NodeState.RUNNING;
        return state;
    }
}
