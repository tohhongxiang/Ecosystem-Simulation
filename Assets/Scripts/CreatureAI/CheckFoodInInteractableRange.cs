using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckFoodInInteractableRange : Node
{
    private AgentBehavior _agentBehavior;

    public CheckFoodInInteractableRange(AgentBehavior agentBehavior) {
        _agentBehavior = agentBehavior;
    }

    public override NodeState Evaluate() {
        GameObject target = (GameObject)GetData("target");

        if (target == null) {
            state = NodeState.FAILURE;
            return state;
        }

        if (_agentBehavior.IsCoordinateInteractable(target.transform.position)) {
            state = NodeState.SUCCESS;
            return state;
        }

        if (_agentBehavior.IsAtDestination() && !_agentBehavior.IsPathPossible(target.transform.position)) {
            Debug.Log("Blacklist food" + _agentBehavior.gameObject);
            _agentBehavior.BlacklistTarget(target);
            ClearData("target");
            state = NodeState.FAILURE;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}
