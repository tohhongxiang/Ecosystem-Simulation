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
        List<GameObject> predators = _agentBehavior.GetPredatorsInFOVRange();
        if (predators.Count == 0)
        {
            state = NodeState.FAILURE;
            return state;
        }

        GameObject t = (GameObject)GetData("predator");
        if (t == null || (_agentBehavior.transform.position - predators[0].transform.position).sqrMagnitude < (_agentBehavior.transform.position - t.transform.position).sqrMagnitude) {
            parent.parent.SetData("predator", predators[0]);
        }
        

        state = NodeState.SUCCESS;
        return state;
    }
}

