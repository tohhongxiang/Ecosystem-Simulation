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

        AgentBehavior targetAgentBehavior = target.GetComponent<AgentBehavior>();
        if (!targetAgentBehavior.CanMate() || targetAgentBehavior.GetAgentState() == AgentBehavior.AgentState.MATING) {
            ClearData("mate");
            state = NodeState.FAILURE;
            return state;
        }

        if (_agentBehavior.IsTargetInReproduceRange(target)) {
            state = NodeState.SUCCESS;
            return state;
        }

        if (_agentBehavior.IsAtDestination()) {
            ClearData("mate");

            state = NodeState.FAILURE;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}
