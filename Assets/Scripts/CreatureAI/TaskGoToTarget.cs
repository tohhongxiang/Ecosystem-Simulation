using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskGoToTarget : Node
{
    private AgentBehavior _agentBehavior;

    // Start is called before the first frame update
    public TaskGoToTarget(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    public override NodeState Evaluate() {
        GameObject g = (GameObject)GetData("target");

        if (g == null || !g.CompareTag(_agentBehavior.foodTag)) { // food is being interacted with by something else
            ClearData("target");

            state = NodeState.FAILURE;
            return state;
        }

        Transform target = g.transform;
        _agentBehavior.Seek(target.position);

        state = NodeState.RUNNING;
        return state;
    }
}
