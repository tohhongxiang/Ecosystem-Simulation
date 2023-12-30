using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckFoodInAttackRange : Node
{
    private AgentBehavior _agentBehavior;

    public CheckFoodInAttackRange(AgentBehavior agentBehavior) {
        _agentBehavior = agentBehavior;
    }

    public override NodeState Evaluate() {
        GameObject target = (GameObject)GetData("target");

        if (target == null) {
            state = NodeState.FAILURE;
            return state;
        }

        if (_agentBehavior.IsTargetInAttackRange(target)) {
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}
