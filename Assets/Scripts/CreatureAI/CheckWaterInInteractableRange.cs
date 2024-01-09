using UnityEngine;

using BehaviorTree;

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

        if (_agentBehavior.IsAtDestination() && !_agentBehavior.IsPathPossible(waterPoint)) {
            Debug.Log("Blacklist water point" + _agentBehavior.gameObject);
            _agentBehavior.BlacklistWaterPoint(waterPoint);
            ClearData("water");
            state = NodeState.FAILURE;
            return state;
        }

        state = NodeState.FAILURE;
        return state;
    }
}
