using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class TaskEat : Node
{
    private AgentBehavior _agentBehavior;
    public TaskEat(AgentBehavior agentBehavior) {
        _agentBehavior = agentBehavior;
    }

    public override NodeState Evaluate() {
        GameObject target = (GameObject)GetData("target");

        target.tag = "Untagged";
        _agentBehavior.Eat(target);

        if (target == null) {
            ClearData("target");
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.RUNNING;
        return state;
    }
}
