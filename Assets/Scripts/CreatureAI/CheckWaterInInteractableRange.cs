using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using Unity.VisualScripting;
using UnityEngine.AI;

public class CheckWaterInInteractableRange : Node
{
    private AgentBehavior _agentBehavior;

    public CheckWaterInInteractableRange(AgentBehavior agentBehavior) {
        _agentBehavior = agentBehavior;
    }

    public override NodeState Evaluate() {
        if (GetData("water") is null) {
            state = NodeState.FAILURE;
            return state;
        }

        Vector3 waterPoint = (Vector3)GetData("water");
        if (_agentBehavior.IsCoordinateInteractable(waterPoint)) {
            state = NodeState.SUCCESS;
            return state;
        }

        if (_agentBehavior.IsAtDestination()) {
            _agentBehavior.BlacklistWaterPoint(waterPoint); // water point is unreachable, hence blacklist it
            ClearData("water");

            state = NodeState.FAILURE;
            return state;
        }


        state = NodeState.FAILURE;
        return state;
    }
}
