using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckFoodInConsumeRange : Node
{
    private AgentBehavior _agentBehavior;

    public CheckFoodInConsumeRange(AgentBehavior agentBehavior) {
        _agentBehavior = agentBehavior;
    }

    public override NodeState Evaluate() {
        object t = GetData("target");

        if ((t as Object) == null) {
            state = NodeState.FAILURE;
            return state;
        }

        GameObject target = (GameObject)t;
        if (_agentBehavior.IsTargetInteractable(target)) {
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}
