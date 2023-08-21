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

        Transform target = g.transform;
        _agentBehavior.Seek(target.position);

        state = NodeState.RUNNING;
        return state;
    }
}
