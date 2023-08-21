using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskConsume : Node
{
    private AgentBehavior _agentBehavior;
    public TaskConsume(AgentBehavior agentBehavior) {
        _agentBehavior = agentBehavior;
    }

    public override NodeState Evaluate() {
        GameObject target = (GameObject)GetData("target");

        _agentBehavior.Consume(target);

        ClearData("target");
        state = NodeState.RUNNING;
        return state;
    }
}
