using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckMateInInteractableRange : Node
{
    private AgentBehavior _agentBehavior;

    public CheckMateInInteractableRange(AgentBehavior agentBehavior) {
        _agentBehavior = agentBehavior;
    }

    public override NodeState Evaluate() {
        GameObject target = (GameObject)GetData("mate");

        if (target == null) {
            state = NodeState.FAILURE;
            return state;
        }

        if (!target.GetComponent<AgentBehavior>().CanMate()) {
            ClearData("mate");
            state = NodeState.FAILURE;
            return state;
        }

        if (_agentBehavior.IsTargetInReproduceRange(target)) {
            state = NodeState.SUCCESS;
            return state;
        }

        if (_agentBehavior.IsAtDestination() && !_agentBehavior.IsTargetInReproduceRange(target)) {
            ClearData("mate");

            state = NodeState.FAILURE;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}
