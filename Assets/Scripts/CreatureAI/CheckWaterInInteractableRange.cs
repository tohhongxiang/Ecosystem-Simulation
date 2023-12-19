using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;
using Unity.VisualScripting;

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

        if (_agentBehavior.IsAtDestination() && _agentBehavior.IsCoordinateInteractable((Vector3)GetData("water"))) {
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}
