using System.Collections.Generic;
using UnityEngine;

using BehaviorTree;

public class CheckFoodInFOVRange : Node
{
    private AgentBehavior _agentBehavior;
    public CheckFoodInFOVRange(AgentBehavior _agentBehavior)
    {
        this._agentBehavior = _agentBehavior;
    }

    public override NodeState Evaluate()
    {
        List<GameObject> targetFoods = _agentBehavior.GetFoodInFOVRange();
        if (targetFoods.Count == 0) {
            state = NodeState.FAILURE;
            return state;
        }

        GameObject t = (GameObject)GetData("target");
        if (t == null || Vector3.Distance(t.transform.position, _agentBehavior.transform.position) > Vector3.Distance(targetFoods[0].transform.position, _agentBehavior.transform.position)) {
            parent.parent.parent.parent.SetData("target", targetFoods[0]);
        }

        state = NodeState.SUCCESS;
        return state;
    }
}
