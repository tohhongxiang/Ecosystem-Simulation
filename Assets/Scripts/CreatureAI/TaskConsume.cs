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

        target.tag = "Untagged";
        _agentBehavior.Consume(target);

        if (target == null) {
            ClearData("target");
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.RUNNING;
        return state;
    }
}
