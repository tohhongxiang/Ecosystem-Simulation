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
        
        // water is too far already
        if (Vector3.Distance(g, _agentBehavior.gameObject.transform.position) > _agentBehavior.stats.fovRange) {
            ClearData("water");
            state = NodeState.FAILURE;
            return state;
        }
        
        _agentBehavior.GoToWater(g);

        state = NodeState.RUNNING;
        return state;
    }
}
