using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckPredatorsInRange : Node
{
    private AgentBehavior _agentBehavior;
    public CheckPredatorsInRange(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    public override NodeState Evaluate()
    {
        GameObject t = (GameObject)GetData("predator");

        if (t == null)
        {
            List<GameObject> predators = _agentBehavior.GetPredatorsInFOVRange();
            if (predators.Count == 0)
            {
                state = NodeState.FAILURE;
                return state;
            }
            
            parent.parent.SetData("predator", predators[0]);
            state = NodeState.SUCCESS;
            return state;
        }

        state = NodeState.SUCCESS;
        return state;
    }
}

