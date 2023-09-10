using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskGoToMate : Node
{
    private AgentBehavior _agentBehavior;

    // Start is called before the first frame update
    public TaskGoToMate(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    public override NodeState Evaluate() {
        GameObject g = (GameObject)GetData("mate");

        if (g == null || !g.GetComponent<AgentBehavior>().CanMate()) { 
            ClearData("mate");

            state = NodeState.FAILURE;
            return state;
        }

        Transform target = g.transform;
        _agentBehavior.Pursue(target.transform);

        state = NodeState.RUNNING;
        return state;
    }
}
