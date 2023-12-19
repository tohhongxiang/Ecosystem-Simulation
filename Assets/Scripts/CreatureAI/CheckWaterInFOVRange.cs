using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckWaterInFOVRange : Node
{
    private AgentBehavior _agentBehavior;
    public CheckWaterInFOVRange(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    public override NodeState Evaluate()
    {
        if (GetData("water") == null)
        { // if there is no current target, check if there are valid targets
            List<Vector3> waterLocations = _agentBehavior.GetWaterInFOVRange();
            if (waterLocations.Count == 0)
            {
                state = NodeState.FAILURE;
                return state;
            }
            
            parent.parent.SetData("water", waterLocations[0]);
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.SUCCESS;
        return state;
    }
}
